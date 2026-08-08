using System;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Represents the complete initialization result for the
/// standard lot-sizing solver infrastructure.
/// </summary>
public sealed class StandardLotSizingSolverBootstrapResult
{
    /// <summary>
    /// Initializes a standard solver-bootstrap result.
    /// </summary>
    /// <param name="formulationRegistry">
    /// Standard formulation registry created for the runtime.
    /// </param>
    /// <param name="bootstrapResult">
    /// Underlying solver-runtime bootstrap result.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when an argument is <see langword="null"/>.
    /// </exception>
    public StandardLotSizingSolverBootstrapResult(
        MathematicalModelFormulationRegistry formulationRegistry,
        LotSizingSolverBootstrapResult bootstrapResult)
    {
        ArgumentNullException.ThrowIfNull(
            formulationRegistry);

        ArgumentNullException.ThrowIfNull(
            bootstrapResult);

        FormulationRegistry =
            formulationRegistry;

        BootstrapResult =
            bootstrapResult;
    }

    /// <summary>
    /// Gets the formulation registry created for this runtime.
    /// </summary>
    public MathematicalModelFormulationRegistry FormulationRegistry
    {
        get;
    }

    /// <summary>
    /// Gets the underlying solver bootstrap result.
    /// </summary>
    public LotSizingSolverBootstrapResult BootstrapResult
    {
        get;
    }

    /// <summary>
    /// Gets the initialized high-level solver service, or
    /// <see langword="null"/> when initialization did not
    /// produce a service.
    /// </summary>
    public LotSizingSolverService? SolverService =>
        BootstrapResult.SolverService;

    /// <summary>
    /// Gets a value indicating whether a high-level solver
    /// service was initialized.
    /// </summary>
    public bool IsSuccessful =>
        BootstrapResult.IsSuccessful;

    /// <summary>
    /// Gets a value indicating whether at least one discovered
    /// solver can currently solve mathematical models.
    /// </summary>
    public bool CanSolve =>
        BootstrapResult.CanSolve;
}
