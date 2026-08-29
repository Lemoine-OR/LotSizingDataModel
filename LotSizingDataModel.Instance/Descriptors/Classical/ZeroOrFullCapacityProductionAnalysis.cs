using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Descriptors.Classical;

/// <summary>
/// Result of testing whether positive production is forced to equal the
/// period production capacity.
/// </summary>
public sealed class ZeroOrFullCapacityProductionAnalysis
{
    internal ZeroOrFullCapacityProductionAnalysis(
        UniversalConditionState state,
        IEnumerable<double> lowerBoundCapacityGaps,
        double effectiveTolerance)
    {
        State = state;
        LowerBoundCapacityGaps =
            lowerBoundCapacityGaps.ToArray();
        EffectiveTolerance = effectiveTolerance;
    }

    public UniversalConditionState State { get; }

    /// <summary>
    /// Gets minimum-lot-size minus capacity for each period.
    /// </summary>
    public IReadOnlyList<double> LowerBoundCapacityGaps { get; }

    public double EffectiveTolerance { get; }

    public bool IsSatisfied =>
        State == UniversalConditionState.Satisfied;

    public UniversalSemanticConditionAssessment
        ToUniversalAssessment() =>
            new(
                UniversalSemanticCondition
                    .ZeroOrFullCapacityProduction,
                State);
}
