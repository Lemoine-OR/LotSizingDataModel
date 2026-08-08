using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Adapters;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Discovery;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Selects a compatible mathematical-model solver adapter and
/// executes an already built solver-independent mathematical
/// model.
/// </summary>
public sealed class MathematicalModelSolverExecutionService :
    IMathematicalModelSolverExecutionService
{
    private readonly MathematicalModelSolverSelectionService
        _selectionService =
            new();

    private readonly object _synchronizationRoot =
        new();

    private IMathematicalModelSolverAdapter? _activeAdapter;

    /// <summary>
    /// Selects a solver adapter and solves the mathematical
    /// model.
    /// </summary>
    /// <param name="request">
    /// Mathematical-model solve request.
    /// </param>
    /// <param name="requestedSolver">
    /// Solver requested by the caller.
    /// </param>
    /// <param name="registry">
    /// Registry containing loaded solver adapters.
    /// </param>
    /// <param name="availabilityInformation">
    /// Availability information associated with detected
    /// solvers.
    /// </param>
    /// <param name="selectionOptions">
    /// Solver-selection options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel solver selection and execution.
    /// </param>
    /// <returns>
    /// Generic mathematical-model solve result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another mathematical-model solve is already
    /// running through this execution service.
    /// </exception>
    public async ValueTask<MathematicalModelSolveResult> SolveAsync(
        MathematicalModelSolveRequest request,
        SolverKind requestedSolver,
        SolverAdapterRegistry registry,
        IEnumerable<SolverAvailabilityInfo>
            availabilityInformation,
        SolverSelectionOptions selectionOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        ArgumentNullException.ThrowIfNull(
            registry);

        ArgumentNullException.ThrowIfNull(
            availabilityInformation);

        ArgumentNullException.ThrowIfNull(
            selectionOptions);

        request.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        MathematicalModelSolverSelectionResult selection =
            await _selectionService.SelectAsync(
                requestedSolver,
                registry,
                availabilityInformation,
                selectionOptions,
                cancellationToken);

        if (!selection.IsSuccessful ||
            selection.Adapter is null)
        {
            var unavailableResult =
                new MathematicalModelSolveResult
                {
                    RunName =
                        request.RunName,

                    FormulationId =
                        request.FormulationId,

                    SolverKind =
                        selection.SelectedSolver,

                    SolverName =
                        selection.AdapterName,

                    TerminationReason =
                        SolverTerminationReason.SolverUnavailable,

                    HasFeasibleSolution =
                        false,

                    IsOptimal =
                        false
                };

            foreach (
                string diagnostic
                in selection.Diagnostics)
            {
                unavailableResult.AddDiagnostic(
                    diagnostic);
            }

            unavailableResult.AddDiagnostic(
                "No usable mathematical-model solver adapter " +
                "could be selected.");

            unavailableResult.EnsureValid();

            return unavailableResult;
        }

        SetActiveAdapter(
            selection.Adapter);

        try
        {
            MathematicalModelSolveResult result =
                await selection.Adapter.SolveAsync(
                    request,
                    cancellationToken);

            foreach (
                string diagnostic
                in selection.Diagnostics)
            {
                result.AddDiagnostic(
                    diagnostic);
            }

            result.EnsureValid();

            return result;
        }
        finally
        {
            ClearActiveAdapter(
                selection.Adapter);
        }
    }

    /// <summary>
    /// Requests interruption of the mathematical-model solve
    /// currently executed by the selected solver adapter.
    /// </summary>
    /// <remarks>
    /// Calling this method when no solver adapter is currently
    /// running has no effect.
    /// </remarks>
    public void RequestStop()
    {
        IMathematicalModelSolverAdapter? adapter;

        lock (_synchronizationRoot)
        {
            adapter =
                _activeAdapter;
        }

        if (adapter is not null)
        {
            ((global::LotSizingDataModel.Solver.Contracts.IMathematicalModelSolver)adapter)
                .RequestStop();
        }
    }

    private void SetActiveAdapter(
        IMathematicalModelSolverAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(
            adapter);

        lock (_synchronizationRoot)
        {
            if (_activeAdapter is not null)
            {
                throw new InvalidOperationException(
                    "Another mathematical-model solve is already " +
                    "running through this execution service.");
            }

            _activeAdapter =
                adapter;
        }
    }

    private void ClearActiveAdapter(
        IMathematicalModelSolverAdapter adapter)
    {
        lock (_synchronizationRoot)
        {
            if (ReferenceEquals(
                    _activeAdapter,
                    adapter))
            {
                _activeAdapter =
                    null;
            }
        }
    }
}
