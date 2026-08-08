using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Selects a usable solver adapter according to the requested
/// solver kind, adapter capabilities, availability information,
/// and configured priority.
/// </summary>
public sealed class SolverSelectionService
{
    /// <summary>
    /// Initializes a new solver-selection service.
    /// </summary>
    public SolverSelectionService()
    {
    }

    /// <summary>
    /// Selects a solver adapter.
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
    /// Token used to cancel availability checks.
    /// </param>
    /// <returns>
    /// Solver-selection result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registry"/>,
    /// <paramref name="availabilityInformation"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public async ValueTask<SolverSelectionResult> SelectAsync(
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

        options.EnsureValid();

        var result =
            new SolverSelectionResult
            {
                RequestedSolver =
                    requestedSolver
            };

        if (requestedSolver ==
            SolverKind.Unknown)
        {
            result.AddDiagnostic(
                "The requested solver cannot be Unknown.");

            return result;
        }

        Dictionary<SolverKind, SolverAvailabilityInfo>
            availabilityBySolver =
                availabilityInformation
                    .Where(
                        information =>
                            information is not null)
                    .GroupBy(
                        information =>
                            information.SolverKind)
                    .ToDictionary(
                        group =>
                            group.Key,
                        group =>
                            group.First());

        IReadOnlyList<SolverKind> solverOrder =
            BuildSolverOrder(
                requestedSolver,
                options);

        foreach (
            SolverKind solverKind
            in solverOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<ISolverAdapter> adapters =
                registry.FindBySolverKind(
                    solverKind);

            if (adapters.Count == 0)
            {
                result.AddDiagnostic(
                    $"No loaded adapter is registered for " +
                    $"solver '{solverKind}'.");

                if (requestedSolver !=
                        SolverKind.Automatic &&
                    options.RequireExactSolverKind)
                {
                    return result;
                }

                continue;
            }

            SolverAvailabilityInfo? availability =
                availabilityBySolver.TryGetValue(
                    solverKind,
                    out SolverAvailabilityInfo?
                        discoveredAvailability)
                    ? discoveredAvailability
                    : null;

            foreach (
                ISolverAdapter adapter
                in adapters)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!SupportsRequiredCapabilities(
                        adapter,
                        options.RequiredCapabilities))
                {
                    result.AddDiagnostic(
                        $"Adapter '{adapter.AdapterName}' does " +
                        "not support all required " +
                        "capabilities.");

                    continue;
                }

                SolverAvailabilityInfo effectiveAvailability =
                    availability ??
                    await adapter.CheckAvailabilityAsync(
                        cancellationToken);

                if (!IsAvailabilityAccepted(
                        effectiveAvailability,
                        options))
                {
                    result.AddDiagnostic(
                        $"Adapter '{adapter.AdapterName}' is " +
                        $"not usable. Availability status: " +
                        $"'{effectiveAvailability.Status}'.");

                    continue;
                }

                result.SetSelection(
                    adapter,
                    effectiveAvailability);

                result.AddDiagnostic(
                    $"Adapter '{adapter.AdapterName}' was " +
                    $"selected for solver '{solverKind}'.");

                return result;
            }

            if (requestedSolver !=
                    SolverKind.Automatic &&
                options.RequireExactSolverKind)
            {
                return result;
            }
        }

        result.AddDiagnostic(
            "No suitable solver adapter could be selected.");

        return result;
    }

    private static IReadOnlyList<SolverKind>
        BuildSolverOrder(
            SolverKind requestedSolver,
            SolverSelectionOptions options)
    {
        if (requestedSolver ==
            SolverKind.Automatic)
        {
            return options.SolverPriority.ToArray();
        }

        if (options.RequireExactSolverKind)
        {
            return new[]
            {
                requestedSolver
            };
        }

        var solverOrder =
            new List<SolverKind>
            {
                requestedSolver
            };

        foreach (
            SolverKind solverKind
            in options.SolverPriority)
        {
            if (!solverOrder.Contains(
                    solverKind))
            {
                solverOrder.Add(
                    solverKind);
            }
        }

        return solverOrder;
    }

    private static bool SupportsRequiredCapabilities(
        ISolverAdapter adapter,
        IEnumerable<SolverCapability>
            requiredCapabilities)
    {
        foreach (
            SolverCapability capability
            in requiredCapabilities)
        {
            if (!adapter.SupportsCapability(
                    capability))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAvailabilityAccepted(
        SolverAvailabilityInfo availability,
        SolverSelectionOptions options)
    {
        if (availability.Status ==
            SolverAvailabilityStatus.Available)
        {
            return true;
        }

        return
            options.AllowLimitedAvailability &&
            availability.Status ==
                SolverAvailabilityStatus
                    .AvailableWithLimitations;
    }
}
