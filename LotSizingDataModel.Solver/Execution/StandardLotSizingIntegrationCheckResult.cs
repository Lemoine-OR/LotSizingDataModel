using System;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Represents the result of a complete standard lot-sizing
/// integration check.
/// </summary>
public sealed class StandardLotSizingIntegrationCheckResult
{
    /// <summary>
    /// Initializes an integration-check result.
    /// </summary>
    /// <param name="bootstrapResult">
    /// Standard solver bootstrap result.
    /// </param>
    /// <param name="runResult">
    /// Solver run result, or <see langword="null"/> when solving
    /// could not be started.
    /// </param>
    public StandardLotSizingIntegrationCheckResult(
        StandardLotSizingSolverBootstrapResult bootstrapResult,
        SolverRunResult? runResult)
    {
        ArgumentNullException.ThrowIfNull(bootstrapResult);

        BootstrapResult = bootstrapResult;
        RunResult = runResult;
    }

    /// <summary>
    /// Gets the bootstrap result.
    /// </summary>
    public StandardLotSizingSolverBootstrapResult BootstrapResult
    {
        get;
    }

    /// <summary>
    /// Gets the solver run result, when solving was started.
    /// </summary>
    public SolverRunResult? RunResult
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether solver initialization was
    /// successful.
    /// </summary>
    public bool BootstrapSucceeded =>
        BootstrapResult.IsSuccessful;

    /// <summary>
    /// Gets a value indicating whether at least one usable solver
    /// was available.
    /// </summary>
    public bool CanSolve =>
        BootstrapResult.CanSolve;

    /// <summary>
    /// Gets a value indicating whether a feasible lot-sizing
    /// solution was produced.
    /// </summary>
    public bool HasFeasibleSolution =>
        RunResult?.Solution is not null;

    /// <summary>
    /// Gets the normalized termination reason, or
    /// <see cref="SolverTerminationReason.Unknown"/> when no run
    /// was started.
    /// </summary>
    public SolverTerminationReason TerminationReason =>
        RunResult?.TerminationReason ??
        SolverTerminationReason.Unknown;
}
