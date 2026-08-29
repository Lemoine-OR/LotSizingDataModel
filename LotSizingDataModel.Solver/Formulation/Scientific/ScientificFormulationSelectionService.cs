using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Scientific;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Scientifically filters registered mathematical formulations before
/// technical build selection.
/// </summary>
/// <remarks>
/// Automatic selection requires verified scientific compatibility.
/// Undetermined profiles/extensions are never auto-selected.
///
/// The LotSizingInstance overload additionally evaluates CanBuild(instance).
/// </remarks>
public sealed class ScientificFormulationSelectionService
{
    private readonly ScientificClassificationEngine _classificationEngine;
    private readonly ScientificFormulationCompatibilityService
        _compatibilityService;

    public ScientificFormulationSelectionService()
        : this(
            new ScientificClassificationEngine(),
            new ScientificFormulationCompatibilityService())
    {
    }

    public ScientificFormulationSelectionService(
        ScientificClassificationEngine classificationEngine,
        ScientificFormulationCompatibilityService compatibilityService)
    {
        _classificationEngine =
            classificationEngine ??
            throw new ArgumentNullException(nameof(classificationEngine));

        _compatibilityService =
            compatibilityService ??
            throw new ArgumentNullException(nameof(compatibilityService));
    }

    public ScientificFormulationSelectionResult Select(
        ScientificClassificationResult classification,
        MathematicalModelFormulationRegistry registry,
        string requestedFormulationId = "",
        bool allowFallback = true)
    {
        ArgumentNullException.ThrowIfNull(classification);
        ArgumentNullException.ThrowIfNull(registry);

        return SelectCore(
            classification,
            registry,
            requestedFormulationId,
            allowFallback,
            instance: null);
    }

    public ScientificFormulationSelectionResult Select(
        LotSizingInstance instance,
        MathematicalModelFormulationRegistry registry,
        string requestedFormulationId = "",
        bool allowFallback = true,
        ScientificClassificationRequest? classificationRequest = null)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(registry);

        ScientificClassificationResult classification =
            _classificationEngine.Analyze(
                instance,
                classificationRequest);

        return SelectCore(
            classification,
            registry,
            requestedFormulationId,
            allowFallback,
            instance);
    }

    private ScientificFormulationSelectionResult SelectCore(
        ScientificClassificationResult classification,
        MathematicalModelFormulationRegistry registry,
        string requestedFormulationId,
        bool allowFallback,
        LotSizingInstance? instance)
    {
        string requested =
            requestedFormulationId?.Trim() ?? string.Empty;

        var diagnostics =
            new List<ScientificFormulationDiagnostic>();

        var candidates =
            registry.GetAll()
                .Select(
                    formulation =>
                    {
                        ScientificFormulationCompatibilityResult compatibility =
                            _compatibilityService.Assess(
                                classification,
                                formulation.FormulationId);

                        bool? canBuild =
                            instance is null
                                ? null
                                : formulation.CanBuild(instance);

                        if (canBuild == false)
                        {
                            diagnostics.Add(
                                new ScientificFormulationDiagnostic(
                                    "LSDM-FORM-030",
                                    ScientificFormulationDiagnosticSeverity.Error,
                                    $"formulation.{formulation.FormulationId}",
                                    "Scientific compatibility may be satisfied, " +
                                    "but the formulation's technical CanBuild " +
                                    "contract rejected the instance."));
                        }

                        return new ScientificFormulationSelectionCandidate(
                            formulation,
                            compatibility,
                            canBuild);
                    })
                .OrderBy(
                    candidate =>
                        candidate.Formulation.FormulationId,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        ScientificFormulationSelectionCandidate? selected = null;
        bool usedFallback = false;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            ScientificFormulationSelectionCandidate? requestedCandidate =
                candidates.FirstOrDefault(
                    candidate =>
                        candidate.Formulation.FormulationId.Equals(
                            requested,
                            StringComparison.OrdinalIgnoreCase));

            if (requestedCandidate is null)
            {
                diagnostics.Add(
                    new ScientificFormulationDiagnostic(
                        "LSDM-FORM-031",
                        ScientificFormulationDiagnosticSeverity.Error,
                        "selection.requestedFormulation",
                        $"Requested formulation '{requested}' is not registered."));
            }
            else if (requestedCandidate.IsSelectable)
            {
                selected = requestedCandidate;
            }
            else
            {
                diagnostics.Add(
                    new ScientificFormulationDiagnostic(
                        "LSDM-FORM-032",
                        ScientificFormulationDiagnosticSeverity.Error,
                        "selection.requestedFormulation",
                        $"Requested formulation '{requested}' is not " +
                        "scientifically and technically selectable."));
            }

            if (selected is null && !allowFallback)
            {
                return new ScientificFormulationSelectionResult(
                    classification,
                    requested,
                    selectedCandidate: null,
                    usedFallback: false,
                    candidates,
                    diagnostics);
            }
        }

        if (selected is null)
        {
            selected =
                candidates.FirstOrDefault(
                    candidate =>
                        candidate.IsSelectable);

            usedFallback =
                selected is not null &&
                !string.IsNullOrWhiteSpace(requested);
        }

        if (selected is null)
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-033",
                    ScientificFormulationDiagnosticSeverity.Error,
                    "selection",
                    "No registered formulation has verified scientific " +
                    "compatibility and an acceptable technical build status."));
        }
        else
        {
            diagnostics.Add(
                new ScientificFormulationDiagnostic(
                    "LSDM-FORM-034",
                    ScientificFormulationDiagnosticSeverity.Information,
                    "selection",
                    $"Selected formulation '{selected.Formulation.FormulationId}'."));
        }

        return new ScientificFormulationSelectionResult(
            classification,
            requested,
            selected,
            usedFallback,
            candidates,
            diagnostics);
    }
}
