using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Provides the shared state and lookup services required while
/// mapping a mathematical solver result back to the lot-sizing
/// domain model.
/// </summary>
public sealed class MathematicalSolutionMappingContext
{
    private readonly Dictionary<int, MathematicalVariable>
        _variablesById;

    private readonly Dictionary<int, MathematicalVariableValue>
        _valuesByVariableId;

    private readonly Dictionary<string, MathematicalVariableValue>
        _valuesByDomainKey;

    /// <summary>
    /// Initializes a mathematical-solution mapping context.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="model">
    /// Solver-independent mathematical model.
    /// </param>
    /// <param name="solveResult">
    /// Generic mathematical-model solve result.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the model and solver result are inconsistent.
    /// </exception>
    public MathematicalSolutionMappingContext(
        LotSizingInstance instance,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult)
        : this(
            instance,
            model,
            solveResult,
            new MathematicalSolutionMappingOptions())
    {
    }

    /// <summary>
    /// Initializes a mathematical-solution mapping context with
    /// explicit mapping options.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="model">
    /// Solver-independent mathematical model.
    /// </param>
    /// <param name="solveResult">
    /// Generic mathematical-model solve result.
    /// </param>
    /// <param name="options">
    /// Mapping options to apply.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the model, solver result, or options are
    /// inconsistent.
    /// </exception>
    public MathematicalSolutionMappingContext(
        LotSizingInstance instance,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        MathematicalSolutionMappingOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            model);

        ArgumentNullException.ThrowIfNull(
            solveResult);

        ArgumentNullException.ThrowIfNull(
            options);

        model.EnsureValid();
        solveResult.EnsureValid();

        MathematicalSolutionMappingOptions normalizedOptions =
            options.Clone();

        normalizedOptions.EnsureValid();

        Instance =
            instance;

        Model =
            model;

        SolveResult =
            solveResult;

        Options =
            normalizedOptions;

        ValueNormalizer =
            new MathematicalVariableValueNormalizer(
                normalizedOptions.ZeroTolerance,
                MathematicalVariableValueNormalizer
                    .DefaultIntegralityTolerance,
                MathematicalVariableValueNormalizer
                    .DefaultNearIntegerTolerance);

        _variablesById =
            model.Variables.ToDictionary(
                variable =>
                    variable.Id);

        _valuesByVariableId =
            solveResult.VariableValues.ToDictionary(
                variableValue =>
                    variableValue.VariableId);

        _valuesByDomainKey =
            new Dictionary<string, MathematicalVariableValue>(
                StringComparer.OrdinalIgnoreCase);

