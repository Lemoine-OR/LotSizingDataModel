namespace LotSizingDataModel.Solution.Metadata.Scientific;

/// <summary>
/// Result of decoding scientific provenance from generation metadata.
/// </summary>
public sealed class SolutionScientificProvenanceReadResult
{
    internal SolutionScientificProvenanceReadResult(
        SolutionScientificProvenanceReadKind kind,
        SolutionScientificProvenance? provenance,
        string diagnostic)
    {
        Kind = kind;
        Provenance = provenance;
        Diagnostic = diagnostic ?? string.Empty;
    }

    public SolutionScientificProvenanceReadKind Kind { get; }

    public SolutionScientificProvenance? Provenance { get; }

    public string Diagnostic { get; }

    public bool IsValid =>
        Kind == SolutionScientificProvenanceReadKind.Valid;
}
