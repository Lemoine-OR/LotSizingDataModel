using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global warehouse capacity constraints by aggregating
/// all item-specific inventory loads stored at the warehouse.
/// </summary>
public sealed class WarehouseCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "warehouseCapacity";

    /// <summary>
    /// Determines whether at least one physical warehouse is
    /// globally capacitated.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        foreach (var entry in StandardFormulationResourceEnumerator
                     .EnumerateWarehouses(instance.SupplyChain))
        {
            if (entry.Warehouse.CapacityConstraint is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds global warehouse capacity constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (var entry in StandardFormulationResourceEnumerator
                     .EnumerateWarehouses(instance.SupplyChain))
        {
            if (entry.Warehouse.CapacityConstraint is null)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression =
                    new LinearExpressionBuilder();

                foreach (Inventory inventory
                         in instance.SupplyChain.Inventories)
                {
                    if (!StandardFormulationDomainKeyFactory.AreSameWarehouse(
                            inventory.Warehouse,
                            entry.Reference))
                    {
                        continue;
                    }

                    var inventoryKeyBuilder =
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.Inventory)
                            .Add(
                                MathematicalDomainKeySegment.Item,
                                inventory.ItemId);

                    StandardFormulationDomainKeyFactory.AddWarehouse(
                        inventoryKeyBuilder,
                        inventory.Warehouse);

                    string inventoryKey =
                        inventoryKeyBuilder
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build();

                    expression.Add(
                        context.GetVariable(inventoryKey),
                        inventory.UnitCapacityConsumption?[period] ??
                            1.0);

                    double setupTime =
                        inventory.SetupTime?[period] ??
                        0.0;

                    if (setupTime > 0.0)
                    {
                        var setupKeyBuilder =
                            new MathematicalDomainKeyBuilder(
                                MathematicalDecisionCategory.InventorySetup)
                                .Add(
                                    MathematicalDomainKeySegment.Item,
                                    inventory.ItemId);

                        StandardFormulationDomainKeyFactory.AddWarehouse(
                            setupKeyBuilder,
                            inventory.Warehouse);

                        string setupKey =
                            setupKeyBuilder
                                .Add(
                                    MathematicalDomainKeySegment.Period,
                                    period)
                                .Build();

                        if (context.VariableRegistry.TryGet(
                                setupKey,
                                out MathematicalVariable? setupVariable) &&
                            setupVariable is not null)
                        {
                            expression.Add(
                                setupVariable,
                                setupTime);
                        }
                    }
                }

                var additionalKeyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WarehouseAdditionalCapacity);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    additionalKeyBuilder,
                    entry.Reference);

                string additionalKey =
                    additionalKeyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                if (context.VariableRegistry.TryGet(
                        additionalKey,
                        out MathematicalVariable? additionalVariable) &&
                    additionalVariable is not null)
                {
                    expression.Subtract(
                        additionalVariable);
                }

                AddConstraint(
                    context,
                    $"warehouseCapacity_w{entry.Reference.ReferenceId}" +
                    $"_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    entry.Warehouse.CapacityConstraint[period],
                    description:
                        "Global warehouse capacity constraint.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
