using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// End-to-end normalized result for one ULSAlgorithms heuristic.
/// </summary>
public sealed class UlsAlgorithmsHeuristicAdapterResult
{
    public UlsAlgorithmsHeuristicAdapterResult(
        UlsAlgorithmsHeuristicBridgeResult externalResult,
        MathematicalModelSolveResult mathematicalResult,
        LotSizingSolution solution)
    {
        ExternalResult =
            externalResult ??
            throw new ArgumentNullException(
                nameof(externalResult));

        MathematicalResult =
            mathematicalResult ??
            throw new ArgumentNullException(
                nameof(mathematicalResult));

        Solution =
            solution ??
            throw new ArgumentNullException(
                nameof(solution));
    }

    public UlsAlgorithmsHeuristicBridgeResult ExternalResult
    {
        get;
    }

    public MathematicalModelSolveResult MathematicalResult
    {
        get;
    }

    public LotSizingSolution Solution
    {
        get;
    }
}
