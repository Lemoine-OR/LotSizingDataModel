using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Contracts;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Adapters;

/// <summary>
/// Provides common execution-state and cancellation handling for
/// solver adapters that solve solver-independent mathematical
/// models.
/// </summary>
public abstract class MathematicalModelSolverAdapterBase :
    IMathematicalModelSolver
{
    private readonly object _syncRoot =
        new();

    private CancellationTokenSource? _activeSolveCancellationSource;

    private bool _isRunning;

    /// <summary>
    /// Gets the solver kind.
    /// </summary>
    public abstract SolverKind SolverKind
    {
        get;
    }

    /// <summary>
    /// Gets the solver name.
    /// </summary>
    public abstract string SolverName
    {
        get;
    }

    /// <summary>
    /// Gets the solver version when available.
    /// </summary>
    public abstract string SolverVersion
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether a solve operation is
    /// currently running.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            lock (_syncRoot)
            {
                return _isRunning;
            }
        }
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another solve operation is already running.
    /// </exception>
    public async ValueTask<MathematicalModelSolveResult> SolveAsync(
        MathematicalModelSolveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        request.EnsureValid();

        CancellationTokenSource linkedCancellationSource =
            BeginSolve(
                cancellationToken);

        try
        {
            MathematicalModelSolveResult result =
                await SolveCoreAsync(
                    request,
                    linkedCancellationSource.Token);

            ArgumentNullException.ThrowIfNull(
                result);

            CompleteResultMetadata(
                request,
                result);

            result.EnsureValid();

            return result;
        }
        finally
        {
            EndSolve(
                linkedCancellationSource);
        }
    }

    /// <summary>
    /// Requests interruption of the currently running solve
    /// operation.
    /// </summary>
    public void RequestStop()
    {
        CancellationTokenSource? cancellationSource;

        lock (_syncRoot)
        {
            cancellationSource =
                _activeSolveCancellationSource;
        }

        if (cancellationSource is null)
        {
            return;
        }

        try
        {
            cancellationSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The solve operation completed concurrently.
        }

        RequestNativeStop();
    }

    /// <summary>
    /// Performs the solver-specific translation and solve.
    /// </summary>
    /// <param name="request">
    /// Validated mathematical-model solve request.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the native solve operation.
    /// </param>
    /// <returns>
    /// Task returning the solver result.
    /// </returns>
    protected abstract ValueTask<MathematicalModelSolveResult>
        SolveCoreAsync(
            MathematicalModelSolveRequest request,
            CancellationToken cancellationToken);

    /// <summary>
    /// Requests interruption through the native solver API.
    /// </summary>
    /// <remarks>
    /// Derived adapters should override this method when the
    /// native solver exposes an explicit interruption mechanism.
    /// </remarks>
    protected virtual void RequestNativeStop()
    {
    }

    /// <summary>
    /// Completes generic result metadata when it was not supplied
    /// by the native adapter.
    /// </summary>
    /// <param name="request">
    /// Source solve request.
    /// </param>
    /// <param name="result">
    /// Solver result to complete.
    /// </param>
    protected virtual void CompleteResultMetadata(
        MathematicalModelSolveRequest request,
        MathematicalModelSolveResult result)
    {
        if (string.IsNullOrWhiteSpace(
                result.RunName))
        {
            result.RunName =
                request.RunName;
        }

        if (string.IsNullOrWhiteSpace(
                result.FormulationId))
        {
            result.FormulationId =
                request.FormulationId;
        }

        if (result.SolverKind ==
            SolverKind.Unknown)
        {
            result.SolverKind =
                SolverKind;
        }

        if (string.IsNullOrWhiteSpace(
                result.SolverName))
        {
            result.SolverName =
                SolverName;
        }

        if (string.IsNullOrWhiteSpace(
                result.SolverVersion))
        {
            result.SolverVersion =
                SolverVersion;
        }
    }

    private CancellationTokenSource BeginSolve(
        CancellationToken cancellationToken)
    {
        lock (_syncRoot)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException(
                    $"Solver '{SolverName}' is already running.");
            }

            var linkedCancellationSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            _activeSolveCancellationSource =
                linkedCancellationSource;

            _isRunning =
                true;

            return linkedCancellationSource;
        }
    }

    private void EndSolve(
        CancellationTokenSource cancellationSource)
    {
        lock (_syncRoot)
        {
            if (ReferenceEquals(
                    _activeSolveCancellationSource,
                    cancellationSource))
            {
                _activeSolveCancellationSource =
                    null;

                _isRunning =
                    false;
            }
        }

        cancellationSource.Dispose();
    }
}
