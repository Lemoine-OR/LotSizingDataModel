namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Represents the historical Bitran-Yanasse temporal structure
/// alpha/beta/gamma/delta for single-item capacitated lot sizing.
/// </summary>
/// <remarks>
/// The four positions correspond respectively to setup cost, holding cost,
/// production cost and capacity. This historical profile is intentionally
/// separate from the future universal LotSizingDataModel alpha|beta|gamma
/// notation.
/// </remarks>
public sealed class BitranYanasseTemporalProfile
{
    internal BitranYanasseTemporalProfile(
        TemporalPatternAnalysis setupCost,
        TemporalPatternAnalysis holdingCost,
        TemporalPatternAnalysis productionCost,
        TemporalPatternAnalysis capacity)
    {
        SetupCost = setupCost;
        HoldingCost = holdingCost;
        ProductionCost = productionCost;
        Capacity = capacity;
    }

    public TemporalPatternAnalysis SetupCost { get; }
    public TemporalPatternAnalysis HoldingCost { get; }
    public TemporalPatternAnalysis ProductionCost { get; }
    public TemporalPatternAnalysis Capacity { get; }

    /// <summary>
    /// Gets the exact historical alpha/beta/gamma/delta code.
    /// </summary>
    public string HistoricalCode =>
        $"{SetupCost.HistoricalCode}/" +
        $"{HoldingCost.HistoricalCode}/" +
        $"{ProductionCost.HistoricalCode}/" +
        $"{Capacity.HistoricalCode}";
}
