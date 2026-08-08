using System;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Creates the public solver-run result from the generic
/// mathematical-model result and the normalized solution
/// mapping result.
/// </summary>
public static class MathematicalSolverRunResultMapper
{
    /// <summary>
    /// Creates a normalized solver-run result.
    /// </summary>
    /// <param name="solveResult">
    /// Generic mathematical-model solve result returned by the
    /// selected solver adapter.
    /// </param>
    /// <param name="mappingResult">
    /// Result of mapping mathematical variable values back to a
    /// lot-sizing solution.
    /// </param>
    /// <param name="startedAtUtc">
    /// UTC date and time at which the complete solve workflow
    /// started.
    /// </param>
    /// <param name="elapsed">
    /// Total elapsed duration of the complete solve workflow.
    /// </param>
    /// <returns>
    /// Public normalized solver-run result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="solveResult"/> or
    /// <paramref name="mappingResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="elapsed"/> is negative.
    /// </exception>
    public static SolverRunResult Create(
        MathematicalModelSolveResult solveResult,
        MathematicalSolutionMappingResult mappingResult,
        DateTime startedAtUtc,
        TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(
            solveResult);

        ArgumentNullException.ThrowIfNull(
            mappingResult);

        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsed),
                elapsed,
                "The total solve duration cannot be negative.");
        }

        DateTime normalizedStartedAtUtc =
            NormalizeUtc(
                startedAtUtc);

        var result =
            new SolverRunResult
            {
                RunName =
                    solveResult.RunName ?? string.Empty,

                SolverKind =
                    solveResult.SolverKind,

                SolverName =
                    solveResult.SolverName ?? string.Empty,

                SolverVersion =
                    solveResult.SolverVersion ?? string.Empty,

                FormulationName =
                    solveResult.FormulationId ?? string.Empty,

                StartedAtUtc =
                    normalizedStartedAtUtc,

                CompletedAtUtc =
                    normalizedStartedAtUtc + elapsed,

                ElapsedSeconds =
                    elapsed.TotalSeconds,

                TerminationReason =
                    solveResult.TerminationReason,

                ObjectiveValue =
                    solveResult.ObjectiveValue,

                BestBound =
                    solveResult.BestBound,

                AbsoluteGap =
                    solveResult.AbsoluteGap,

                RelativeGap =
                    solveResult.RelativeGap,

                ExploredNodeCount =
                    solveResult.ExploredNodeCount,

                IterationCount =
                    solveResult.IterationCount,

                SolutionCount =
                    solveResult.HasFeasibleSolution
                        ? 1
                        : 0,

                Solution =
                    mappingResult.IsSuccessful
                        ? mappingResult.Solution
                        : null
            };

        result.AddDiagnostics(
            solveResult);

        result.AddDiagnostics(
            mappingResult);

        return result;
    }

    private static DateTime NormalizeUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
        };
    }

    private static void AddDiagnostics(
        this SolverRunResult target,
        MathematicalModelSolveResult source)
    {
        foreach (
            string diagnostic
            in source.Diagnostics)
        {
            if (!string.IsNullOrWhiteSpace(
                    diagnostic))
            {
                target.AddDiagnostic(
                    diagnostic);
            }
        }
    }

    private static void AddDiagnostics(
        this SolverRunResult target,
        MathematicalSolutionMappingResult source)
    {
        foreach (
            string diagnostic
            in source.Diagnostics)
        {
            if (!string.IsNullOrWhiteSpace(
                    diagnostic))
            {
                target.AddDiagnostic(
                    diagnostic);
            }
        }

        if (!source.IsSuccessful &&
            !string.IsNullOrWhiteSpace(
                source.FailureMessage))
        {
            bool alreadyPresent =
                source.Diagnostics.Exists(
                    diagnostic =>
                        string.Equals(
                            diagnostic,
                            source.FailureMessage,
                            StringComparison.Ordinal));

            if (!alreadyPresent)
            {
                target.AddDiagnostic(
                    source.FailureMessage);
            }
        }
    }
}
