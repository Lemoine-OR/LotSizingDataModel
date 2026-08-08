using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Contracts;

/// <summary>
/// Defines a solver adapter capable of solving an already built,
/// solver-independent mathematical model.
/// </summary>
/// <remarks>
/// Implementations translate the generic mathematical model
/// contained in <see cref="MathematicalModelSolveRequest"/> into
/// the native representation of a specific optimization engine.
/// They must not rebuild lot-sizing equations from the original
/// business instance.
/// </remarks>
public interface IMathematicalModelSolver
{
    /// <summary>
    /// Gets the solver kind.
    /// </summary>
    SolverKind SolverKind
    {
        get;
    }

    /// <summary>
    /// Gets the solver name.
    /// </summary>
    string SolverName
    {
        get;
    }

    /// <summary>
    /// Gets the solver version when available.
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
    /// Solves a solver-independent mathematical model.
    /// </summary>
    /// <param name="request">
    /// Mathematical-model solve request.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the solve operation.
    /// </param>
    /// <returns>
    /// Task returning the generic mathematical-model solve
    /// result.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="request"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the request is invalid or another solve
    /// operation is already running.
    /// </exception>
    ValueTask<MathematicalModelSolveResult> SolveAsync(
        MathematicalModelSolveRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests interruption of the currently running solve
    /// operation.
    /// </summary>
    /// <remarks>
    /// The request is cooperative. Native solver adapters should
    /// forward it to the corresponding solver interruption
    /// mechanism whenever available.
    /// </remarks>
    void RequestStop();
}
