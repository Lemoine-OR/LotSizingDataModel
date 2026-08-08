using System;
using System.Collections.Generic;
using System.Linq;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Stores and resolves mathematical decision mappers by
/// business-domain category.
/// </summary>
public sealed class MathematicalDecisionMapperRegistry
{
    private readonly Dictionary<string, IMathematicalDecisionMapper>
        _mappers =
            new(
                StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of registered decision mappers.
    /// </summary>
    public int Count =>
        _mappers.Count;

    /// <summary>
    /// Registers a mathematical decision mapper.
    /// </summary>
    /// <param name="mapper">
    /// Decision mapper to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="mapper"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mapper category is empty or already
    /// registered.
    /// </exception>
    public void Register(
        IMathematicalDecisionMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(
            mapper);

        string category =
            NormalizeCategory(
                mapper.Category);

        if (!_mappers.TryAdd(
                category,
                mapper))
        {
            throw new InvalidOperationException(
                $"A mathematical decision mapper for category " +
                $"'{category}' is already registered.");
        }
    }

    /// <summary>
    /// Registers or replaces a mathematical decision mapper.
    /// </summary>
    /// <param name="mapper">
    /// Decision mapper to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="mapper"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mapper category is empty.
    /// </exception>
    public void RegisterOrReplace(
        IMathematicalDecisionMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(
            mapper);

        string category =
            NormalizeCategory(
                mapper.Category);

        _mappers[category] =
            mapper;
    }

    /// <summary>
    /// Determines whether a decision mapper is registered for a
    /// category.
    /// </summary>
    /// <param name="category">
    /// Business-domain category.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a mapper is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        string category)
    {
        return _mappers.ContainsKey(
            NormalizeCategory(
                category));
    }

    /// <summary>
    /// Gets the required decision mapper for a category.
    /// </summary>
    /// <param name="category">
    /// Business-domain category.
    /// </param>
    /// <returns>
    /// Registered decision mapper.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no mapper is registered for the supplied
    /// category.
    /// </exception>
    public IMathematicalDecisionMapper GetRequired(
        string category)
    {
        string normalizedCategory =
            NormalizeCategory(
                category);

        if (_mappers.TryGetValue(
                normalizedCategory,
                out IMathematicalDecisionMapper? mapper))
        {
            return mapper;
        }

        throw new KeyNotFoundException(
            $"No mathematical decision mapper is registered for " +
            $"category '{normalizedCategory}'.");
    }

    /// <summary>
    /// Attempts to get a decision mapper for a category.
    /// </summary>
    /// <param name="category">
    /// Business-domain category.
    /// </param>
    /// <param name="mapper">
    /// Registered mapper when found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a mapper is found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGet(
        string category,
        out IMathematicalDecisionMapper? mapper)
    {
        return _mappers.TryGetValue(
            NormalizeCategory(
                category),
            out mapper);
    }

    /// <summary>
    /// Returns all registered decision mappers ordered by
    /// category.
    /// </summary>
    /// <returns>
    /// Read-only list of registered decision mappers.
    /// </returns>
    public IReadOnlyList<IMathematicalDecisionMapper> GetAll()
    {
        return _mappers.Values
            .OrderBy(
                mapper =>
                    mapper.Category,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Removes a decision mapper.
    /// </summary>
    /// <param name="category">
    /// Business-domain category.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a mapper was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(
        string category)
    {
        return _mappers.Remove(
            NormalizeCategory(
                category));
    }

    /// <summary>
    /// Removes all registered decision mappers.
    /// </summary>
    public void Clear()
    {
        _mappers.Clear();
    }

    private static string NormalizeCategory(
        string category)
    {
        if (string.IsNullOrWhiteSpace(
                category))
        {
            throw new InvalidOperationException(
                "A mathematical decision-mapper category is " +
                "required.");
        }

        return category.Trim();
    }
}
