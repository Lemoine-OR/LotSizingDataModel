using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Discovery;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Selects a solver adapter that supports direct execution of
/// solver-independent mathematical models.
/// </summary>
public sealed class MathematicalModelSolverSelectionService
{
    private readonly SolverSelectionService _solverSelectionService =
        new();

    /// <summary>
    /// Selects a usable mathematical-model solver adapter.
    /// </summary>
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
    /// <param name="options">
    /// Solver-selection options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel adapter selection and availability
    /// checks.
    /// </param>
    /// <returns>
    /// Mathematical-model solver-selection result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registry"/>,
    /// <paramref name="availabilityInformation"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public async ValueTask<MathematicalModelSolverSelectionResult>
        SelectAsync(
            SolverKind requestedSolver,
            SolverAdapterRegistry registry,
            IEnumerable<SolverAvailabilityInfo>
                availabilityInformation,
            SolverSelectionOptions options,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        ArgumentNullException.ThrowIfNull(
            availabilityInformation);

        ArgumentNullException.ThrowIfNull(
            options);

        SolverSelectionResult generalSelection =
            await _solverSelectionService.SelectAsync(
                requestedSolver,
                registry,
                availabilityInformation,
                options,
                cancellationToken);

        MathematicalModelSolverSelectionResult result =
            MathematicalModelSolverSelectionResult.From(
                generalSelection);

        if (generalSelection.IsSuccessful &&
            result.Adapter is null)
        {
            result.AddDiagnostic(
                "The selected solver adapter cannot execute a " +
                "solver-independent mathematical model.");

            result.SelectedSolver =
                SolverKind.Unknown;

            result.Availability =
                null;
        }

        return result;
    }
}
