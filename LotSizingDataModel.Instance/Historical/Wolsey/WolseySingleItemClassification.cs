namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Lossless typed representation of Wolsey's PROB-CAP-VAR single-item
/// classification.
/// </summary>
public sealed class WolseySingleItemClassification
{
    private readonly IReadOnlyCollection<WolseyVariant> _variants;

    public WolseySingleItemClassification(
        WolseyProblemVersion problem,
        WolseyCapacityRegime capacity,
        IEnumerable<WolseyVariant>? variants = null)
    {
        Problem = problem;
        Capacity = capacity;

        _variants =
            (variants ?? Array.Empty<WolseyVariant>())
                .Distinct()
                .OrderBy(variant => (int)variant)
                .ToArray();
    }

    public WolseyProblemVersion Problem { get; }
    public WolseyCapacityRegime Capacity { get; }
    public IReadOnlyCollection<WolseyVariant> Variants => _variants;

    /// <summary>
    /// Renders a stable LotSizingDataModel canonical transcription of the
    /// historical PROB-CAP-VAR code.
    /// </summary>
    /// <remarks>
    /// Empty VAR is omitted, as in Wolsey's WW-U example.
    /// Multiple variants are rendered as a set using comma separators.
    /// </remarks>
    public string HistoricalCode
    {
        get
        {
            string core =
                $"{Problem}-{Capacity}";

            if (_variants.Count == 0)
            {
                return core;
            }

            return
                core +
                "-{" +
                string.Join(
                    ",",
                    _variants.Select(RenderVariant)) +
                "}";
        }
    }

    public bool HasVariant(WolseyVariant variant) =>
        _variants.Contains(variant);

    private static string RenderVariant(
        WolseyVariant variant)
    {
        return variant switch
        {
            WolseyVariant.STConstant => "ST(C)",
            WolseyVariant.LBConstant => "LB(C)",
            _ => variant.ToString()
        };
    }

    public override string ToString() =>
        HistoricalCode;
}
