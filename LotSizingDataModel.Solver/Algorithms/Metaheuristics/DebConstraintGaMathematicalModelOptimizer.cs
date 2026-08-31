using System.Diagnostics;
using LotSizingDataModel.Solver.Modeling;
using MetaheuristicsPlatform.Algorithms.Constraints.DebConstraintGa;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class DebConstraintGaMathematicalModelOptimizer
{
    public DebConstraintGaBridgeResult Optimize(
        MathematicalModel model,
        DebConstraintGaBridgeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            model);

        options ??=
            new DebConstraintGaBridgeOptions();

        MetaheuristicsPlatformBridgeProvenance
            .EnsureCompatibleRuntime();

        var encoding =
            new MathematicalModelMetaheuristicEncoding(
                model,
                options.Encoding);

        var problem =
            new MathematicalModelConstrainedMetaheuristicProblem(
                model,
                encoding,
                options.EqualityTolerance);

        var optimizer =
            new DebConstraintGaOptimizer();

        if (!string.Equals(
                optimizer.Descriptor.Id,
                MetaheuristicAlgorithmIds.DebConstraintGa,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Unexpected Deb constraint-GA algorithm identity.");
        }

        var parameters =
            new DebConstraintGaParameters
            {
                PopulationSize =
                    options.PopulationSize,

                MaximumGenerations =
                    options.MaximumGenerations,

                CrossoverProbability =
                    options.CrossoverProbability,

                MutationProbability =
                    options.MutationProbability,

                DistributionIndex =
                    options.DistributionIndex
            };

        var platformOptions =
            new OptimizationOptions
            {
                Seed =
                    options.Seed
            };

        var stopwatch =
            Stopwatch.StartNew();

        ConstrainedOptimizationResult platformResult =
            optimizer.Optimize(
                problem,
                parameters,
                platformOptions,
                cancellationToken);

        stopwatch.Stop();

        double[] decoded =
            problem.DecodeCandidate(
                platformResult.Best.Solution);

        double objective =
            problem.EvaluateObjective(
                platformResult.Best.Solution);

        double scale =
            Math.Max(
                1.0,
                Math.Abs(objective));

        if (Math.Abs(
                objective -
                platformResult.Best.Objective) >
            1.0e-9 * scale)
        {
            throw new InvalidOperationException(
                "MetaheuristicsPlatform objective result does not match independent bridge evaluation.");
        }

        bool isFeasible =
            platformResult.Best.Constraints.IsFeasible &&
            problem.IsDecodedCandidateFeasible(
                platformResult.Best.Solution);

        return new DebConstraintGaBridgeResult(
            optimizer.Descriptor.Id,
            platformResult.Best.Solution,
            decoded,
            objective,
            platformResult.Best.Constraints.TotalViolation,
            isFeasible,
            platformResult.Evaluations,
            platformResult.Iterations,
            platformResult.Seed,
            stopwatch.Elapsed);
    }
}
