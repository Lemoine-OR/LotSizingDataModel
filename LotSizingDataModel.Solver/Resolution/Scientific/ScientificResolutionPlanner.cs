using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// Builds an explainable scientific resolution plan from already classified
/// instance/formulation evidence.
/// </summary>
public sealed class ScientificResolutionPlanner
{
    private readonly ScientificSolutionMethodCompatibilityService
        _methodCompatibilityService;

    public ScientificResolutionPlanner()
        : this(
            new ScientificSolutionMethodCompatibilityService())
    {
    }

    public ScientificResolutionPlanner(
        ScientificSolutionMethodCompatibilityService
            methodCompatibilityService)
    {
        _methodCompatibilityService =
            methodCompatibilityService ??
            throw new ArgumentNullException(
                nameof(methodCompatibilityService));
    }

    public ScientificResolutionPlan Create(
        ScientificClassificationResult classification,
        ScientificFormulationSelectionResult formulationSelection,
        SolverKind requestedSolverKind)
    {
        ArgumentNullException.ThrowIfNull(classification);
        ArgumentNullException.ThrowIfNull(formulationSelection);

        ScientificSolutionMethodCandidate[] candidates =
            ScientificSolutionMethodCatalog.All
                .Select(
                    method =>
                        _methodCompatibilityService.Assess(
                            classification,
                            formulationSelection,
                            method))
                .ToArray();

        ScientificSolutionMethodCandidate? selectedMethod =
            candidates
                .Where(candidate => candidate.IsExecutableCandidate)
                .OrderBy(
                    candidate =>
                        candidate.Method.MethodId,
                    StringComparer.Ordinal)
                .FirstOrDefault();

        var diagnostics =
            new List<string>();

        if (classification.IsBlocked)
        {
            diagnostics.Add(
                "Scientific classification is blocked.");

            return new ScientificResolutionPlan(
                ScientificResolutionPlanStatus.Blocked,
                classification,
                formulationSelection,
                candidates,
                selectedMethod: null,
                requestedSolverKind,
                backendCandidates:
                    Array.Empty<ScientificSolverBackendDefinition>(),
                selectedBackend: null,
                diagnostics);
        }

        if (selectedMethod is null)
        {
            diagnostics.Add(
                "No executable scientific solution method is available.");

            return new ScientificResolutionPlan(
                ScientificResolutionPlanStatus.NoExecutableMethod,
                classification,
                formulationSelection,
                candidates,
                selectedMethod: null,
                requestedSolverKind,
                backendCandidates:
                    Array.Empty<ScientificSolverBackendDefinition>(),
                selectedBackend: null,
                diagnostics);
        }

        ScientificSolverBackendDefinition[] backendCandidates =
            selectedMethod.Method.RequiresMilpBackend
                ? ScientificSolverBackendCatalog.All
                    .Where(
                        backend =>
                            backend.Supports(selectedMethod.Method))
                    .ToArray()
                : Array.Empty<ScientificSolverBackendDefinition>();

        ScientificSolverBackendDefinition? selectedBackend =
            null;

        if (
            requestedSolverKind != SolverKind.Automatic)
        {
            selectedBackend =
                ScientificSolverBackendCatalog.Find(
                    requestedSolverKind);

            if (
                selectedBackend is null ||
                !selectedBackend.Supports(
                    selectedMethod.Method))
            {
                diagnostics.Add(
                    $"Requested solver backend '{requestedSolverKind}' is " +
                    $"not scientifically compatible with selected method " +
                    $"'{selectedMethod.Method.MethodId}'.");

                return new ScientificResolutionPlan(
                    ScientificResolutionPlanStatus.BackendIncompatible,
                    classification,
                    formulationSelection,
                    candidates,
                    selectedMethod,
                    requestedSolverKind,
                    backendCandidates,
                    selectedBackend: null,
                    diagnostics);
            }
        }

        diagnostics.Add(
            requestedSolverKind == SolverKind.Automatic
                ? "Resolution method is fixed scientifically; concrete MILP " +
                  "backend remains delegated to technical availability."
                : $"Resolution method and requested backend " +
                  $"'{requestedSolverKind}' are scientifically compatible.");

        return new ScientificResolutionPlan(
            ScientificResolutionPlanStatus.Ready,
            classification,
            formulationSelection,
            candidates,
            selectedMethod,
            requestedSolverKind,
            backendCandidates,
            selectedBackend,
            diagnostics);
    }
}
