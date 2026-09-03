namespace LotSizingDataModel.Instance.Notation.Lsi;

/// <summary>
/// Immutable scientific compatibility projection for the LSI/1.0 notation.
/// </summary>
/// <remarks>
/// LSI/1.0 is a view over the stable typed descriptor and Universal Notation.
/// It is not an independent source of semantic truth.
/// </remarks>
public sealed class Lsi10Projection
{
    public Lsi10Projection(
        string canonicalText,
        string universalNotationText,
        string legacyProblemFamily,
        IReadOnlyDictionary<string, string> dimensions)
    {
        CanonicalText =
            canonicalText ??
            throw new ArgumentNullException(nameof(canonicalText));

        UniversalNotationText =
            universalNotationText ??
            throw new ArgumentNullException(nameof(universalNotationText));

        LegacyProblemFamily =
            legacyProblemFamily ??
            throw new ArgumentNullException(nameof(legacyProblemFamily));

        Dimensions =
            dimensions ??
            throw new ArgumentNullException(nameof(dimensions));
    }

    public string SchemeVersion => "1.0";

    public string CanonicalText { get; }

    public string UniversalNotationText { get; }

    public string LegacyProblemFamily { get; }

    public IReadOnlyDictionary<string, string> Dimensions { get; }

    public override string ToString() => CanonicalText;
}
