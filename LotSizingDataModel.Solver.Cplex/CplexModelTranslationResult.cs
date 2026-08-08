using System;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;
using NativeCplex = global::ILOG.CPLEX.Cplex;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Stores the native CPLEX model objects created from one
/// solver-independent mathematical model.
/// </summary>
public sealed class CplexModelTranslationResult
{
    private readonly IReadOnlyDictionary<int, INumVar>
        _variablesById;

    /// <summary>
    /// Initializes a CPLEX translation result.
    /// </summary>
    /// <param name="cplex">
    /// Native CPLEX model.
    /// </param>
    /// <param name="variablesById">
    /// Mapping from generic mathematical-variable identifiers to
    /// native Concert variables.
    /// </param>
    public CplexModelTranslationResult(
        NativeCplex cplex,
        IReadOnlyDictionary<int, INumVar> variablesById)
    {
        ArgumentNullException.ThrowIfNull(cplex);
        ArgumentNullException.ThrowIfNull(variablesById);

        Cplex = cplex;
        _variablesById = variablesById;
    }

    /// <summary>
    /// Gets the native CPLEX model.
    /// </summary>
    public NativeCplex Cplex
    {
        get;
    }

    /// <summary>
    /// Gets the native variables indexed by generic identifier.
    /// </summary>
    public IReadOnlyDictionary<int, INumVar> VariablesById =>
        _variablesById;

    /// <summary>
    /// Resolves a native variable by generic mathematical
    /// identifier.
    /// </summary>
    /// <param name="variableId">
    /// Generic mathematical-variable identifier.
    /// </param>
    /// <returns>
    /// Native Concert variable.
    /// </returns>
    public INumVar GetVariable(
        int variableId)
    {
        if (!_variablesById.TryGetValue(
                variableId,
                out INumVar? variable))
        {
            throw new InvalidOperationException(
                $"No native CPLEX variable is registered for " +
                $"mathematical variable identifier {variableId}.");
        }

        return variable;
    }
}
