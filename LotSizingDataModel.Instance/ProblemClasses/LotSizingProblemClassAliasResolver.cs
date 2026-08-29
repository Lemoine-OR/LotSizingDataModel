namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Resolves canonical codes and historical/literature aliases without
/// arbitrarily collapsing ambiguous acronyms.
/// </summary>
public sealed class LotSizingProblemClassAliasResolver
{
    public LotSizingProblemClassAliasResolution Resolve(
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new LotSizingProblemClassAliasResolution(
                query ?? string.Empty,
                Array.Empty<LotSizingProblemClassDefinition>());
        }

        string normalized = query.Trim();

        LotSizingProblemClassDefinition[] matches =
            LotSizingProblemClassCatalog.All
                .Where(
                    definition =>
                        definition.Code.Equals(
                            normalized,
                            StringComparison.OrdinalIgnoreCase) ||
                        definition.Id.ToString().Equals(
                            normalized,
                            StringComparison.OrdinalIgnoreCase) ||
                        definition.Aliases.Any(
                            alias =>
                                alias.Equals(
                                    normalized,
                                    StringComparison.OrdinalIgnoreCase)))
                .ToArray();

        return new LotSizingProblemClassAliasResolution(
            normalized,
            matches);
    }
}
