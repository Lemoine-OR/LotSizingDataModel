namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Provides stable historical codes for temporal-pattern categories.
/// </summary>
public static class TemporalPatternTypeExtensions
{
    /// <summary>
    /// Gets the Bitran-Yanasse code for a temporal pattern.
    /// </summary>
    public static string ToBitranYanasseCode(
        this TemporalPatternType pattern)
    {
        return pattern switch
        {
            TemporalPatternType.Zero => "Z",
            TemporalPatternType.Constant => "C",
            TemporalPatternType.NonIncreasing => "NI",
            TemporalPatternType.NonDecreasing => "ND",
            TemporalPatternType.General => "G",
            _ => throw new ArgumentOutOfRangeException(
                nameof(pattern),
                pattern,
                "Unknown temporal-pattern type.")
        };
    }
}
