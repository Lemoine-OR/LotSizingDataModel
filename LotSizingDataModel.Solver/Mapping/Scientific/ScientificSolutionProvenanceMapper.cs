using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Formulation.Scientific;
using LotSizingDataModel.Solver.Resolution.Scientific;
using LotSizingDataModel.Solver.Common;

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
                    SolutionScientificProvenance.LegacySchemaVersion,
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
    /// <summary>
    /// Captures current schema-v2 provenance including the scientific
    /// solution-method family and actual native solver backend.
    /// </summary>
    public static SolutionScientificProvenance Apply(
        LotSizingSolution solution,
        ScientificFormulationSelectionResult selection,
        ScientificSolutionMethodDefinition solutionMethod,
        SolverKind solverBackendKind,
        DateTime? capturedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(solutionMethod);

        if (
            solutionMethod.SupportLevel !=
            ScientificSolutionMethodSupportLevel.Executable)
        {
            throw new InvalidOperationException(
                "Only an executable scientific solution method can be " +
                "recorded in current solution provenance.");
        }

        ScientificSolverBackendDefinition? backend =
            ScientificSolverBackendCatalog.Find(
                solverBackendKind);

        if (
            solutionMethod.RequiresMilpBackend &&
            (
                backend is null ||
                !backend.Supports(solutionMethod)
            ))
        {
            throw new InvalidOperationException(
                $"Solver backend '{solverBackendKind}' is not compatible " +
                $"with solution method '{solutionMethod.MethodId}'.");
        }

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
                SolutionMethodId =
                    solutionMethod.MethodId,
                SolutionMethodCategory =
                    solutionMethod.Category.ToString(),
                SolverBackendKind =
                    solverBackendKind.ToString(),
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
