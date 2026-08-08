using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Discovery;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Defines a service that selects a compatible solver adapter
/// and executes an already built solver-independent
/// mathematical model.
/// </summary>
public interface IMathematicalModelSolverExecutionService
{
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
    /// Task returning the generic mathematical-model solve
    /// result.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    ValueTask<MathematicalModelSolveResult> SolveAsync(
        MathematicalModelSolveRequest request,
        SolverKind requestedSolver,
        SolverAdapterRegistry registry,
        IEnumerable<SolverAvailabilityInfo>
            availabilityInformation,
        SolverSelectionOptions selectionOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests interruption of the mathematical-model solve
    /// currently executed by the selected solver adapter.
    /// </summary>
    /// <remarks>
    /// Calling this method when no solver adapter is currently
    /// running has no effect. The selected adapter remains
    /// responsible for translating this request to its native
    /// solver API.
    /// </remarks>
    void RequestStop();
}
