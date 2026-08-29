namespace LotSizingDataModel.Instance.Notation.Matching;

/// <summary>
/// Represents the complete result of matching a typed descriptor against
/// a universal problem specification.
/// </summary>
public sealed class UniversalNotationMatchResult
{
    internal UniversalNotationMatchResult(
        UniversalNotationMatchKind kind,
        UniversalProblemSpecification specification,
        string generatedCanonicalNotation,
        IEnumerable<UniversalNotationMatchIssue> issues)
    {
        Kind = kind;

        Specification =
            specification ??
            throw new ArgumentNullException(nameof(specification));

        GeneratedCanonicalNotation =
            generatedCanonicalNotation ??
            throw new ArgumentNullException(
                nameof(generatedCanonicalNotation));

        Issues =
            (issues ??
             throw new ArgumentNullException(nameof(issues)))
                .ToArray();
    }

    public UniversalNotationMatchKind Kind { get; }
    public UniversalProblemSpecification Specification { get; }

    /// <summary>
    /// Gets the complete canonical notation generated from the descriptor.
    /// </summary>
    public string GeneratedCanonicalNotation { get; }

    public IReadOnlyList<UniversalNotationMatchIssue> Issues { get; }

    public bool IsExact =>
        Kind == UniversalNotationMatchKind.Exact;

    public bool IsCompatible =>
        Kind is
            UniversalNotationMatchKind.Exact or
            UniversalNotationMatchKind.Compatible;

    public bool HasContradiction =>
        Kind == UniversalNotationMatchKind.Contradiction;

    public bool HasIncompleteInformation =>
        Kind == UniversalNotationMatchKind.Incomplete;
}
