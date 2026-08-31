using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds the physical inventory-flow balance for every
/// item-warehouse inventory relationship and planning period.
/// </summary>
/// <remarks>
/// The variable part of every balance is built through
/// <see cref="InventoryBalanceConstraintBuilder"/>, preserving
/// the corrected centralized sign convention:
/// <code>
/// inventory(t) - inventory(t-1)
/// - variable inflows + variable outflows = fixed inflows.
/// </code>
/// The first-period right-hand side also contains the initial
/// inventory. Scheduled receipts are fixed inflows.
/// Production and supplier receipts are shifted by their lead
/// times. Transport leaves the origin in its decision period and
/// reaches the destination after the lane lead time.
/// Production of a parent item consumes its BOM components in
/// the production-start period.
/// </remarks>
public sealed class InventoryBalanceConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "inventoryBalance";

    /// <summary>
    /// Determines whether inventory balances are required.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.Inventories.Count > 0;
    }

    /// <summary>
    /// Builds all inventory-flow balance constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MathematicalVariable currentInventory =
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory
                            .CreateInventoryKey(
                                MathematicalDecisionCategory.Inventory,
                                inventory.ItemId,
                                inventory.Warehouse,
                                period));

                MathematicalVariable? previousInventory =
                    period > 1
                        ? context.GetVariable(
                            StandardFormulationVariableKeyFactory
                                .CreateInventoryKey(
                                    MathematicalDecisionCategory.Inventory,
                                    inventory.ItemId,
                                    inventory.Warehouse,
                                    period - 1))
                        : inventory.InitialInventoryDecisionMode ==
                            InitialInventoryDecisionMode.VariableDecision
                            ? context.GetVariable(
                                InitialInventoryDecisionDomainKeyFactory.Create(
                                    inventory))
                            : null;

                var balance =
                    new InventoryBalanceConstraintBuilder()
                        .SetCurrentState(currentInventory)
                        .SetPreviousState(previousInventory);

                AddProductionFlows(
                    instance,
                    context,
                    inventory,
                    period,
                    balance,
                    options);

                AddSupplierFlows(
                    instance,
                    context,
                    inventory,
                    period,
                    balance,
                    options);

                AddTransportFlows(
                    instance,
                    context,
                    inventory,
                    period,
                    balance,
                    options);

                AddDistributionOutflows(
                    instance,
                    context,
                    inventory,
                    period,
                    balance);

                double fixedInflow =
                    inventory.ScheduledReceipt?[period] ??
                    0.0;

                if (period == 1 &&
                    inventory.InitialInventoryDecisionMode !=
                        InitialInventoryDecisionMode.FixedParameter &&
                    inventory.InitialInventory != 0.0)
                {
                    throw new InvalidOperationException(
                        "DLS/DLSI initial-stock semantics require the fixed " +
                        "InitialInventory value to be zero.");
                }

                double rightHandSide =
                    fixedInflow +
                    (period == 1 &&
                     inventory.InitialInventoryDecisionMode ==
                        InitialInventoryDecisionMode.FixedParameter
                        ? inventory.InitialInventory
                        : 0.0);

                context.AddConstraint(
                    $"inventoryBalance_i{inventory.ItemId}" +
                    $"_w{inventory.Warehouse.ReferenceId}_t{period}",
                    balance.BuildExpression(),
                    MathematicalConstraintSense.Equal,
                    rightHandSide,
                    description:
                        "Corrected physical inventory-flow " +
                        "balance.");
            }
        }

        return ValueTask.CompletedTask;
    }

    private static void AddProductionFlows(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        Inventory inventory,
        int period,
        InventoryBalanceConstraintBuilder balance,
        StandardLotSizingFormulationOptions options)
    {
        if (inventory.Warehouse.Kind !=
            WarehouseReferenceKind.PlantWarehouse)
        {
            return;
        }

        int plantId =
            inventory.Warehouse.ReferenceId;

        foreach (ProductionRouting routing
                 in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.PlantId != plantId)
            {
                continue;
            }

            if (routing.ItemId == inventory.ItemId)
            {
                int startPeriod =
                    period - routing.LeadTime;

                if (startPeriod >= 1)
                {
                    balance.AddInflow(
                        context.GetVariable(
                            StandardFormulationVariableKeyFactory
                                .CreateProductionKey(
                                    routing.Id,
                                    startPeriod)));
                }
            }

            if (!options.IncludeBillOfMaterials)
            {
                continue;
            }

            foreach (ComponentRequirement requirement
                     in instance.SupplyChain.ComponentRequirements)
            {
                if (requirement.ComponentItemId != inventory.ItemId ||
                    requirement.ParentItemId != routing.ItemId)
                {
                    continue;
                }

                balance.AddOutflow(
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory
                            .CreateProductionKey(
                                routing.Id,
                                period)),
                    requirement.Quantity);
            }
        }
    }

    private static void AddSupplierFlows(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        Inventory inventory,
        int period,
        InventoryBalanceConstraintBuilder balance,
        StandardLotSizingFormulationOptions options)
    {
        if (!options.IncludeProcurement)
        {
            return;
        }

        foreach (SupplierDelivery delivery
                 in instance.SupplyChain.SupplierDeliveries)
        {
            if (delivery.ItemId != inventory.ItemId ||
                !StandardFormulationDomainKeyFactory.AreSameWarehouse(
                    delivery.Warehouse,
                    inventory.Warehouse))
            {
                continue;
            }

            int orderPeriod =
                period - delivery.LeadTime;

            if (orderPeriod < 1)
            {
                continue;
            }

            balance.AddInflow(
                context.GetVariable(
                    StandardFormulationVariableKeyFactory
                        .CreateProcurementKey(
                            delivery.SupplierId,
                            delivery.ItemId,
                            delivery.Warehouse,
                            orderPeriod)));
        }
    }

    private static void AddTransportFlows(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        Inventory inventory,
        int period,
        InventoryBalanceConstraintBuilder balance,
        StandardLotSizingFormulationOptions options)
    {
        if (!options.IncludeTransport)
        {
            return;
        }

        foreach (TransportCharacteristic characteristic
                 in instance.SupplyChain.TransportCharacteristics)
        {
            if (characteristic.ItemId != inventory.ItemId)
            {
                continue;
            }

            TransportResource resource =
                instance.SupplyChain.TransportResources
                    .First(
                        candidate =>
                            candidate.Id ==
                            characteristic.TransportResourceId);

            foreach (TransportLane lane in resource.Lanes)
            {
                if (StandardFormulationDomainKeyFactory.AreSameWarehouse(
                        lane.Origin,
                        inventory.Warehouse))
                {
                    balance.AddOutflow(
                        context.GetVariable(
                            StandardFormulationVariableKeyFactory
                                .CreateTransportKey(
                                    inventory.ItemId,
                                    resource.Id,
                                    lane.Origin,
                                    lane.Destination,
                                    period)));
                }

                if (!StandardFormulationDomainKeyFactory.AreSameWarehouse(
                        lane.Destination,
                        inventory.Warehouse))
                {
                    continue;
                }

                int departurePeriod =
                    period - lane.LeadTime;

                if (departurePeriod < 1)
                {
                    continue;
                }

                balance.AddInflow(
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory
                            .CreateTransportKey(
                                inventory.ItemId,
                                resource.Id,
                                lane.Origin,
                                lane.Destination,
                                departurePeriod)));
            }
        }
    }

    private static void AddDistributionOutflows(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        Inventory inventory,
        int period,
        InventoryBalanceConstraintBuilder balance)
    {
        foreach (DistributionCenterSourcing sourcing
                 in instance.SupplyChain.DistributionCenterSourcings)
        {
            if (sourcing.ItemId != inventory.ItemId ||
                !StandardFormulationDomainKeyFactory.AreSameWarehouse(
                    sourcing.Warehouse,
                    inventory.Warehouse))
            {
                continue;
            }

            balance.AddOutflow(
                context.GetVariable(
                    StandardFormulationVariableKeyFactory
                        .CreateDistributionKey(
                            MathematicalDecisionCategory.Delivery,
                            sourcing.DistributionCenterId,
                            sourcing.ItemId,
                            sourcing.Warehouse,
                            period)));
        }
    }
}
