using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Discovery;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Initializes the complete lot-sizing solver infrastructure by
/// discovering solver plugins, building the solver runtime, and
/// creating the high-level solver service.
/// </summary>
public static class LotSizingSolverBootstrapper
{
    /// <summary>
    /// Initializes the complete solver infrastructure using
    /// default solver-discovery options.
    /// </summary>
    /// <param name="formulationRegistry">
    /// Registry containing the mathematical formulations
    /// available to the application.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel solver discovery and adapter
    /// loading.
    /// </param>
    /// <returns>
    /// Result containing the runtime-build information and, when
    /// initialization succeeds, the high-level solver service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulationRegistry"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static ValueTask<LotSizingSolverBootstrapResult>
        InitializeAsync(
            MathematicalModelFormulationRegistry formulationRegistry,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            formulationRegistry);

        return InitializeAsync(
            formulationRegistry,
            new SolverDiscoveryOptions(),
            cancellationToken);
    }

    /// <summary>
    /// Initializes the complete solver infrastructure using the
    /// supplied solver-discovery options.
    /// </summary>
    /// <param name="formulationRegistry">
    /// Registry containing the mathematical formulations
    /// available to the application.
    /// </param>
    /// <param name="discoveryOptions">
    /// Solver-discovery options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel solver discovery and adapter
    /// loading.
    /// </param>
    /// <returns>
    /// Result containing the runtime-build information and, when
    /// initialization succeeds, the high-level solver service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulationRegistry"/> or
    /// <paramref name="discoveryOptions"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static async ValueTask<LotSizingSolverBootstrapResult>
        InitializeAsync(
            MathematicalModelFormulationRegistry formulationRegistry,
            SolverDiscoveryOptions discoveryOptions,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            formulationRegistry);

        ArgumentNullException.ThrowIfNull(
            discoveryOptions);

        SolverRuntimeBuildResult runtimeBuildResult =
            await SolverRuntimeFactory.BuildAsync(
                discoveryOptions,
                cancellationToken);

        if (runtimeBuildResult.RuntimeContext is null)
        {
            return new LotSizingSolverBootstrapResult(
                runtimeBuildResult,
                null);
        }

        LotSizingSolverService solverService =
            LotSizingSolverServiceFactory.Create(
                formulationRegistry,
                runtimeBuildResult.RuntimeContext);

        return new LotSizingSolverBootstrapResult(
            runtimeBuildResult,
            solverService);
    }
}
