using MetaheuristicsPlatform.Algorithms.Constraints.DebConstraintGa;
using MetaheuristicsPlatform.Algorithms.Matheuristics.LocalBranching;
using MetaheuristicsPlatform.Catalog;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public static class MetaheuristicsPlatformBridgeProvenance
{
    public const string Version = "1.0.1";

    public const string SourceCommit =
        "0ab7521dc1f42f50209c8badea811502977b8409";

    public const string ReleaseArchiveSha256 =
        "ce7d39c184e17965e64b2739516b62bd186ff4bf12bdedf570f1293e241f2404";

    public static void EnsureCompatibleRuntime()
    {
        var deb =
            new DebConstraintGaOptimizer();

        if (!string.Equals(
                deb.Descriptor.Id,
                MetaheuristicAlgorithmIds.DebConstraintGa,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "MetaheuristicsPlatform Deb constraint-GA identity drift.");
        }

        var localBranching =
            new LocalBranchingMatheuristicOptimizer();

        if (!string.Equals(
                localBranching.Descriptor.Id,
                MetaheuristicAlgorithmIds.LocalBranchingMatheuristic,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "MetaheuristicsPlatform Local Branching identity drift.");
        }
    }
}
