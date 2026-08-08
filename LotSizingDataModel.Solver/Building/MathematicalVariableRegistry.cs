using System;
using System.Collections.Generic;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Building;

/// <summary>
/// Maintains the correspondence between business-domain keys
/// and mathematical variables during model construction.
/// </summary>
/// <remarks>
/// A domain key identifies one lot-sizing decision independently
/// of the solver-specific variable object. Examples include:
/// <code>
/// production|item=1|plant=2|period=3
/// inventory|item=1|warehouse=4|period=3
/// setup|item=1|resource=5|period=3
/// </code>
/// </remarks>
public sealed class MathematicalVariableRegistry
{
    private readonly Dictionary<string, MathematicalVariable>
        _variablesByDomainKey =
            new(
                StringComparer.Ordinal);

    /// <summary>
    /// Gets the number of registered variables.
    /// </summary>
    public int Count =>
        _variablesByDomainKey.Count;

    /// <summary>
    /// Registers a mathematical variable using its domain key.
    /// </summary>
    /// <param name="variable">
    /// Mathematical variable to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variable"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the variable has no domain key or when the
    /// domain key is already registered.
    /// </exception>
    public void Register(
        MathematicalVariable variable)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        variable.EnsureValid();

        string domainKey =
            NormalizeDomainKey(
                variable.DomainKey);

        if (!_variablesByDomainKey.TryAdd(
                domainKey,
                variable))
        {
            throw new InvalidOperationException(
                $"The mathematical variable domain key " +
                $"'{domainKey}' is already registered.");
        }
    }

    /// <summary>
    /// Registers a mathematical variable with an explicit
    /// domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <param name="variable">
    /// Mathematical variable to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variable"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the key conflicts with the variable domain key
    /// or is already registered.
    /// </exception>
    public void Register(
        string domainKey,
        MathematicalVariable variable)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        variable.EnsureValid();

        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        if (!string.IsNullOrWhiteSpace(
                variable.DomainKey) &&
            !string.Equals(
                variable.DomainKey.Trim(),
                normalizedDomainKey,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The supplied domain key does not match the " +
                "mathematical variable domain key.");
        }

        variable.DomainKey =
            normalizedDomainKey;

        Register(
            variable);
    }

    /// <summary>
    /// Determines whether a variable is registered for a domain
    /// key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a variable is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        string domainKey)
    {
        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        return _variablesByDomainKey.ContainsKey(
            normalizedDomainKey);
    }

    /// <summary>
    /// Gets the variable registered for a domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <returns>
    /// Registered mathematical variable.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no variable is registered for the supplied
    /// domain key.
    /// </exception>
    public MathematicalVariable GetRequired(
        string domainKey)
    {
        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        if (_variablesByDomainKey.TryGetValue(
                normalizedDomainKey,
                out MathematicalVariable? variable))
        {
            return variable;
        }

        throw new KeyNotFoundException(
            $"No mathematical variable is registered for domain " +
            $"key '{normalizedDomainKey}'.");
    }

    /// <summary>
    /// Attempts to get the variable registered for a domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <param name="variable">
    /// Registered variable when the method succeeds.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a variable is found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(
        string domainKey,
        out MathematicalVariable? variable)
    {
        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        return _variablesByDomainKey.TryGetValue(
            normalizedDomainKey,
            out variable);
    }

    /// <summary>
    /// Removes the variable registered for a domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a variable was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(
        string domainKey)
    {
        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        return _variablesByDomainKey.Remove(
            normalizedDomainKey);
    }

    /// <summary>
    /// Removes all registered variables.
    /// </summary>
    public void Clear()
    {
        _variablesByDomainKey.Clear();
    }

    /// <summary>
    /// Returns a snapshot of all registered variables.
    /// </summary>
    /// <returns>
    /// Read-only dictionary indexed by domain key.
    /// </returns>
    public IReadOnlyDictionary<string, MathematicalVariable>
        GetSnapshot()
    {
        return new Dictionary<string, MathematicalVariable>(
            _variablesByDomainKey,
            StringComparer.Ordinal);
    }

    private static string NormalizeDomainKey(
        string domainKey)
    {
        if (string.IsNullOrWhiteSpace(
                domainKey))
        {
            throw new InvalidOperationException(
                "A mathematical variable domain key is required.");
        }

        return domainKey.Trim();
    }
}
