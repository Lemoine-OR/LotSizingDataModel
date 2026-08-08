using System;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical additional-capacity values to work-center
/// capacity decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Expected canonical domain-key format:
/// <code>
/// workCenterAdditionalCapacity|plant=&lt;id&gt;|workCenter=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class WorkCenterAdditionalCapacityDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.WorkCenterAdditionalCapacity;

    /// <summary>
    /// Maps one non-zero additional-capacity value.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed work-center additional-capacity domain key.
    /// </param>
    /// <param name="variableValue">
    /// Additional-capacity value returned by the solver.
    /// </param>
    protected override void MapValue(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        MathematicalDomainKey domainKey,
        MathematicalVariableValue variableValue)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            solution);

        ArgumentNullException.ThrowIfNull(
            domainKey);

        ArgumentNullException.ThrowIfNull(
            variableValue);

        int plantId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Plant);

        int workCenterId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.WorkCenter);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        var workCenter =
            new WorkCenterReference
            {
                PlantId =
                    plantId,

                WorkCenterId =
                    workCenterId
            };

        WorkCenterCapacityDecision? decision =
            solution.WorkCenterCapacityDecisions
                .FirstOrDefault(
                    existing =>
                        existing.Matches(
                            workCenter));

        if (decision is null)
        {
            if (solution.PlanningHorizon <= 0)
            {
                throw new InvalidOperationException(
                    "The target lot-sizing solution must have a " +
                    "strictly positive planning horizon before " +
                    "work-center capacity decisions are mapped.");
            }

            decision =
                new WorkCenterCapacityDecision(
                    workCenter,
                    solution.PlanningHorizon);

            solution.AddWorkCenterCapacityDecision(
                decision);
        }

        decision.SetAdditionalCapacityUsed(
            period,
            variableValue.Value);
    }
}
