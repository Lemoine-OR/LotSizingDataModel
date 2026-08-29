using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Keeps declared notation and its comparison with detected notation separate.
/// </summary>
public sealed class ScientificNotationComparison
{
    internal ScientificNotationComparison(
        string declaredText,
        ScientificNotationComparisonKind kind,
        UniversalProblemSpecification? declaredSpecification = null,
        UniversalNotationMatchResult? match = null)
    {
        DeclaredText = declaredText ?? string.Empty;
        Kind = kind;
        DeclaredSpecification = declaredSpecification;
        Match = match;
    }

    public string DeclaredText { get; }
    public bool HasDeclaredNotation =>
        !string.IsNullOrWhiteSpace(DeclaredText);

    public ScientificNotationComparisonKind Kind { get; }

    public UniversalProblemSpecification? DeclaredSpecification { get; }

    public UniversalNotationMatchResult? Match { get; }

    public string? CanonicalDeclaredNotation =>
        DeclaredSpecification?.CanonicalText;
}
