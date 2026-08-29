namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// Executes scientific preflight, technical solve, provenance capture and
/// independent verification as one orchestrated workflow.
/// </summary>
public interface IScientificLotSizingSolvePipeline
{
    ValueTask<ScientificSolvePipelineResult> SolveAsync(
        ScientificSolvePipelineRequest request,
        CancellationToken cancellationToken = default);

    void RequestStop();
}
