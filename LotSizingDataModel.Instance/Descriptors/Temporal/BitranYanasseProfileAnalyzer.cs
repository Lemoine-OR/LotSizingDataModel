namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Builds the historical Bitran-Yanasse temporal profile from four explicit
/// period series.
/// </summary>
public sealed class BitranYanasseProfileAnalyzer
{
    private readonly TemporalPatternAnalyzer _patternAnalyzer;

    public BitranYanasseProfileAnalyzer()
        : this(new TemporalPatternAnalyzer())
    {
    }

    public BitranYanasseProfileAnalyzer(
        TemporalPatternAnalyzer patternAnalyzer)
    {
        _patternAnalyzer =
            patternAnalyzer ??
            throw new ArgumentNullException(
                nameof(patternAnalyzer));
    }

    public BitranYanasseTemporalProfile Analyze(
        IEnumerable<double> setupCost,
        IEnumerable<double> holdingCost,
        IEnumerable<double> productionCost,
        IEnumerable<double> capacity,
        TemporalPatternTolerance? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(setupCost);
        ArgumentNullException.ThrowIfNull(holdingCost);
        ArgumentNullException.ThrowIfNull(productionCost);
        ArgumentNullException.ThrowIfNull(capacity);

        return new BitranYanasseTemporalProfile(
            _patternAnalyzer.Analyze(setupCost, tolerance),
            _patternAnalyzer.Analyze(holdingCost, tolerance),
            _patternAnalyzer.Analyze(productionCost, tolerance),
            _patternAnalyzer.Analyze(capacity, tolerance));
    }
}
