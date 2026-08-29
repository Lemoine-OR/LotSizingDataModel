namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Result of resolving a literature/project acronym or canonical code.
/// </summary>
public sealed class LotSizingProblemClassAliasResolution
{
    internal LotSizingProblemClassAliasResolution(
        string query,
        IEnumerable<LotSizingProblemClassDefinition> matches)
    {
        Query = query;

        Matches =
            matches
                .DistinctBy(definition => definition.Id)
                .OrderBy(definition => definition.Code, StringComparer.Ordinal)
                .ToArray();

        Kind =
            Matches.Count switch
            {
                0 => LotSizingProblemClassAliasResolutionKind.Unknown,
                1 => LotSizingProblemClassAliasResolutionKind.Unique,
                _ => LotSizingProblemClassAliasResolutionKind.Ambiguous
            };
    }

    public string Query { get; }
    public LotSizingProblemClassAliasResolutionKind Kind { get; }
    public IReadOnlyList<LotSizingProblemClassDefinition> Matches { get; }

    public LotSizingProblemClassDefinition? UniqueMatch =>
        Kind == LotSizingProblemClassAliasResolutionKind.Unique
            ? Matches[0]
            : null;
}
