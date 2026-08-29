using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// Explicit scientific resolution plan:
/// problem -> formulation -> solution method -> compatible backend family.
/// </summary>
public sealed class ScientificResolutionPlan
{
    internal ScientificResolutionPlan(
        ScientificResolutionPlanStatus status,
        ScientificClassificationResult classification,
        ScientificFormulationSelectionResult formulationSelection,
        IEnumerable<ScientificSolutionMethodCandidate> methodCandidates,
        ScientificSolutionMethodCandidate? selectedMethod,
        SolverKind requestedSolverKind,
        IEnumerable<ScientificSolverBackendDefinition> backendCandidates,
        ScientificSolverBackendDefinition? selectedBackend,
        IEnumerable<string> diagnostics)
    {
        Status = status;

        Classification =
            classification ??
            throw new ArgumentNullException(nameof(classification));

        FormulationSelection =
            formulationSelection ??
            throw new ArgumentNullException(nameof(formulationSelection));

        MethodCandidates =
            methodCandidates.ToArray();

        SelectedMethod = selectedMethod;
        RequestedSolverKind = requestedSolverKind;

        BackendCandidates =
            backendCandidates.ToArray();

        SelectedBackend = selectedBackend;

        Diagnostics =
            diagnostics.ToArray();
    }

    public ScientificResolutionPlanStatus Status { get; }

    public ScientificClassificationResult Classification { get; }

    public ScientificFormulationSelectionResult
        FormulationSelection { get; }

    public IReadOnlyList<ScientificSolutionMethodCandidate>
        MethodCandidates { get; }

    public ScientificSolutionMethodCandidate? SelectedMethod { get; }

    public SolverKind RequestedSolverKind { get; }

    public IReadOnlyList<ScientificSolverBackendDefinition>
        BackendCandidates { get; }

    /// <summary>
    /// Null for Automatic backend selection.
    /// </summary>
    public ScientificSolverBackendDefinition? SelectedBackend { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public bool IsReady =>
        Status == ScientificResolutionPlanStatus.Ready &&
        SelectedMethod is not null;

    public string? SelectedMethodId =>
        SelectedMethod?.Method.MethodId;
}
