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
/// Builds inventory safety-stock constraints.
/// </summary>
/// <remarks>
/// When a soft safety-stock violation variable exists, the
/// generated constraint is:
/// <code>
/// inventory + violation &gt;= safetyStock.
/// </code>
/// Otherwise the safety-stock bound is hard:
/// <code>
/// inventory &gt;= safetyStock.
/// </code>
/// </remarks>
public sealed class SafetyStockConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "safetyStock";

    /// <summary>
    /// Determines whether safety-stock constraints are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IncludeSafetyStock)
        {
            return false;
        }

        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            if (inventory.SafetyStock is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds safety-stock constraints for every configured
    /// inventory and period.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            if (inventory.SafetyStock is null)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

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

                MathematicalVariable inventoryVariable =
                    context.GetVariable(
                        inventoryKey);

                var expression =
                    new LinearExpressionBuilder()
                        .Add(
                            inventoryVariable);

                if (options.IncludeSafetyStockViolation &&
                    inventory.SafetyStockViolationCost is not null)
                {
                    var violationKeyBuilder =
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory
                                .InventorySafetyStockViolation)
                            .Add(
                                MathematicalDomainKeySegment.Item,
                                inventory.ItemId);

                    StandardFormulationDomainKeyFactory.AddWarehouse(
                        violationKeyBuilder,
                        inventory.Warehouse);

                    string violationKey =
                        violationKeyBuilder
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build();

                    if (context.VariableRegistry.TryGet(
                            violationKey,
                            out MathematicalVariable? violationVariable) &&
                        violationVariable is not null)
                    {
                        expression.Add(
                            violationVariable);
                    }
                }

                AddConstraint(
                    context,
                    $"safetyStock_i{inventory.ItemId}" +
                    $"_w{inventory.Warehouse.ReferenceId}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.GreaterThanOrEqual,
                    inventory.SafetyStock[period],
                    description:
                        "Inventory safety-stock constraint.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
