using System;
using System.Linq;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical transport-resource activation values to
/// transport-resource capacity decisions in a lot-sizing
/// solution.
/// </summary>
/// <remarks>
/// Expected canonical domain-key format:
/// <code>
/// transportResourceActivation|transportResource=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class TransportResourceActivationDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.TransportResourceActivation;

    /// <summary>
    /// Maps one non-zero transport-resource activation value.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed transport-resource activation domain key.
    /// </param>
    /// <param name="variableValue">
    /// Activation value returned by the solver.
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

        int transportResourceId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.TransportResource);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        TransportResourceCapacityDecision? decision =
            solution.TransportResourceCapacityDecisions
                .FirstOrDefault(
                    existing =>
                        existing.Matches(
                            transportResourceId));

        if (decision is null)
        {
            if (solution.PlanningHorizon <= 0)
            {
                throw new InvalidOperationException(
                    "The target lot-sizing solution must have a " +
                    "strictly positive planning horizon before " +
                    "transport-resource capacity decisions are " +
                    "mapped.");
            }

            decision =
                new TransportResourceCapacityDecision(
                    transportResourceId,
                    solution.PlanningHorizon);

            solution.AddTransportResourceCapacityDecision(
                decision);
        }

        decision.SetActivated(
            period,
            variableValue.Value >
                ZeroTolerance);
    }
}
