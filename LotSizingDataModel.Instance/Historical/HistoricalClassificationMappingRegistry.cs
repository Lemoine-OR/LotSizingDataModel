namespace LotSizingDataModel.Instance.Historical;

public sealed class HistoricalClassificationMappingRegistry
{
    private readonly List<HistoricalMappingRule> _rules = new();

    public HistoricalClassificationMappingRegistry(
        string registryVersion)
    {
        if (string.IsNullOrWhiteSpace(registryVersion))
        {
            throw new ArgumentException(
                "A registry version is required.",
                nameof(registryVersion));
        }

        RegistryVersion = registryVersion.Trim();
    }

    public string RegistryVersion { get; }
    public IReadOnlyList<HistoricalMappingRule> Rules => _rules;

    public void AddRule(HistoricalMappingRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        rule.EnsureValid();

        if (_rules.Any(existing =>
                string.Equals(
                    existing.RuleId,
                    rule.RuleId,
                    StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Rule identifier '{rule.RuleId}' is already used.");
        }

        if (_rules.Any(existing =>
                existing.Family == rule.Family &&
                string.Equals(
                    existing.HistoricalToken,
                    rule.HistoricalToken,
                    StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Token '{rule.HistoricalToken}' is already registered for '{rule.Family}'.");
        }

        _rules.Add(rule);
    }

    public HistoricalMappingRule? FindForward(
        HistoricalClassificationFamily family,
        string historicalToken)
    {
        string normalized =
            HistoricalMappingRule.NormalizeToken(historicalToken);

        if (normalized.Length == 0)
        {
            return null;
        }

        return _rules.SingleOrDefault(rule =>
            rule.Family == family &&
            string.Equals(
                rule.HistoricalToken,
                normalized,
                StringComparison.Ordinal));
    }

    public HistoricalInverseResolution ResolveInverse(
        HistoricalClassificationFamily family,
        IEnumerable<string> universalTokens)
    {
        ArgumentNullException.ThrowIfNull(universalTokens);

        HistoricalMappingRule[] candidates = _rules
            .Where(rule =>
                rule.Family == family &&
                rule.AllowsInverse &&
                rule.Confidence == HistoricalMappingConfidence.Exact &&
                rule.MatchesUniversalTokenSet(universalTokens))
            .OrderBy(rule => rule.RuleId, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
        {
            return new HistoricalInverseResolution(
                HistoricalInverseResolutionStatus.NotFound,
                candidates);
        }

        if (candidates.Length == 1)
        {
            return new HistoricalInverseResolution(
                HistoricalInverseResolutionStatus.Unique,
                candidates);
        }

        return new HistoricalInverseResolution(
            HistoricalInverseResolutionStatus.Ambiguous,
            candidates);
    }

    public IReadOnlyList<HistoricalMappingRule> GetRules(
        HistoricalClassificationFamily family)
    {
        return _rules
            .Where(rule => rule.Family == family)
            .OrderBy(rule => rule.RuleId, StringComparer.Ordinal)
            .ToArray();
    }

    public void EnsureValid()
    {
        foreach (HistoricalMappingRule rule in _rules)
        {
            rule.EnsureValid();
        }

        if (_rules.GroupBy(rule => rule.RuleId, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException(
                "Historical registry rule identifiers must be unique.");
        }
    }
}
