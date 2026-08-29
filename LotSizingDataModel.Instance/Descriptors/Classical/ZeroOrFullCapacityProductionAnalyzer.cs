using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Descriptors.Classical;

/// <summary>
/// Detects the zero-or-full-capacity production regime from already modeled
/// minimum-lot-size and production-capacity series.
/// </summary>
/// <remarks>
/// If positive production must be at least the period minimum lot and at most
/// the same period capacity, equality of those two parameters forces positive
/// production to full capacity.
/// </remarks>
public sealed class ZeroOrFullCapacityProductionAnalyzer
{
    public ZeroOrFullCapacityProductionAnalysis Analyze(
        IEnumerable<double> minimumLotSizes,
        IEnumerable<double> productionCapacities,
        TemporalPatternTolerance? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(minimumLotSizes);
        ArgumentNullException.ThrowIfNull(productionCapacities);

        double[] lowerBounds =
            minimumLotSizes.ToArray();

        double[] capacities =
            productionCapacities.ToArray();

        if (
            lowerBounds.Length == 0 ||
            lowerBounds.Length != capacities.Length)
        {
            throw new ArgumentException(
                "Minimum-lot-size and capacity series must be non-empty " +
                "and have the same length.");
        }

        ValidateNonNegativeFinite(
            lowerBounds,
            nameof(minimumLotSizes));

        ValidateNonNegativeFinite(
            capacities,
            nameof(productionCapacities));

        TemporalPatternTolerance policy =
            tolerance ??
            TemporalPatternTolerance.Default;

        double scale =
            lowerBounds
                .Concat(capacities)
                .Select(Math.Abs)
                .DefaultIfEmpty(0.0)
                .Max();

        double effectiveTolerance =
            policy.GetEffectiveTolerance(scale);

        double[] gaps =
            new double[lowerBounds.Length];

        bool satisfied = true;

        for (int index = 0; index < lowerBounds.Length; index++)
        {
            gaps[index] =
                lowerBounds[index] -
                capacities[index];

            if (
                capacities[index] <= effectiveTolerance ||
                Math.Abs(gaps[index]) > effectiveTolerance)
            {
                satisfied = false;
            }
        }

        return new ZeroOrFullCapacityProductionAnalysis(
            satisfied
                ? UniversalConditionState.Satisfied
                : UniversalConditionState.NotSatisfied,
            gaps,
            effectiveTolerance);
    }

    private static void ValidateNonNegativeFinite(
        IEnumerable<double> values,
        string parameterName)
    {
        if (
            values.Any(
                value =>
                    !double.IsFinite(value) ||
                    value < 0.0))
        {
            throw new ArgumentException(
                "Production-mode inputs must be finite and non-negative.",
                parameterName);
        }
    }
}
