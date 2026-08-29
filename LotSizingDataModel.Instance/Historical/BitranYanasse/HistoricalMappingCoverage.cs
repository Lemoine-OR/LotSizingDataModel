namespace LotSizingDataModel.Instance.Historical.BitranYanasse;

/// <summary>
/// Describes how completely a historical classification can currently be
/// represented by the universal notation scheme.
/// </summary>
public enum HistoricalMappingCoverage
{
    /// <summary>
    /// Every historical dimension is represented by the universal
    /// specification.
    /// </summary>
    Exact,

    /// <summary>
    /// The universal specification represents only a strict subset of the
    /// historical dimensions; the remaining dimensions are preserved
    /// separately and explicitly.
    /// </summary>
    Partial
}
