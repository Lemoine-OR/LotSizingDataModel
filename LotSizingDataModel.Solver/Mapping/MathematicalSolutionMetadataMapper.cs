using System;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Metadata;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps generic solver execution metadata to the generation
/// metadata of a normalized lot-sizing solution.
/// </summary>
public static class MathematicalSolutionMetadataMapper
{
    /// <summary>
    /// Applies solver execution metadata to a lot-sizing
    /// solution.
    /// </summary>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="solveResult">
    /// Generic mathematical-model solve result.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="solution"/> or
    /// <paramref name="solveResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static void Apply(
        LotSizingSolution solution,
        MathematicalModelSolveResult solveResult)
    {
        ArgumentNullException.ThrowIfNull(
            solution);

        ArgumentNullException.ThrowIfNull(
            solveResult);

        solveResult.EnsureValid();

        SolutionGenerationMetadata metadata =
            solution.GenerationMetadata;

        metadata.MethodKind =
            SolutionMethodKind.ExactOptimization;

        metadata.MethodName =
            "Mathematical optimization";

        metadata.MethodVersion =
            solveResult.FormulationId;

        metadata.ImplementationName =
            solveResult.SolverName;

        metadata.ImplementationVersion =
            solveResult.SolverVersion;

        metadata.TerminationReason =
            MapTerminationReason(
                solveResult.TerminationReason);

        metadata.DurationSeconds =
            solveResult.SolveDuration.TotalSeconds;

        metadata.IterationCount =
            solveResult.IterationCount;

        metadata.CreatedAtUtc =
            DateTime.UtcNow;

        metadata.Comment =
            BuildComment(
                solveResult);
    }

    /// <summary>
    /// Converts a solver termination reason to the generic
    /// solution-generation termination reason.
    /// </summary>
    /// <param name="terminationReason">
    /// Solver-specific normalized termination reason.
    /// </param>
    /// <returns>
    /// Solution-generation termination reason.
    /// </returns>
    public static TerminationReason MapTerminationReason(
        SolverTerminationReason terminationReason)
    {
        return terminationReason switch
        {
            SolverTerminationReason.Optimal =>
                TerminationReason.OptimalityProven,

            SolverTerminationReason.Feasible =>
                TerminationReason.Completed,

            SolverTerminationReason.TimeLimit =>
                TerminationReason.TimeLimit,

            SolverTerminationReason.IterationLimit =>
                TerminationReason.IterationLimit,

            SolverTerminationReason.NodeLimit =>
                TerminationReason.ResourceLimit,

            SolverTerminationReason.MemoryLimit =>
                TerminationReason.ResourceLimit,

            SolverTerminationReason.SolutionLimit =>
                TerminationReason.EvaluationLimit,

            SolverTerminationReason.ObjectiveLimit =>
                TerminationReason.TargetReached,

            SolverTerminationReason.RelativeGapLimit =>
                TerminationReason.TargetReached,

            SolverTerminationReason.AbsoluteGapLimit =>
                TerminationReason.TargetReached,

            SolverTerminationReason.UserInterrupted =>
                TerminationReason.UserInterrupted,

            SolverTerminationReason.Infeasible =>
                TerminationReason.Completed,

            SolverTerminationReason.Unbounded =>
                TerminationReason.Completed,

            SolverTerminationReason.InfeasibleOrUnbounded =>
                TerminationReason.Completed,

            SolverTerminationReason.NumericalDifficulty =>
                TerminationReason.Error,

            SolverTerminationReason.LicenseError =>
                TerminationReason.Error,

            SolverTerminationReason.SolverUnavailable =>
                TerminationReason.Error,

            SolverTerminationReason.ModelError =>
                TerminationReason.Error,

            SolverTerminationReason.InternalError =>
                TerminationReason.Error,

            _ =>
                TerminationReason.Unknown
        };
    }

    private static string BuildComment(
        MathematicalModelSolveResult solveResult)
    {
        if (!solveResult.ObjectiveValue.HasValue &&
            !solveResult.BestBound.HasValue &&
            !solveResult.RelativeGap.HasValue &&
            !solveResult.AbsoluteGap.HasValue)
        {
            return string.Empty;
        }

        return
            $"Objective={FormatNullable(solveResult.ObjectiveValue)}; " +
            $"BestBound={FormatNullable(solveResult.BestBound)}; " +
            $"RelativeGap={FormatNullable(solveResult.RelativeGap)}; " +
            $"AbsoluteGap={FormatNullable(solveResult.AbsoluteGap)}.";
    }

    private static string FormatNullable(
        double? value)
    {
        return value.HasValue
            ? value.Value.ToString(
                "G17",
                System.Globalization.CultureInfo.InvariantCulture)
            : "n/a";
    }
}
