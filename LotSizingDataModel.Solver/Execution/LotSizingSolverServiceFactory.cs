using System;
using LotSizingDataModel.Solver.Discovery;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Provides convenient factory methods for creating the
/// high-level lot-sizing solver service from a prepared solver
/// runtime.
/// </summary>
public static class LotSizingSolverServiceFactory
{
    /// <summary>
    /// Creates the high-level solver service using the supplied
    /// formulation registry and solver runtime context.
    /// </summary>
    /// <param name="formulationRegistry">
    /// Registry containing the mathematical formulations
    /// available to the application.
    /// </param>
    /// <param name="runtimeContext">
    /// Prepared runtime containing loaded solver adapters and
    /// normalized solver availability information.
    /// </param>
    /// <returns>
    /// Configured high-level lot-sizing solver service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulationRegistry"/> or
    /// <paramref name="runtimeContext"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static LotSizingSolverService Create(
        MathematicalModelFormulationRegistry formulationRegistry,
        SolverRuntimeContext runtimeContext)
    {
        ArgumentNullException.ThrowIfNull(
            formulationRegistry);

        ArgumentNullException.ThrowIfNull(
            runtimeContext);

        return new LotSizingSolverService(
            formulationRegistry,
            runtimeContext.AdapterRegistry,
            runtimeContext.AvailabilityInformation);
    }

    /// <summary>
    /// Creates the high-level solver service directly from a
    /// successful solver-runtime build result.
    /// </summary>
    /// <param name="formulationRegistry">
    /// Registry containing the mathematical formulations
    /// available to the application.
    /// </param>
    /// <param name="runtimeBuildResult">
    /// Solver-runtime build result.
    /// </param>
    /// <returns>
    /// Configured high-level lot-sizing solver service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the supplied arguments is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the runtime build result does not contain a
    /// valid runtime context.
    /// </exception>
    public static LotSizingSolverService Create(
        MathematicalModelFormulationRegistry formulationRegistry,
        SolverRuntimeBuildResult runtimeBuildResult)
    {
        ArgumentNullException.ThrowIfNull(
            formulationRegistry);

        ArgumentNullException.ThrowIfNull(
            runtimeBuildResult);

        if (runtimeBuildResult.RuntimeContext is null)
        {
            throw new InvalidOperationException(
                "The solver runtime build result does not contain " +
                "a valid runtime context.");
        }

        return Create(
            formulationRegistry,
            runtimeBuildResult.RuntimeContext);
    }
}
