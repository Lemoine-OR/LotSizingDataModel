using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds global additional work-center capacity costs.
/// </summary>
public sealed class WorkCenterAdditionalCapacityCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "workCenterAdditionalCapacityCost";

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
    /// Builds additional work-center capacity cost terms.
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
                .EnumerateWorkCenters(
                    instance.SupplyChain))
        {
            if (entry.WorkCenter.AdditionalCapacity is null)
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
                    entry.WorkCenter.AdditionalCapacity[period];

                if (options.RemoveStructurallyZeroVariables &&
                    double.IsFinite(upperBound) &&
                    upperBound <= options.StructuralZeroTolerance)
                {
                    continue;
                }

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WorkCenterAdditionalCapacity)
                        .Add(
                            MathematicalDomainKeySegment.Plant,
                            entry.PlantId)
                        .Add(
                            MathematicalDomainKeySegment.WorkCenter,
                            entry.WorkCenter.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddCostTerm(
                    context,
                    expressionBuilder,
                    domainKey,
                    entry.WorkCenter.AdditionalCapacityCost?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
