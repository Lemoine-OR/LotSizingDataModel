using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Solver.Mapping.Scientific;

/// <summary>
/// Captures scientific classification/formulation evidence for a generated
/// lot-sizing solution.
/// </summary>
public static class ScientificSolutionProvenanceMapper
{
    public static SolutionScientificProvenance Apply(
        LotSizingSolution solution,
        ScientificFormulationSelectionResult selection,
        DateTime? capturedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(selection);

        if (!selection.IsSuccessful)
        {
            throw new InvalidOperationException(
                "Scientific provenance cannot be captured from an " +
                "unsuccessful formulation selection.");
        }

        ScientificClassificationResult classification =
            selection.Classification;

        if (
            classification.IsBlocked ||
            classification.DetectedNotation is null ||
            classification.PrimaryProblemClass is null)
        {
            throw new InvalidOperationException(
                "Scientific provenance requires an unblocked classification " +
                "with detected notation and a unique primary problem class.");
        }

        ScientificFormulationSelectionCandidate candidate =
            selection.SelectedCandidate!;

        if (
            candidate.Compatibility.Kind !=
            ScientificFormulationCompatibilityKind.Compatible)
        {
            throw new InvalidOperationException(
                "Only a scientifically compatible formulation selection can " +
                "be recorded as solution provenance.");
        }

        MathematicalFormulationScientificProfile profile =
            candidate.Compatibility.Profile ??
            throw new InvalidOperationException(
                "A scientifically compatible selection must have a profile.");

        var provenance =
            new SolutionScientificProvenance
            {
                SchemaVersion =
                    SolutionScientificProvenance.CurrentSchemaVersion,
                NotationSchemeId =
                    UniversalNotationScheme.Id,
                NotationSchemeVersion =
                    classification.DetectedNotation.SchemeVersion,
                DetectedNotation =
                    classification.DetectedNotation.Render(),
                CanonicalProblemClassCode =
                    classification.PrimaryProblemClass.Definition.Code,
                ProblemClassMatchKind =
                    classification.PrimaryProblemClass.Kind.ToString(),
                FormulationId =
                    candidate.Formulation.FormulationId,
                FormulationFamily =
                    profile.FormulationFamily,
                FormulationScientificCompatibility =
                    candidate.Compatibility.Kind.ToString(),
                CapturedAtUtc =
                    (capturedAtUtc ?? DateTime.UtcNow)
                        .ToUniversalTime()
            };

        SolutionScientificProvenanceCodec.Write(
            solution.GenerationMetadata,
            provenance);

        return provenance;
    }
}
