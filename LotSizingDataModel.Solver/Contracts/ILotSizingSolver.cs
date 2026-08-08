using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Events;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Contracts;

/// <summary>
/// Defines the common contract implemented by every
/// lot-sizing solver adapter.
/// </summary>
/// <remarks>
/// Implementations adapt a native optimization engine such as
/// CPLEX, Gurobi, FICO Xpress, or COIN-OR CBC to the common
/// solver model.
/// </remarks>
public interface ILotSizingSolver
{
    /// <summary>
    /// Occurs when new progress information is available.
    /// </summary>
    event EventHandler<SolverProgressEventArgs>?
        ProgressChanged;

    /// <summary>
    /// Gets the solver kind implemented by this adapter.
    /// </summary>
    SolverKind SolverKind
    {
        get;
    }

    /// <summary>
    /// Gets the display name of the solver.
    /// </summary>
    string SolverName
    {
        get;
    }

    /// <summary>
    /// Gets the detected solver version.
    /// </summary>
    string SolverVersion
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether a solve operation is
    /// currently running.
    /// </summary>
    bool IsRunning
    {
        get;
    }

    /// <summary>
    /// Checks whether the solver is installed, loadable, and
    /// licensed when a license is required.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the availability check.
    /// </param>
    /// <returns>
    /// Availability and installation information.
    /// </returns>
    ValueTask<SolverAvailabilityInfo> CheckAvailabilityAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a lot-sizing instance.
    /// </summary>
    /// <param name="request">
    /// Solver request.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to interrupt model generation or solver
    /// execution.
    /// </param>
    /// <returns>
    /// Normalized solver execution result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a second solve operation is requested while
    /// the solver is already running.
    /// </exception>
    Task<SolverRunResult> SolveAsync(
        SolverRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests interruption of the current solver execution.
    /// </summary>
    /// <remarks>
    /// The request is cooperative. The native solver adapter
    /// should forward it to the underlying solver as quickly as
    /// its callback or interruption API permits.
    /// </remarks>
    void RequestStop();
}
