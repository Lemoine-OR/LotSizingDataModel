using System;
using System.Threading;
using System.Threading.Tasks;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Provides convenient factory methods for constructing the
/// default solver runtime without exposing the discovery and
/// adapter-loading implementation details to application code.
/// </summary>
public static class SolverRuntimeFactory
{
    /// <summary>
    /// Builds the default solver runtime using standard
    /// discovery and adapter-loading services.
    /// </summary>
    /// <param name="options">
    /// Solver discovery options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel solver discovery and adapter
    /// loading.
    /// </param>
    /// <returns>
    /// Complete solver-runtime build result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static ValueTask<SolverRuntimeBuildResult> BuildAsync(
        SolverDiscoveryOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        var discoveryService =
            new DefaultSolverDiscoveryService();

        var runtimeBuilder =
            new SolverRuntimeBuilder(
                discoveryService);

        return runtimeBuilder.BuildAsync(
            options,
            cancellationToken);
    }

    /// <summary>
    /// Builds the default solver runtime using default discovery
    /// options.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel solver discovery and adapter
    /// loading.
    /// </param>
    /// <returns>
    /// Complete solver-runtime build result.
    /// </returns>
    public static ValueTask<SolverRuntimeBuildResult>
        BuildDefaultAsync(
            CancellationToken cancellationToken = default)
    {
        return BuildAsync(
            new SolverDiscoveryOptions(),
            cancellationToken);
    }
}
