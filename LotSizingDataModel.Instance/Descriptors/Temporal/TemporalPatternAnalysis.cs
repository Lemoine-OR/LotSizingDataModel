namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Represents the deterministic analysis of one finite, non-empty time series.
/// </summary>
public sealed class TemporalPatternAnalysis
{
    internal TemporalPatternAnalysis(
        TemporalPatternType pattern,
        int valueCount,
        double firstValue,
        double lastValue,
        double minimumValue,
        double maximumValue,
        double effectiveTolerance)
    {
        Pattern = pattern;
        ValueCount = valueCount;
        FirstValue = firstValue;
        LastValue = lastValue;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
        EffectiveTolerance = effectiveTolerance;
    }

    public TemporalPatternType Pattern { get; }
    public string HistoricalCode =>
        Pattern.ToBitranYanasseCode();

    public int ValueCount { get; }
    public double FirstValue { get; }
    public double LastValue { get; }
    public double MinimumValue { get; }
    public double MaximumValue { get; }
    public double Range => MaximumValue - MinimumValue;
    public double EffectiveTolerance { get; }
}
