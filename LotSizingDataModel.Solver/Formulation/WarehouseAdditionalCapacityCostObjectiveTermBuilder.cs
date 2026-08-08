using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds global additional warehouse-capacity costs.
/// </summary>
public sealed class WarehouseAdditionalCapacityCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "warehouseAdditionalCapacityCost";

    /// <summary>
    /// Determines whether these costs are enabled.
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
    /// Builds additional warehouse-capacity cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            var entry
            in StandardFormulationResourceEnumerator
                .EnumerateWarehouses(
                    instance.SupplyChain))
        {
            if (entry.Warehouse.AdditionalCapacity is null)
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
                    entry.Warehouse.AdditionalCapacity[period];

                if (options.RemoveStructurallyZeroVariables &&
                    double.IsFinite(upperBound) &&
                    upperBound <= options.StructuralZeroTolerance)
                {
                    continue;
                }

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WarehouseAdditionalCapacity);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    entry.Reference);

                AddCostTerm(
                    context,
                    expressionBuilder,
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                    entry.Warehouse.AdditionalCapacityCost?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
