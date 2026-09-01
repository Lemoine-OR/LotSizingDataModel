namespace LotSizingDataModel.Instance.Historical;

public sealed class HistoricalMappingAuditResult
{
    public HistoricalMappingAuditResult(
        HistoricalClassificationFamily family,
        IEnumerable<string> declaredTokens,
        IEnumerable<string> detectedTokens)
    {
        Family = family;
        DeclaredTokens = Normalize(declaredTokens);
        DetectedTokens = Normalize(detectedTokens);

        DeclaredButNotDetected = DeclaredTokens
            .Except(DetectedTokens, StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        DetectedButNotDeclared = DetectedTokens
            .Except(DeclaredTokens, StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }

    public HistoricalClassificationFamily Family { get; }
    public IReadOnlyList<string> DeclaredTokens { get; }
    public IReadOnlyList<string> DetectedTokens { get; }
    public IReadOnlyList<string> DeclaredButNotDetected { get; }
    public IReadOnlyList<string> DetectedButNotDeclared { get; }

    public bool IsExactMatch =>
        DeclaredButNotDetected.Count == 0 &&
        DetectedButNotDeclared.Count == 0;

    private static string[] Normalize(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return values
            .Select(HistoricalMappingRule.NormalizeToken)
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }
}
