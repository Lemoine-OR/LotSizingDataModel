using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds fixed work-center activation costs.
/// </summary>
public sealed class WorkCenterResourceCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "workCenterResourceCost";

    /// <summary>
    /// Determines whether activation costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeResourceActivation;
    }

    /// <summary>
    /// Builds fixed work-center activation cost terms.
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
            if (entry.WorkCenter.FixedUsageCost is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WorkCenterActivation)
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
                    entry.WorkCenter.FixedUsageCost[period]);
            }
        }

        return ValueTask.CompletedTask;
    }
}
