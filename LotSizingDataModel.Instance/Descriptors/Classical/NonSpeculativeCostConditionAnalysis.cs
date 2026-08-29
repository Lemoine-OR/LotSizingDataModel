using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Descriptors.Classical;

/// <summary>
/// Result of the generic adjacent non-speculative production/holding-cost
/// condition analysis.
/// </summary>
public sealed class NonSpeculativeCostConditionAnalysis
{
    internal NonSpeculativeCostConditionAnalysis(
        UniversalConditionState state,
        IEnumerable<double> transformedHoldingMargins,
        double effectiveTolerance)
    {
        State = state;

        TransformedHoldingMargins =
            transformedHoldingMargins.ToArray();

        EffectiveTolerance = effectiveTolerance;
    }

    public UniversalConditionState State { get; }

    /// <summary>
    /// Gets h_t + p_t - p_(t+1) for each adjacent pair.
    /// </summary>
    public IReadOnlyList<double> TransformedHoldingMargins { get; }

    public double EffectiveTolerance { get; }

    public double MinimumMargin =>
        TransformedHoldingMargins.Min();

    public bool IsSatisfied =>
        State == UniversalConditionState.Satisfied;

    public UniversalSemanticConditionAssessment
        ToUniversalAssessment() =>
            new(
                UniversalSemanticCondition
                    .NonSpeculativeProductionHoldingCosts,
                State);
}
