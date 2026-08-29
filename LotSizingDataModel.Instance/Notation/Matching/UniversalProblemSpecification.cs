namespace LotSizingDataModel.Instance.Notation.Matching;

/// <summary>
/// Represents a universal notation interpreted as a problem specification.
/// </summary>
/// <remarks>
/// Specification matching uses positive constraints:
/// - explicitly present characteristics are required;
/// - absent beta features and absent optional network modifiers are not
///   interpreted as explicit negations.
///
/// Consequently an instance may be richer than the specification and remain
/// compatible.
/// </remarks>
public sealed class UniversalProblemSpecification
{
    public UniversalProblemSpecification(
        UniversalLotSizingNotation notation)
    {
        Notation =
            notation ??
            throw new ArgumentNullException(nameof(notation));
    }

    public UniversalLotSizingNotation Notation { get; }

    public string CanonicalText =>
        Notation.Render();

    public static UniversalProblemSpecification Parse(
        string text,
        string? schemeVersion = null)
    {
        UniversalLotSizingNotation notation =
            new UniversalNotationParser()
                .Parse(
                    text,
                    schemeVersion);

        return new UniversalProblemSpecification(
            notation);
    }

    public override string ToString() =>
        CanonicalText;
}
