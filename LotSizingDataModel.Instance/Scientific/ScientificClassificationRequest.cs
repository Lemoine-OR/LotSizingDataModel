using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Optional inputs to one scientific-classification run.
/// </summary>
public sealed class ScientificClassificationRequest
{
    public ScientificClassificationRequest(
        string? declaredNotation = null,
        UniversalDerivedSemantics? derivedSemantics = null,
        double numericalTolerance =
            LotSizingProblemFeatureExtractor.DefaultNumericalTolerance)
    {
        if (
            !double.IsFinite(numericalTolerance) ||
            numericalTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numericalTolerance),
                numericalTolerance,
                "Numerical tolerance must be finite and non-negative.");
        }

        DeclaredNotation =
            declaredNotation?.Trim() ?? string.Empty;

        DerivedSemantics =
            derivedSemantics ??
            UniversalDerivedSemantics.Empty;

        NumericalTolerance = numericalTolerance;
    }

    /// <summary>
    /// Gets notation declared by a file, author, benchmark or caller.
    /// It is never overwritten by detected notation.
    /// </summary>
    public string DeclaredNotation { get; }

    public bool HasDeclaredNotation =>
        !string.IsNullOrWhiteSpace(DeclaredNotation);

    /// <summary>
    /// Gets explicit detailed analyses supplied by the caller.
    /// </summary>
    public UniversalDerivedSemantics DerivedSemantics { get; }

    public double NumericalTolerance { get; }
}
