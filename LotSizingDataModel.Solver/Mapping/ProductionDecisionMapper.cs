using System;
using System.Linq;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical production-variable values to production
/// decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Production decisions in <see cref="LotSizingSolution"/> are
/// indexed by production-routing identifier and contain complete
/// planning-horizon time series.
/// 
/// Expected canonical domain-key format:
/// <code>
/// production|routing=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class ProductionDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.Production;

    /// <summary>
    /// Maps one non-zero production quantity.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed production domain key.
    /// </param>
    /// <param name="variableValue">
    /// Production quantity returned by the solver.
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
                    "production decisions are mapped.");
            }

            decision =
                new ProductionDecision(
                    routingId,
                    solution.PlanningHorizon);

            solution.AddProductionDecision(
                decision);
        }

        decision.SetQuantity(
            period,
            variableValue.Value);
    }
}
