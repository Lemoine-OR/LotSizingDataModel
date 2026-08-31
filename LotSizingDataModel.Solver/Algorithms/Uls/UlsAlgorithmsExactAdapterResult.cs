using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// End-to-end LotSizingDataModel result of an external exact
/// ULSAlgorithms adapter execution.
/// </summary>
public sealed class UlsAlgorithmsExactAdapterResult
{
    public UlsAlgorithmsExactAdapterResult(
        UlsAlgorithmsExactBridgeResult externalResult,
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

    public UlsAlgorithmsExactBridgeResult ExternalResult
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
