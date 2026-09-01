namespace LotSizingDataModel.Instance.Historical;

public sealed class HistoricalMappingRule
{
    private readonly string[] _universalTokens;

    public HistoricalMappingRule(
        string ruleId,
        HistoricalClassificationFamily family,
        string historicalToken,
        IEnumerable<string> universalTokens,
        HistoricalMappingConfidence confidence,
        bool allowsInverse,
        string sourceReference,
        string notes = "")
    {
        RuleId = NormalizeRequired(ruleId, nameof(ruleId));
        Family = family;
        HistoricalToken = NormalizeRequired(historicalToken, nameof(historicalToken));
        ArgumentNullException.ThrowIfNull(universalTokens);

        _universalTokens = universalTokens
            .Select(NormalizeToken)
            .Where(token => token.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToArray();

        Confidence = confidence;
        AllowsInverse = allowsInverse;
        SourceReference = NormalizeRequired(sourceReference, nameof(sourceReference));
        Notes = notes?.Trim() ?? string.Empty;
        EnsureValid();
    }

    public string RuleId { get; }
    public HistoricalClassificationFamily Family { get; }
    public string HistoricalToken { get; }
    public IReadOnlyList<string> UniversalTokens => _universalTokens;
    public HistoricalMappingConfidence Confidence { get; }
    public bool AllowsInverse { get; }
    public string SourceReference { get; }
    public string Notes { get; }

    public void EnsureValid()
    {
        if (!Enum.IsDefined(typeof(HistoricalClassificationFamily), Family))
        {
            throw new InvalidOperationException(
                "Historical classification family is invalid.");
        }

        if (!Enum.IsDefined(typeof(HistoricalMappingConfidence), Confidence))
        {
            throw new InvalidOperationException(
                "Historical mapping confidence is invalid.");
        }

        if (Confidence == HistoricalMappingConfidence.SourceOnly &&
            _universalTokens.Length != 0)
        {
            throw new InvalidOperationException(
                "Source-only rules cannot contain universal projections.");
        }

        if (Confidence != HistoricalMappingConfidence.SourceOnly &&
            _universalTokens.Length == 0)
        {
            throw new InvalidOperationException(
                "Projected rules require at least one universal token.");
        }

        if (AllowsInverse &&
            Confidence != HistoricalMappingConfidence.Exact)
        {
            throw new InvalidOperationException(
                "Only exact mappings can participate in inverse detection.");
        }
    }

    internal bool MatchesUniversalTokenSet(
        IEnumerable<string> universalTokens)
    {
        string[] normalized = universalTokens
            .Select(NormalizeToken)
            .Where(token => token.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToArray();

        return _universalTokens.SequenceEqual(
            normalized,
            StringComparer.Ordinal);
    }

    internal static string NormalizeToken(string? value) =>
        value?.Trim() ?? string.Empty;

    private static string NormalizeRequired(
        string? value,
        string parameterName)
    {
        string normalized = NormalizeToken(value);

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "A non-empty historical mapping value is required.",
                parameterName);
        }

        return normalized;
    }
}
