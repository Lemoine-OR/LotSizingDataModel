namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Classifies a numerical time series as Zero, Constant, NonIncreasing,
/// NonDecreasing or General.
/// </summary>
/// <remarks>
/// Canonical specificity is deterministic:
/// Zero > Constant > directional monotonicity > General.
///
/// Numerical comparisons use one effective tolerance computed from the
/// maximum absolute value in the complete series. When tolerance makes a
/// non-constant series simultaneously non-increasing and non-decreasing,
/// the analyzer conservatively returns General instead of choosing an
/// arbitrary direction.
/// </remarks>
public sealed class TemporalPatternAnalyzer
{
    public TemporalPatternAnalysis Analyze(
        IEnumerable<double> values,
        TemporalPatternTolerance? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        double[] series = values.ToArray();

        if (series.Length == 0)
        {
            throw new ArgumentException(
                "Temporal-pattern analysis requires at least one value.",
                nameof(values));
        }

        for (int index = 0; index < series.Length; index++)
        {
            if (!double.IsFinite(series[index]))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(values),
                    series[index],
                    $"Value at zero-based index {index} is not finite.");
            }
        }

        TemporalPatternTolerance policy =
            tolerance ?? TemporalPatternTolerance.Default;

        double maximumAbsoluteValue =
            series.Max(value => Math.Abs(value));

        double epsilon =
            policy.GetEffectiveTolerance(
                maximumAbsoluteValue);

        double minimum = series.Min();
        double maximum = series.Max();

        bool isZero =
            maximumAbsoluteValue <= epsilon;

        bool isConstant =
            maximum - minimum <= epsilon;

        bool isNonIncreasing = true;
        bool isNonDecreasing = true;

        for (int index = 1; index < series.Length; index++)
        {
            double previous = series[index - 1];
            double current = series[index];

            if (current > previous + epsilon)
            {
                isNonIncreasing = false;
            }

            if (current < previous - epsilon)
            {
                isNonDecreasing = false;
            }
        }

        TemporalPatternType pattern;

        if (isZero)
        {
            pattern = TemporalPatternType.Zero;
        }
        else if (isConstant)
        {
            pattern = TemporalPatternType.Constant;
        }
        else if (isNonIncreasing && !isNonDecreasing)
        {
            pattern = TemporalPatternType.NonIncreasing;
        }
        else if (isNonDecreasing && !isNonIncreasing)
        {
            pattern = TemporalPatternType.NonDecreasing;
        }
        else
        {
            pattern = TemporalPatternType.General;
        }

        return new TemporalPatternAnalysis(
            pattern,
            series.Length,
            series[0],
            series[^1],
            minimum,
            maximum,
            epsilon);
    }
}
