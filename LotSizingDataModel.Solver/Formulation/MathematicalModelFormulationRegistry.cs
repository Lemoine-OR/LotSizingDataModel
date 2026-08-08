using System;
using System.Collections.Generic;
using System.Linq;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Stores and resolves solver-independent mathematical
/// formulations.
/// </summary>
public sealed class MathematicalModelFormulationRegistry
{
    private readonly Dictionary<string, IMathematicalModelFormulation>
        _formulations =
            new(
                StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of registered formulations.
    /// </summary>
    public int Count =>
        _formulations.Count;

    /// <summary>
    /// Registers a mathematical formulation.
    /// </summary>
    /// <param name="formulation">
    /// Formulation to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulation"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the formulation identifier is empty or is
    /// already registered.
    /// </exception>
    public void Register(
        IMathematicalModelFormulation formulation)
    {
        ArgumentNullException.ThrowIfNull(
            formulation);

        string formulationId =
            NormalizeFormulationId(
                formulation.FormulationId);

        if (!_formulations.TryAdd(
                formulationId,
                formulation))
        {
            throw new InvalidOperationException(
                $"Mathematical formulation '{formulationId}' is " +
                "already registered.");
        }
    }

    /// <summary>
    /// Registers or replaces a mathematical formulation.
    /// </summary>
    /// <param name="formulation">
    /// Formulation to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulation"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the formulation identifier is empty.
    /// </exception>
    public void RegisterOrReplace(
        IMathematicalModelFormulation formulation)
    {
        ArgumentNullException.ThrowIfNull(
            formulation);

        string formulationId =
            NormalizeFormulationId(
                formulation.FormulationId);

        _formulations[formulationId] =
            formulation;
    }

    /// <summary>
    /// Determines whether a formulation is registered.
    /// </summary>
    /// <param name="formulationId">
    /// Formulation identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the formulation is
    /// registered; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        string formulationId)
    {
        string normalizedFormulationId =
            NormalizeFormulationId(
                formulationId);

        return _formulations.ContainsKey(
            normalizedFormulationId);
    }

    /// <summary>
    /// Gets a required mathematical formulation.
    /// </summary>
    /// <param name="formulationId">
    /// Formulation identifier.
    /// </param>
    /// <returns>
    /// Registered mathematical formulation.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no formulation is registered for the supplied
    /// identifier.
    /// </exception>
    public IMathematicalModelFormulation GetRequired(
        string formulationId)
    {
        string normalizedFormulationId =
            NormalizeFormulationId(
                formulationId);

        if (_formulations.TryGetValue(
                normalizedFormulationId,
                out IMathematicalModelFormulation? formulation))
        {
            return formulation;
        }

        throw new KeyNotFoundException(
            $"No mathematical formulation is registered for " +
            $"identifier '{normalizedFormulationId}'.");
    }

    /// <summary>
    /// Attempts to get a registered mathematical formulation.
    /// </summary>
    /// <param name="formulationId">
    /// Formulation identifier.
    /// </param>
    /// <param name="formulation">
    /// Registered formulation when the method succeeds.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the formulation is found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(
        string formulationId,
        out IMathematicalModelFormulation? formulation)
    {
        string normalizedFormulationId =
            NormalizeFormulationId(
                formulationId);

        return _formulations.TryGetValue(
            normalizedFormulationId,
            out formulation);
    }

    /// <summary>
    /// Removes a registered mathematical formulation.
    /// </summary>
    /// <param name="formulationId">
    /// Formulation identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a formulation was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(
        string formulationId)
    {
        string normalizedFormulationId =
            NormalizeFormulationId(
                formulationId);

        return _formulations.Remove(
            normalizedFormulationId);
    }

    /// <summary>
    /// Returns all registered formulations ordered by name and
    /// identifier.
    /// </summary>
    /// <returns>
    /// Read-only formulation list.
    /// </returns>
    public IReadOnlyList<IMathematicalModelFormulation> GetAll()
    {
        return _formulations.Values
            .OrderBy(
                formulation =>
                    formulation.Name,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                formulation =>
                    formulation.FormulationId,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Removes all registered formulations.
    /// </summary>
    public void Clear()
    {
        _formulations.Clear();
    }

    private static string NormalizeFormulationId(
        string formulationId)
    {
        if (string.IsNullOrWhiteSpace(
                formulationId))
        {
            throw new InvalidOperationException(
                "A mathematical formulation identifier is " +
                "required.");
        }

        return formulationId.Trim();
    }
}
