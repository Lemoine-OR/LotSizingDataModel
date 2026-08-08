using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds item-specific inventory capacity constraints.
/// </summary>
/// <remarks>
/// For each inventory relation with an item-specific capacity,
/// the generated load is:
/// <code>
/// unitCapacityConsumption * inventory
/// + setupTime * inventorySetup
/// &lt;= regularCapacity + additionalCapacityUsed.
/// </code>
/// </remarks>
public sealed class InventoryCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "inventoryCapacity";

    /// <summary>
    /// Determines whether item-specific inventory capacities
    /// exist.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            if (inventory.CapacityConstraint is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds item-specific inventory capacity constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            if (inventory.CapacityConstraint is null)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MathematicalVariable inventoryVariable =
                    GetInventoryVariable(
                        context,
                        inventory,
                        period);

                var expression =
                    new LinearExpressionBuilder()
                        .Add(
                            inventoryVariable,
                            inventory.UnitCapacityConsumption?[period] ??
                                1.0);

                AddOptionalSetupLoad(
                    context,
                    expression,
                    inventory,
                    period);

                AddOptionalAdditionalCapacity(
                    context,
                    expression,
                    inventory,
                    period);

                AddConstraint(
                    context,
                    $"inventoryCapacity_i{inventory.ItemId}" +
                    $"_w{inventory.Warehouse.ReferenceId}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    inventory.CapacityConstraint[period],
                    description:
                        "Item-specific inventory capacity " +
                        "constraint.");
            }
        }

        return ValueTask.CompletedTask;
    }

    private static MathematicalVariable GetInventoryVariable(
        MathematicalModelBuildContext context,
        Inventory inventory,
        int period)
    {
        var keyBuilder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Inventory)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    inventory.ItemId);

        StandardFormulationDomainKeyFactory.AddWarehouse(
            keyBuilder,
            inventory.Warehouse);

        return context.GetVariable(
            keyBuilder
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build());
    }

    private static void AddOptionalSetupLoad(
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expression,
        Inventory inventory,
        int period)
    {
        double setupTime =
            inventory.SetupTime?[period] ??
            0.0;

        if (setupTime == 0.0)
        {
            return;
        }

        var keyBuilder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.InventorySetup)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    inventory.ItemId);

        StandardFormulationDomainKeyFactory.AddWarehouse(
            keyBuilder,
            inventory.Warehouse);

        string key =
            keyBuilder
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();

        if (context.VariableRegistry.TryGet(
                key,
                out MathematicalVariable? setupVariable) &&
            setupVariable is not null)
        {
            expression.Add(
                setupVariable,
                setupTime);
        }
    }

    private static void AddOptionalAdditionalCapacity(
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expression,
        Inventory inventory,
        int period)
    {
        var keyBuilder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory
                    .InventoryAdditionalCapacity)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    inventory.ItemId);

        StandardFormulationDomainKeyFactory.AddWarehouse(
            keyBuilder,
            inventory.Warehouse);

        string key =
            keyBuilder
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();

        if (context.VariableRegistry.TryGet(
                key,
                out MathematicalVariable? additionalVariable) &&
            additionalVariable is not null)
        {
            expression.Subtract(
                additionalVariable);
        }
    }
}
