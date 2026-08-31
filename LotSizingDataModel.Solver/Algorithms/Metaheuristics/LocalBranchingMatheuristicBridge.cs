using MetaheuristicsPlatform.Algorithms.Matheuristics.LocalBranching;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class LocalBranchingMatheuristicBridge
{
    public LocalBranchingBridgeResult Optimize(
        LotSizingExactRepairMatheuristicDomain domain,
        LocalBranchingBridgeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            domain);

        options ??=
            new LocalBranchingBridgeOptions();

        MetaheuristicsPlatformBridgeProvenance
            .EnsureCompatibleRuntime();

        var optimizer =
            new LocalBranchingMatheuristicOptimizer();

        if (!string.Equals(
                optimizer.Descriptor.Id,
                MetaheuristicAlgorithmIds.LocalBranchingMatheuristic,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Unexpected Local Branching algorithm identity.");
        }

        var parameters =
            new LocalBranchingMatheuristicParameters
            {
                MaximumIterations =
                    options.MaximumIterations,

                HammingRadius =
                    options.HammingRadius,

                NodeLimit =
                    options.NodeLimit
            };

        var platformOptions =
            new OptimizationOptions
            {
                Seed =
                    options.Seed
            };

        MatheuristicOptimizationResult result =
            optimizer.Optimize(
                domain,
                parameters,
                platformOptions,
                cancellationToken);

        return new LocalBranchingBridgeResult(
            optimizer.Descriptor.Id,
            result.Best.Values,
            result.BestObjective,
            result.ExactSolves,
            result.RelaxationSolves,
            result.Iterations,
            result.Seed,
            result.ExactRepairTrace);
    }
}
