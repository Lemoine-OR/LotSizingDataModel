using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Descriptors.Classical;

/// <summary>
/// Tests the generic adjacent non-speculative cost condition
/// h_t + p_t - p_(t+1) >= 0.
/// </summary>
/// <remarks>
/// The caller supplies adjacent unit source/production costs p and holding
/// costs h. There must be one more p value than h values.
///
/// This generic condition is the cost relation used by Wolsey's WW class, but
/// the analyzer itself is independent from Wolsey.
/// </remarks>
public sealed class NonSpeculativeCostConditionAnalyzer
{
    public NonSpeculativeCostConditionAnalysis Analyze(
        IEnumerable<double> adjacentUnitCosts,
        IEnumerable<double> holdingCosts,
        TemporalPatternTolerance? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(adjacentUnitCosts);
        ArgumentNullException.ThrowIfNull(holdingCosts);

        double[] p = adjacentUnitCosts.ToArray();
        double[] h = holdingCosts.ToArray();

        if (h.Length == 0 || p.Length != h.Length + 1)
        {
            throw new ArgumentException(
                "Non-speculative analysis requires at least one holding " +
                "interval and exactly one more unit-cost value than holding " +
                "cost values.");
        }

        ValidateFinite(p, nameof(adjacentUnitCosts));
        ValidateFinite(h, nameof(holdingCosts));

        TemporalPatternTolerance policy =
            tolerance ??
            TemporalPatternTolerance.Default;

        double scale =
            p.Concat(h)
                .Select(Math.Abs)
                .DefaultIfEmpty(0.0)
                .Max();

        double effectiveTolerance =
            policy.GetEffectiveTolerance(scale);

        double[] margins =
            new double[h.Length];

        bool satisfied = true;

        for (int index = 0; index < h.Length; index++)
        {
            margins[index] =
                h[index] +
                p[index] -
                p[index + 1];

            if (margins[index] < -effectiveTolerance)
            {
                satisfied = false;
            }
        }

        return new NonSpeculativeCostConditionAnalysis(
            satisfied
                ? UniversalConditionState.Satisfied
                : UniversalConditionState.NotSatisfied,
            margins,
            effectiveTolerance);
    }

    private static void ValidateFinite(
        IEnumerable<double> values,
        string parameterName)
    {
        if (values.Any(value => !double.IsFinite(value)))
        {
            throw new ArgumentException(
                "Cost-condition inputs must contain only finite values.",
                parameterName);
        }
    }
}
