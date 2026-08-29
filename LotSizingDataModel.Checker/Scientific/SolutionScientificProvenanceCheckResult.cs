using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Scientific;

/// <summary>
/// Independent scientific provenance verification result.
/// </summary>
public sealed class SolutionScientificProvenanceCheckResult
{
    internal SolutionScientificProvenanceCheckResult(
        SolutionScientificProvenanceCheckKind kind,
        SolutionScientificProvenance? recordedProvenance,
        ScientificClassificationResult? currentClassification,
        ScientificFormulationCompatibilityResult? formulationCompatibility,
        IEnumerable<SolutionScientificProvenanceDiagnostic> diagnostics)
    {
        Kind = kind;
        RecordedProvenance = recordedProvenance;
        CurrentClassification = currentClassification;
        FormulationCompatibility = formulationCompatibility;
        Diagnostics = diagnostics.ToArray();
    }

    public SolutionScientificProvenanceCheckKind Kind { get; }

    public SolutionScientificProvenance? RecordedProvenance { get; }

    public ScientificClassificationResult? CurrentClassification { get; }

    public ScientificFormulationCompatibilityResult?
        FormulationCompatibility { get; }

    public IReadOnlyList<SolutionScientificProvenanceDiagnostic>
        Diagnostics { get; }

    public bool IsCoherent =>
        Kind == SolutionScientificProvenanceCheckKind.Coherent;

    public bool HasErrors =>
        Diagnostics.Any(diagnostic => diagnostic.IsError);
}
