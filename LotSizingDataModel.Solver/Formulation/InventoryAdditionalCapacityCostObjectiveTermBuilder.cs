using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds item-specific additional inventory-capacity costs.
/// </summary>
public sealed class InventoryAdditionalCapacityCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "inventoryAdditionalCapacityCost";

    /// <summary>
    /// Determines whether additional-capacity costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeAdditionalCapacity;
    }

    /// <summary>
    /// Builds item-specific additional-capacity cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            Inventory inventory
            in instance.SupplyChain.Inventories)
        {
            if (inventory.AdditionalCapacity is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double upperBound =
                    inventory.AdditionalCapacity[period];

                if (options.RemoveStructurallyZeroVariables &&
                    double.IsFinite(upperBound) &&
                    upperBound <= options.StructuralZeroTolerance)
                {
                    continue;
                }

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

                AddCostTerm(
                    context,
                    expressionBuilder,
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                    inventory.AdditionalCapacityCost?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
