namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Defines the numerical tolerance policy used for temporal-pattern analysis.
/// </summary>
public sealed class TemporalPatternTolerance
{
    /// <summary>
    /// Gets the default conservative numerical tolerance.
    /// </summary>
    public static TemporalPatternTolerance Default { get; } =
        new(
            absoluteTolerance: 1e-9,
            relativeTolerance: 1e-9);

    /// <summary>
    /// Initializes a tolerance policy.
    /// </summary>
    public TemporalPatternTolerance(
        double absoluteTolerance,
        double relativeTolerance)
    {
        ValidateTolerance(
            absoluteTolerance,
            nameof(absoluteTolerance));

        ValidateTolerance(
            relativeTolerance,
            nameof(relativeTolerance));

        AbsoluteTolerance = absoluteTolerance;
        RelativeTolerance = relativeTolerance;
    }

    public double AbsoluteTolerance { get; }
    public double RelativeTolerance { get; }

    /// <summary>
    /// Computes the effective absolute tolerance for a given value scale.
    /// </summary>
    public double GetEffectiveTolerance(double scale)
    {
        if (!double.IsFinite(scale) || scale < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                scale,
                "The comparison scale must be finite and non-negative.");
        }

        return Math.Max(
            AbsoluteTolerance,
            RelativeTolerance * scale);
    }

    private static void ValidateTolerance(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) || value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A numerical tolerance must be finite and non-negative.");
        }
    }
}
