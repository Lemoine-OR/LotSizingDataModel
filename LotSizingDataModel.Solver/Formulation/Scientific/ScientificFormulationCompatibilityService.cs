using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Evaluates whether one mathematical formulation has verified scientific
/// coverage for a classified lot-sizing instance.
/// </summary>
public sealed class ScientificFormulationCompatibilityService
{
    public ScientificFormulationCompatibilityResult Assess(
        ScientificClassificationResult classification,
        string formulationId)
    {
        ArgumentNullException.ThrowIfNull(classification);

        if (
            !MathematicalFormulationScientificCatalog.TryGet(
                formulationId,
                out MathematicalFormulationScientificProfile? profile))
        {
            return new ScientificFormulationCompatibilityResult(
                formulationId?.Trim() ?? string.Empty,
                profile: null,
                ScientificFormulationCompatibilityKind.Undetermined,
                problemClass: null,
                verifiedSupportedExtensions:
                    Array.Empty<LotSizingProblemClassExtensionKind>(),
                knownUnsupportedExtensions:
                    Array.Empty<LotSizingProblemClassExtensionKind>(),
                undeterminedExtensions:
                    Array.Empty<LotSizingProblemClassExtensionKind>(),
                diagnostics:
                    new[]
                    {
                        new ScientificFormulationDiagnostic(
                            "LSDM-FORM-001",
                            ScientificFormulationDiagnosticSeverity.Warning,
                            "formulation.profile",
                            "No scientific capability profile is registered " +
                            "for this formulation.")
                    });
        }

        return Assess(
            classification,
            profile!);
    }

    public ScientificFormulationCompatibilityResult Assess(
        ScientificClassificationResult classification,
        MathematicalFormulationScientificProfile profile)
    {
        ArgumentNullException.ThrowIfNull(classification);
        ArgumentNullException.ThrowIfNull(profile);

        var diagnostics =
            new List<ScientificFormulationDiagnostic>();

        if (classification.IsBlocked)
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-002",
                    ScientificFormulationDiagnosticSeverity.Error,
                    "classification",
                    "Scientific formulation compatibility is blocked " +
                    "because instance scientific classification is blocked."));

            return Create(
                profile,
                ScientificFormulationCompatibilityKind.Blocked,
                problemClass: null,
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                diagnostics);
        }

        LotSizingProblemClassMatchResult? primary =
            classification.PrimaryProblemClass;

        if (primary is null)
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-003",
                    ScientificFormulationDiagnosticSeverity.Warning,
                    "classification.problemClass",
                    "No unique canonical problem class is available for " +
                    "scientific formulation assessment."));

            return Create(
                profile,
                ScientificFormulationCompatibilityKind.Undetermined,
                problemClass: null,
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                diagnostics);
        }

        CanonicalLotSizingProblemClassId problemClass =
            primary.Definition.Id;

        if (!profile.SupportsProblemClass(problemClass))
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-010",
                    ScientificFormulationDiagnosticSeverity.Error,
                    "classification.problemClass",
                    $"Formulation '{profile.FormulationId}' has no " +
                    $"verified support for canonical class '{problemClass}'."));

            return Create(
                profile,
                ScientificFormulationCompatibilityKind.Incompatible,
                problemClass,
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                Array.Empty<LotSizingProblemClassExtensionKind>(),
                diagnostics);
        }

        var supported =
            new List<LotSizingProblemClassExtensionKind>();

        var unsupported =
            new List<LotSizingProblemClassExtensionKind>();

        var undetermined =
            new List<LotSizingProblemClassExtensionKind>();

        foreach (
            LotSizingProblemClassExtensionKind extension
            in primary.Extensions)
        {
            if (profile.IsExtensionVerifiedSupported(extension))
            {
                supported.Add(extension);
            }
            else if (profile.IsExtensionKnownUnsupported(extension))
            {
                unsupported.Add(extension);
            }
            else
            {
                undetermined.Add(extension);
            }
        }

        if (unsupported.Count > 0)
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-011",
                    ScientificFormulationDiagnosticSeverity.Error,
                    "classification.extensions",
                    "Known unsupported formulation extensions: " +
                    string.Join(",", unsupported)));

            return Create(
                profile,
                ScientificFormulationCompatibilityKind.Incompatible,
                problemClass,
                supported,
                unsupported,
                undetermined,
                diagnostics);
        }

        if (undetermined.Count > 0)
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-012",
                    ScientificFormulationDiagnosticSeverity.Warning,
                    "classification.extensions",
                    "Formulation support has not yet been scientifically " +
                    "verified for extensions: " +
                    string.Join(",", undetermined)));

            return Create(
                profile,
                ScientificFormulationCompatibilityKind.Undetermined,
                problemClass,
                supported,
                unsupported,
                undetermined,
                diagnostics);
        }

        diagnostics.Add(
            new ScientificFormulationDiagnostic(
                "LSDM-FORM-020",
                ScientificFormulationDiagnosticSeverity.Information,
                "formulation",
                $"Formulation '{profile.FormulationId}' has verified " +
                "scientific coverage for the detected canonical core and " +
                "all classified extensions."));

        return Create(
            profile,
            ScientificFormulationCompatibilityKind.Compatible,
            problemClass,
            supported,
            unsupported,
            undetermined,
            diagnostics);
    }

    private static ScientificFormulationCompatibilityResult Create(
        MathematicalFormulationScientificProfile profile,
        ScientificFormulationCompatibilityKind kind,
        CanonicalLotSizingProblemClassId? problemClass,
        IEnumerable<LotSizingProblemClassExtensionKind> supported,
        IEnumerable<LotSizingProblemClassExtensionKind> unsupported,
        IEnumerable<LotSizingProblemClassExtensionKind> undetermined,
        IEnumerable<ScientificFormulationDiagnostic> diagnostics) =>
            new(
                profile.FormulationId,
                profile,
                kind,
                problemClass,
                supported,
                unsupported,
                undetermined,
                diagnostics);
}
