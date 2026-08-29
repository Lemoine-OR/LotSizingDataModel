namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// High-level status of the end-to-end scientific solve pipeline.
/// </summary>
public enum ScientificSolvePipelineStatus
{
    PreflightRejected,
    CompletedWithoutSolution,
    FormulationDrift,
    ProvenanceCaptureFailed,
    Completed
}
