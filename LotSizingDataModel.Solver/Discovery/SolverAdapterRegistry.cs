using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Stores solver adapters that were loaded successfully and
/// provides lookup services by adapter identifier or solver
/// kind.
/// </summary>
public sealed class SolverAdapterRegistry
{
    private readonly Dictionary<string, ISolverAdapter>
        _adaptersById =
            new(
                StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<SolverKind, List<ISolverAdapter>>
        _adaptersBySolverKind =
            new();

    /// <summary>
    /// Initializes an empty solver-adapter registry.
    /// </summary>
    public SolverAdapterRegistry()
    {
    }

    /// <summary>
    /// Gets all registered solver adapters.
    /// </summary>
    public IReadOnlyCollection<ISolverAdapter> Adapters =>
        _adaptersById.Values.ToArray();

    /// <summary>
    /// Gets the number of registered adapters.
    /// </summary>
    public int Count =>
        _adaptersById.Count;

    /// <summary>
    /// Registers a solver adapter.
    /// </summary>
    /// <param name="adapter">
    /// Adapter to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="adapter"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the adapter identifier is empty, the solver
    /// kind is not concrete, or another adapter already uses the
    /// same identifier.
    /// </exception>
    public void Register(
        ISolverAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(
            adapter);

        if (string.IsNullOrWhiteSpace(
                adapter.AdapterId))
        {
            throw new InvalidOperationException(
                "A solver adapter identifier is required.");
        }

        if (adapter.SolverKind is
            SolverKind.Unknown or
            SolverKind.Automatic)
        {
            throw new InvalidOperationException(
                "A solver adapter must target a concrete " +
                "solver kind.");
        }

        string adapterId =
            adapter.AdapterId.Trim();

        if (_adaptersById.ContainsKey(
                adapterId))
        {
            throw new InvalidOperationException(
                $"A solver adapter with identifier " +
                $"'{adapterId}' is already registered.");
        }

        _adaptersById.Add(
            adapterId,
            adapter);

        if (!_adaptersBySolverKind.TryGetValue(
                adapter.SolverKind,
                out List<ISolverAdapter>? solverAdapters))
        {
            solverAdapters =
                new List<ISolverAdapter>();

            _adaptersBySolverKind.Add(
                adapter.SolverKind,
                solverAdapters);
        }

        solverAdapters.Add(
            adapter);
    }

    /// <summary>
    /// Registers an adapter from a successful load result.
    /// </summary>
    /// <param name="loadResult">
    /// Adapter-loading result.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the adapter was registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loadResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    public bool TryRegister(
        SolverAdapterLoadResult loadResult)
    {
        ArgumentNullException.ThrowIfNull(
            loadResult);

        if (!loadResult.IsLoaded ||
            loadResult.Adapter is null)
        {
            return false;
        }

        if (_adaptersById.ContainsKey(
                loadResult.Adapter.AdapterId))
        {
            return false;
        }

        Register(
            loadResult.Adapter);

        return true;
    }

    /// <summary>
    /// Finds a registered adapter by identifier.
    /// </summary>
    /// <param name="adapterId">
    /// Adapter identifier.
    /// </param>
    /// <returns>
    /// Matching adapter, or <see langword="null"/> when no
    /// adapter is registered with that identifier.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="adapterId"/> is empty.
    /// </exception>
    public ISolverAdapter? FindById(
        string adapterId)
    {
        if (string.IsNullOrWhiteSpace(
                adapterId))
        {
            throw new ArgumentException(
                "A solver adapter identifier is required.",
                nameof(adapterId));
        }

        _adaptersById.TryGetValue(
            adapterId.Trim(),
            out ISolverAdapter? adapter);

        return adapter;
    }

    /// <summary>
    /// Gets all registered adapters targeting a solver kind.
    /// </summary>
    /// <param name="solverKind">
    /// Concrete solver kind.
    /// </param>
    /// <returns>
    /// Registered adapters for the selected solver.
    /// </returns>
    public IReadOnlyList<ISolverAdapter> FindBySolverKind(
        SolverKind solverKind)
    {
        if (!_adaptersBySolverKind.TryGetValue(
                solverKind,
                out List<ISolverAdapter>? adapters))
        {
            return Array.Empty<ISolverAdapter>();
        }

        return adapters.ToArray();
    }

    /// <summary>
    /// Gets the first registered adapter targeting a solver
    /// kind.
    /// </summary>
    /// <param name="solverKind">
    /// Concrete solver kind.
    /// </param>
    /// <returns>
    /// First matching adapter, or <see langword="null"/> when
    /// none is registered.
    /// </returns>
    public ISolverAdapter? FindFirstBySolverKind(
        SolverKind solverKind)
    {
        if (!_adaptersBySolverKind.TryGetValue(
                solverKind,
                out List<ISolverAdapter>? adapters) ||
            adapters.Count == 0)
        {
            return null;
        }

        return adapters[0];
    }

    /// <summary>
    /// Removes a registered adapter.
    /// </summary>
    /// <param name="adapterId">
    /// Adapter identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when an adapter was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(
        string adapterId)
    {
        ISolverAdapter? adapter =
            FindById(
                adapterId);

        if (adapter is null)
        {
            return false;
        }

        _adaptersById.Remove(
            adapterId.Trim());

        if (_adaptersBySolverKind.TryGetValue(
                adapter.SolverKind,
                out List<ISolverAdapter>? solverAdapters))
        {
            solverAdapters.Remove(
                adapter);

            if (solverAdapters.Count == 0)
            {
                _adaptersBySolverKind.Remove(
                    adapter.SolverKind);
            }
        }

        return true;
    }

    /// <summary>
    /// Removes all registered adapters.
    /// </summary>
    public void Clear()
    {
        _adaptersById.Clear();
        _adaptersBySolverKind.Clear();
    }
}
