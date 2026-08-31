namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Runs every pinned v1.1.0 heuristic in canonical catalog order.
/// </summary>
public sealed class UlsAlgorithmsHeuristicBatchRunner
{
    private readonly UlsAlgorithmsHeuristicBridge
        _bridge =
            new();

    public IReadOnlyList<UlsAlgorithmsHeuristicBridgeResult>
        SolveAll(
            UlsAlgorithmsExactProblemData problemData,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            problemData);

        var results =
            new List<UlsAlgorithmsHeuristicBridgeResult>();

        foreach (string solverId
                 in UlsAlgorithmsHeuristicCatalog.SolverIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            results.Add(
                _bridge.Solve(
                    problemData,
                    solverId,
                    cancellationToken));
        }

        return results;
    }
}