        BuildIndexes();
    }

    /// <summary>
    /// Gets the source lot-sizing instance.
    /// </summary>
    public LotSizingInstance Instance
    {
        get;
    }

    /// <summary>
    /// Gets the solver-independent mathematical model.
    /// </summary>
    public MathematicalModel Model
    {
        get;
    }

    /// <summary>
    /// Gets the generic mathematical-model solve result.
    /// </summary>
    public MathematicalModelSolveResult SolveResult
    {
        get;
    }

    /// <summary>
    /// Gets the normalized mapping options used by this context.
    /// </summary>
    public MathematicalSolutionMappingOptions Options
    {
        get;
    }

    /// <summary>
    /// Gets the numerical normalizer used for all mathematical
    /// variable values exposed to decision mappers.
    /// </summary>
    public MathematicalVariableValueNormalizer ValueNormalizer
    {
        get;
    }

    /// <summary>
    /// Gets a mathematical variable by identifier.
    /// </summary>
    /// <param name="variableId">
    /// Mathematical-variable identifier.
    /// </param>
    /// <returns>
    /// Mathematical variable.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the identifier is unknown.
    /// </exception>
    public MathematicalVariable GetVariable(
        int variableId)
    {
        if (_variablesById.TryGetValue(
                variableId,
                out MathematicalVariable? variable))
        {
            return variable;
        }

        throw new KeyNotFoundException(
            $"No mathematical variable exists for identifier " +
            $"'{variableId}'.");
    }

    /// <summary>
    /// Gets a solver value by mathematical-variable identifier.
    /// </summary>
    /// <param name="variableId">
    /// Mathematical-variable identifier.
    /// </param>
    /// <returns>
    /// Solver value.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no solver value exists for the identifier.
    /// </exception>
    public MathematicalVariableValue GetValue(
        int variableId)
    {
        if (_valuesByVariableId.TryGetValue(
                variableId,
                out MathematicalVariableValue? variableValue))
        {
            return variableValue;
        }

        throw new KeyNotFoundException(
            $"No solver value exists for mathematical variable " +
            $"identifier '{variableId}'.");
    }

    /// <summary>
    /// Gets a solver value by business-domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <returns>
    /// Solver value associated with the domain key.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="domainKey"/> is empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no value is associated with the domain key.
    /// </exception>
    public MathematicalVariableValue GetValue(
        string domainKey)
    {
        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        if (_valuesByDomainKey.TryGetValue(
                normalizedDomainKey,
                out MathematicalVariableValue? variableValue))
        {
            return variableValue;
        }

        throw new KeyNotFoundException(
            $"No solver value exists for domain key " +
            $"'{normalizedDomainKey}'.");
    }

    /// <summary>
    /// Attempts to get a solver value by business-domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <param name="variableValue">
    /// Solver value when found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a value is found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetValue(
        string domainKey,
        out MathematicalVariableValue? variableValue)
    {
        string normalizedDomainKey =
            NormalizeDomainKey(
                domainKey);

        return _valuesByDomainKey.TryGetValue(
            normalizedDomainKey,
            out variableValue);
    }

    /// <summary>
    /// Returns solver values whose domain-key category matches
    /// the supplied category.
    /// </summary>
    /// <param name="category">
    /// Domain-key category.
    /// </param>
    /// <param name="includeZeroValues">
    /// Indicates whether values considered equal to zero must be
    /// included.
    /// </param>
    /// <param name="zeroTolerance">
    /// Absolute tolerance below which a value is considered zero.
    /// </param>
    /// <returns>
    /// Matching mathematical-variable values.
    /// </returns>
    public IReadOnlyList<MathematicalVariableValue>
        GetValuesByCategory(
            string category,
            bool includeZeroValues,
            double zeroTolerance = 1.0e-9)
    {
        if (string.IsNullOrWhiteSpace(
                category))
        {
            throw new ArgumentException(
                "A domain-key category is required.",
                nameof(category));
        }

        if (double.IsNaN(
                zeroTolerance) ||
            double.IsInfinity(
                zeroTolerance) ||
            zeroTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(zeroTolerance),
                zeroTolerance,
                "Zero tolerance must be finite and non-negative.");
        }

        string normalizedCategory =
            category.Trim();

        return _valuesByDomainKey
            .Where(
                pair =>
                {
                    if (!MathematicalDomainKey.TryParse(
                            pair.Key,
                            out MathematicalDomainKey? key) ||
                        !string.Equals(
                            key!.Category,
                            normalizedCategory,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return includeZeroValues ||
                           Math.Abs(
                               pair.Value.Value) >
                           zeroTolerance;
                })
            .Select(
                pair =>
                {
                    MathematicalVariable? variable =
                        Model.FindVariableById(
                            pair.Value.VariableId);

                    if (variable is null)
                    {
                        throw new InvalidOperationException(
                            $"No mathematical variable with identifier " +
                            $"'{pair.Value.VariableId}' exists in the model.");
                    }

                    return ValueNormalizer.Normalize(
                        variable,
                        pair.Value);
                })
            .ToArray();
    }

    /// <summary>
    /// Returns all non-zero solver values whose domain-key
    /// category matches the supplied category.
    /// </summary>
    /// <param name="category">
    /// Domain-key category.
    /// </param>
    /// <param name="zeroTolerance">
    /// Absolute tolerance below which a value is considered zero.
    /// </param>
    /// <returns>
    /// Matching mathematical-variable values.
    /// </returns>
    public IReadOnlyList<MathematicalVariableValue>
        GetNonZeroValuesByCategory(
            string category,
            double zeroTolerance = 1.0e-9)
    {
        return GetValuesByCategory(
            category,
            includeZeroValues: false,
            zeroTolerance);
    }

    private void BuildIndexes()
    {
        foreach (
            MathematicalVariableValue variableValue
            in SolveResult.VariableValues)
        {
            if (!_variablesById.TryGetValue(
                    variableValue.VariableId,
                    out MathematicalVariable? variable))
            {
                throw new InvalidOperationException(
                    $"Solver result references unknown " +
                    $"mathematical variable identifier " +
                    $"'{variableValue.VariableId}'.");
            }

            string domainKey =
                !string.IsNullOrWhiteSpace(
                    variableValue.DomainKey)
                    ? variableValue.DomainKey.Trim()
                    : variable.DomainKey?.Trim() ??
                      string.Empty;

            if (string.IsNullOrWhiteSpace(
                    domainKey))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(
                    variable.DomainKey) &&
                !string.Equals(
                    variable.DomainKey.Trim(),
                    domainKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Solver-result domain key '{domainKey}' does " +
                    $"not match model domain key " +
                    $"'{variable.DomainKey}' for variable " +
                    $"identifier '{variable.Id}'.");
            }

            variableValue.DomainKey =
                domainKey;

            if (!_valuesByDomainKey.TryAdd(
                    domainKey,
                    variableValue))
            {
                throw new InvalidOperationException(
                    $"Domain key '{domainKey}' appears more than " +
                    "once in the mathematical solver result.");
            }
        }
    }

    private static string NormalizeDomainKey(
        string domainKey)
    {
        if (string.IsNullOrWhiteSpace(
                domainKey))
        {
            throw new ArgumentException(
                "A mathematical domain key is required.",
                nameof(domainKey));
        }

        return domainKey.Trim();
    }
}
