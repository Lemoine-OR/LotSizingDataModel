using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps a generic mathematical solver result to a normalized
/// lot-sizing solution and reports mapping diagnostics.
/// </summary>
public sealed class MathematicalSolutionMappingService :
    IMathematicalSolutionMappingService
{
    private readonly MathematicalDecisionMapperRegistry
        _decisionMapperRegistry;

    /// <summary>
    /// Initializes a mathematical-solution mapping service.
    /// </summary>
    /// <param name="decisionMapperRegistry">
    /// Registry containing the decision-family mappers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="decisionMapperRegistry"/> is
    /// <see langword="null"/>.
    /// </exception>
    public MathematicalSolutionMappingService(
        MathematicalDecisionMapperRegistry decisionMapperRegistry)
    {
        ArgumentNullException.ThrowIfNull(
            decisionMapperRegistry);

        _decisionMapperRegistry =
            decisionMapperRegistry;
    }

    /// <summary>
    /// Maps a mathematical-model solve result to a normalized
    /// lot-sizing solution.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="model">
    /// Solver-independent mathematical model that was solved.
    /// </param>
    /// <param name="solveResult">
    /// Generic mathematical-model solve result.
    /// </param>
    /// <param name="options">
    /// Mathematical-solution mapping options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the mapping operation.
    /// </param>
    /// <returns>
    /// Task returning the complete mapping result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    public ValueTask<MathematicalSolutionMappingResult> MapAsync(
        LotSizingInstance instance,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        MathematicalSolutionMappingOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            model);

        ArgumentNullException.ThrowIfNull(
            solveResult);

        ArgumentNullException.ThrowIfNull(
            options);

        MathematicalSolutionMappingOptions normalizedOptions =
            options.Clone();

        normalizedOptions.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            model.EnsureValid();
            solveResult.EnsureValid();

            if (!solveResult.HasFeasibleSolution)
            {
                stopwatch.Stop();

                return new ValueTask<MathematicalSolutionMappingResult>(
                    MathematicalSolutionMappingResult.Failure(
                        "A solver result without a feasible " +
                        "solution cannot be mapped.",
                        stopwatch.Elapsed));
            }

            ValidateCompleteness(
                model,
                solveResult,
                normalizedOptions);

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyCollection<string> categories =
                GetCategories(
                    solveResult);

            ValidateCategories(
                categories,
                normalizedOptions);

            var mapper =
                new MathematicalSolutionMapper(
                    _decisionMapperRegistry);

            LotSizingDataModel.Solution.LotSizingSolution solution =
                mapper.Map(
                    instance,
                    model,
                    solveResult,
                    normalizedOptions);

            cancellationToken.ThrowIfCancellationRequested();

            stopwatch.Stop();

            MathematicalSolutionMappingResult result =
                MathematicalSolutionMappingResult.Success(
                    solution,
                    stopwatch.Elapsed);

            PopulateStatistics(
                result,
                solveResult,
                categories,
                normalizedOptions);

            result.AddDiagnostic(
                $"Mathematical solution mapping processed " +
                $"{result.ProcessedValueCount} values across " +
                $"{result.ProcessedCategoryCount} decision " +
                "categories.");

            result.EnsureValid();

            return new ValueTask<MathematicalSolutionMappingResult>(
                result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            MathematicalSolutionMappingResult result =
                MathematicalSolutionMappingResult.Failure(
                    exception.Message,
                    stopwatch.Elapsed);

            result.AddDiagnostic(
                exception.ToString());

            result.EnsureValid();

            return new ValueTask<MathematicalSolutionMappingResult>(
                result);
        }
    }

    private static IReadOnlyCollection<string> GetCategories(
        MathematicalModelSolveResult solveResult)
    {
        var categories =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (
            MathematicalVariableValue variableValue
            in solveResult.VariableValues)
        {
            if (string.IsNullOrWhiteSpace(
                    variableValue.DomainKey))
            {
                continue;
            }

            if (MathematicalDomainKey.TryParse(
                    variableValue.DomainKey,
                    out MathematicalDomainKey? domainKey))
            {
                categories.Add(
                    domainKey!.Category);
            }
        }

        return categories;
    }

    private void ValidateCategories(
        IEnumerable<string> categories,
        MathematicalSolutionMappingOptions options)
    {
        if (!options.RequireKnownCategories)
        {
            return;
        }

        foreach (
            string category
            in categories)
        {
            if (!_decisionMapperRegistry.Contains(
                    category))
            {
                throw new InvalidOperationException(
                    $"No mathematical decision mapper is " +
                    $"registered for category '{category}'.");
            }
        }
    }

    private static void ValidateCompleteness(
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        MathematicalSolutionMappingOptions options)
    {
        if (!options.RequireCompleteVariableValues)
        {
            return;
        }

        var returnedVariableIds =
            solveResult.VariableValues
                .Select(
                    variableValue =>
                        variableValue.VariableId)
                .ToHashSet();

        foreach (
            MathematicalVariable variable
            in model.Variables)
        {
            if (!returnedVariableIds.Contains(
                    variable.Id))
            {
                throw new InvalidOperationException(
                    $"No solver value was returned for " +
                    $"mathematical variable '{variable.Name}' " +
                    $"with identifier '{variable.Id}'.");
            }
        }
    }

    private static void PopulateStatistics(
        MathematicalSolutionMappingResult result,
        MathematicalModelSolveResult solveResult,
        IReadOnlyCollection<string> categories,
        MathematicalSolutionMappingOptions options)
    {
        int processedValueCount =
            0;

        int ignoredValueCount =
            0;

        foreach (
            MathematicalVariableValue variableValue
            in solveResult.VariableValues)
        {
            bool isZero =
                Math.Abs(
                    variableValue.Value) <=
                options.ZeroTolerance;

            if (isZero &&
                !options.IncludeZeroValues)
            {
                ignoredValueCount++;

                continue;
            }

            processedValueCount++;
        }

        result.ProcessedValueCount =
            processedValueCount;

        result.IgnoredValueCount =
            ignoredValueCount;

        result.ProcessedCategoryCount =
            categories.Count;
    }
}
