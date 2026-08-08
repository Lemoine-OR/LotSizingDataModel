using System.Threading;
using System.Threading.Tasks;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Defines a service that discovers solver adapter plugins and
/// native solver installations on the current computer.
/// </summary>
public interface ISolverDiscoveryService
{
    /// <summary>
    /// Discovers solver adapters and native solver
    /// installations.
    /// </summary>
    /// <param name="options">
    /// Solver-discovery options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the discovery operation.
    /// </param>
    /// <returns>
    /// Complete solver-discovery result.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    ValueTask<SolverDiscoveryResult> DiscoverAsync(
        SolverDiscoveryOptions options,
        CancellationToken cancellationToken = default);
}
