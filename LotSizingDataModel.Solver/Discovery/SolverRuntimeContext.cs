using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Represents the solver runtime infrastructure available to
/// the application after adapter discovery and loading.
/// </summary>
/// <remarks>
/// This class groups the loaded adapter registry and the
/// normalized native-solver availability information so they
/// can be passed together to the high-level solver service.
/// </remarks>
public sealed class SolverRuntimeContext
{
    private readonly IReadOnlyList<SolverAvailabilityInfo>
        _availabilityInformation;

    /// <summary>
    /// Initializes a solver runtime context.
    /// </summary>
    /// <param name="adapterRegistry">
    /// Registry containing the solver adapters that were loaded
    /// successfully.
    /// </param>
    /// <param name="availabilityInformation">
    /// Native-solver availability information.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="adapterRegistry"/> or
    /// <paramref name="availabilityInformation"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the availability collection contains an
    /// invalid entry.
    /// </exception>
    public SolverRuntimeContext(
        SolverAdapterRegistry adapterRegistry,
        IEnumerable<SolverAvailabilityInfo> availabilityInformation)
    {
        ArgumentNullException.ThrowIfNull(
            adapterRegistry);

        ArgumentNullException.ThrowIfNull(
            availabilityInformation);

        SolverAvailabilityInfo[] availability =
            availabilityInformation.ToArray();

        if (availability.Any(
                item =>
                    item is null))
        {
            throw new InvalidOperationException(
                "The solver availability collection cannot " +
                "contain a null entry.");
        }

        AdapterRegistry =
            adapterRegistry;

        _availabilityInformation =
            availability;
    }

    /// <summary>
    /// Gets the registry containing all solver adapters loaded
    /// successfully for the current runtime.
    /// </summary>
    public SolverAdapterRegistry AdapterRegistry
    {
        get;
    }

    /// <summary>
    /// Gets the normalized availability information for the
    /// detected native solvers.
    /// </summary>
    public IReadOnlyList<SolverAvailabilityInfo>
        AvailabilityInformation =>
            _availabilityInformation;

    /// <summary>
    /// Gets a value indicating whether at least one loaded
    /// adapter is available.
    /// </summary>
    public bool HasLoadedAdapter =>
        AdapterRegistry.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one detected
    /// native solver is currently reported as usable.
    /// </summary>
    public bool HasUsableSolver =>
        _availabilityInformation.Any(
            availability =>
                availability.IsUsable);

    /// <summary>
    /// Gets a value indicating whether the runtime contains at
    /// least one loaded adapter whose solver is reported as
    /// usable.
    /// </summary>
    public bool CanSolve =>
        AdapterRegistry.Adapters.Any(
            adapter =>
                _availabilityInformation.Any(
                    availability =>
                        availability.SolverKind ==
                            adapter.SolverKind &&
                        availability.IsUsable));

    /// <summary>
    /// Finds the availability information for a solver kind.
    /// </summary>
    /// <param name="solverKind">
    /// Solver kind to locate.
    /// </param>
    /// <returns>
    /// Matching availability information, or
    /// <see langword="null"/> when no availability information
    /// exists for the requested solver.
    /// </returns>
    public SolverAvailabilityInfo? FindAvailability(
        SolverKind solverKind)
    {
        return _availabilityInformation
            .FirstOrDefault(
                availability =>
                    availability.SolverKind ==
                        solverKind);
    }
}
