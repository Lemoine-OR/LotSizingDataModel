using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.Contracts;

/// <summary>
/// Receives progress notifications produced during a solver
/// execution.
/// </summary>
/// <remarks>
/// Implementations may update a user interface, write a log,
/// store convergence history, or trigger an external action.
///
/// The observer must return quickly because some solver APIs
/// invoke progress notifications from their callback thread.
/// Long-running work should therefore be delegated to another
/// thread or queued asynchronously.
/// </remarks>
public interface ISolverProgressObserver
{
    /// <summary>
    /// Processes a solver progress snapshot.
    /// </summary>
    /// <param name="snapshot">
    /// Current solver progress information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that can be observed by the progress-processing
    /// implementation.
    /// </param>
    /// <returns>
    /// A task representing the progress-processing operation.
    /// </returns>
    ValueTask OnProgressAsync(
        SolverProgressSnapshot snapshot,
        CancellationToken cancellationToken = default);
}
