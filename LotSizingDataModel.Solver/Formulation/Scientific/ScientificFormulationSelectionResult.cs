using LotSizingDataModel.Instance.Scientific;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Result of scientific formulation preselection/selection.
/// </summary>
public sealed class ScientificFormulationSelectionResult
{
    internal ScientificFormulationSelectionResult(
        ScientificClassificationResult classification,
        string requestedFormulationId,
        ScientificFormulationSelectionCandidate? selectedCandidate,
        bool usedFallback,
        IEnumerable<ScientificFormulationSelectionCandidate> candidates,
        IEnumerable<ScientificFormulationDiagnostic> diagnostics)
    {
        Classification =
            classification ??
            throw new ArgumentNullException(nameof(classification));

        RequestedFormulationId =
            requestedFormulationId ?? string.Empty;

        SelectedCandidate = selectedCandidate;
        UsedFallback = usedFallback;
        Candidates = candidates.ToArray();
        Diagnostics = diagnostics.ToArray();
    }

    public ScientificClassificationResult Classification { get; }
    public string RequestedFormulationId { get; }
    public ScientificFormulationSelectionCandidate? SelectedCandidate { get; }
    public bool UsedFallback { get; }
    public IReadOnlyList<ScientificFormulationSelectionCandidate> Candidates { get; }
    public IReadOnlyList<ScientificFormulationDiagnostic> Diagnostics { get; }

    public bool IsSuccessful =>
        SelectedCandidate is not null;

    public IMathematicalModelFormulation? Formulation =>
        SelectedCandidate?.Formulation;
}
