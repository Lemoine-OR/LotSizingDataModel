using System;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.Events;

/// <summary>
/// Provides solver progress information to .NET event
/// subscribers.
/// </summary>
public sealed class SolverProgressEventArgs :
    EventArgs
{
    /// <summary>
    /// Initializes a new solver progress event.
    /// </summary>
    /// <param name="snapshot">
    /// Current solver progress snapshot.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot"/> is
    /// <see langword="null"/>.
    /// </exception>
    public SolverProgressEventArgs(
        SolverProgressSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(
            snapshot);

        Snapshot =
            snapshot;
    }

    /// <summary>
    /// Gets the solver progress snapshot associated with the
    /// event.
    /// </summary>
    public SolverProgressSnapshot Snapshot
    {
        get;
    }
}
