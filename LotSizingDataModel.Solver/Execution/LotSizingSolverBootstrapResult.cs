using System;
using LotSizingDataModel.Solver.Discovery;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Represents the result of initializing the complete
/// lot-sizing solver infrastructure.
/// </summary>
/// <remarks>
/// The result keeps both the low-level solver-runtime build
/// information and the high-level solver service created from
/// that runtime.
/// </remarks>
public sealed class LotSizingSolverBootstrapResult
{
    /// <summary>
    /// Initializes a solver-bootstrap result.
    /// </summary>
    /// <param name="runtimeBuildResult">
    /// Result of solver discovery and adapter loading.
    /// </param>
    /// <param name="solverService">
    /// High-level solver service created from the runtime, or
    /// <see langword="null"/> when initialization did not
    /// produce a usable service.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="runtimeBuildResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingSolverBootstrapResult(
        SolverRuntimeBuildResult runtimeBuildResult,
        LotSizingSolverService? solverService)
    {
        ArgumentNullException.ThrowIfNull(
            runtimeBuildResult);

        RuntimeBuildResult =
            runtimeBuildResult;

        SolverService =
            solverService;
    }

    /// <summary>
    /// Gets the result of solver discovery and adapter loading.
    /// </summary>
    public SolverRuntimeBuildResult RuntimeBuildResult
    {
        get;
    }

    /// <summary>
    /// Gets the high-level solver service created from the
    /// runtime, or <see langword="null"/> when initialization
    /// did not produce one.
    /// </summary>
    public LotSizingSolverService? SolverService
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the bootstrap operation
    /// created a solver service successfully.
    /// </summary>
    public bool IsSuccessful =>
        SolverService is not null;

    /// <summary>
    /// Gets a value indicating whether the initialized runtime
    /// can currently solve at least one mathematical model.
    /// </summary>
    public bool CanSolve =>
        SolverService is not null &&
        RuntimeBuildResult.CanSolve;
}
