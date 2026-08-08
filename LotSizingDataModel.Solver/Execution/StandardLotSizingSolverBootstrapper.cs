using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Discovery;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Provides one-step initialization of the standard lot-sizing
/// formulation together with solver discovery and adapter
/// loading.
/// </summary>
/// <remarks>
/// This bootstrapper is the application-level entry point for
/// the default solver infrastructure. It creates the standard
/// formulation registry and delegates native solver discovery to
/// <see cref="LotSizingSolverBootstrapper"/>.
/// </remarks>
public static class StandardLotSizingSolverBootstrapper
{
    /// <summary>
    /// Initializes the standard formulation and discovers
    /// available solver adapters using default options.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel solver discovery and adapter
    /// loading.
    /// </param>
    /// <returns>
    /// Complete standard solver-bootstrap result.
    /// </returns>
    public static ValueTask<StandardLotSizingSolverBootstrapResult>
        InitializeAsync(
            CancellationToken cancellationToken = default)
    {
        return InitializeAsync(
            new Formulation.StandardLotSizingFormulationOptions(),
            new SolverDiscoveryOptions(),
            cancellationToken);
    }

    /// <summary>
    /// Initializes the supplied standard formulation and
    /// discovers available solver adapters using default
    /// discovery options.
    /// </summary>
    /// <param name="formulationOptions">
    /// Standard formulation options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel solver discovery and adapter
    /// loading.
    /// </param>
    /// <returns>
    /// Complete standard solver-bootstrap result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulationOptions"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static ValueTask<StandardLotSizingSolverBootstrapResult>
        InitializeAsync(
            Formulation.StandardLotSizingFormulationOptions
                formulationOptions,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            formulationOptions);

        return InitializeAsync(
            formulationOptions,
            new SolverDiscoveryOptions(),
            cancellationToken);
    }

    /// <summary>
    /// Initializes the supplied standard formulation and solver
    /// discovery configuration.
    /// </summary>
    /// <param name="formulationOptions">
    /// Standard formulation options.
    /// </param>
    /// <param name="discoveryOptions">
    /// Solver discovery options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel initialization.
    /// </param>
    /// <returns>
    /// Complete standard solver-bootstrap result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the option objects is
    /// <see langword="null"/>.
    /// </exception>
    public static async ValueTask<StandardLotSizingSolverBootstrapResult>
        InitializeAsync(
            Formulation.StandardLotSizingFormulationOptions
                formulationOptions,
            SolverDiscoveryOptions discoveryOptions,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            formulationOptions);

        ArgumentNullException.ThrowIfNull(
            discoveryOptions);

        Formulation.MathematicalModelFormulationRegistry
            formulationRegistry =
                Formulation.StandardLotSizingFormulationRegistryFactory
                    .Create(
                        formulationOptions);

        LotSizingSolverBootstrapResult bootstrapResult =
            await LotSizingSolverBootstrapper.InitializeAsync(
                formulationRegistry,
                discoveryOptions,
                cancellationToken);

        return new StandardLotSizingSolverBootstrapResult(
            formulationRegistry,
            bootstrapResult);
    }
}
