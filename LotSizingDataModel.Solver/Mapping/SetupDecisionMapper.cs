using System;
using System.Linq;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical setup-variable values to setup activation
/// decisions associated with production routings.
/// </summary>
/// <remarks>
/// Expected canonical domain-key format:
/// <code>
/// setup|routing=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class SetupDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.Setup;

    /// <summary>
    /// Maps one non-zero setup value.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed setup domain key.
    /// </param>
    /// <param name="variableValue">
    /// Setup value returned by the solver.
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

        int routingId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Routing);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        ProductionDecision? decision =
            solution.ProductionDecisions
                .FirstOrDefault(
                    existingDecision =>
                        existingDecision.RoutingId ==
                        routingId);

        if (decision is null)
        {
            if (solution.PlanningHorizon <= 0)
            {
                throw new InvalidOperationException(
                    "The target lot-sizing solution must have a " +
                    "strictly positive planning horizon before " +
                    "setup decisions are mapped.");
            }

            decision =
                new ProductionDecision(
                    routingId,
                    solution.PlanningHorizon);

            solution.AddProductionDecision(
                decision);
        }

        bool isActivated =
            variableValue.Value >
            ZeroTolerance;

        decision.SetSetupActivated(
            period,
            isActivated);
    }
}
