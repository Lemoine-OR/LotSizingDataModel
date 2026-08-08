using System;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps a complete mathematical solver result to a normalized
/// lot-sizing solution by delegating each decision family to a
/// registered mathematical decision mapper.
/// </summary>
public sealed class MathematicalSolutionMapper :
    IMathematicalSolutionMapper
{
    private readonly MathematicalDecisionMapperRegistry
        _decisionMapperRegistry;

    /// <summary>
    /// Initializes a mathematical-solution mapper.
    /// </summary>
    /// <param name="decisionMapperRegistry">
    /// Registry containing the decision-family mappers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="decisionMapperRegistry"/> is
    /// <see langword="null"/>.
    /// </exception>
    public MathematicalSolutionMapper(
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
    /// <returns>
    /// Normalized lot-sizing solution.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the solve result is not feasible, the source
    /// instance has no valid planning horizon, or the result
    /// cannot be mapped consistently.
    /// </exception>
    public LotSizingSolution Map(
        LotSizingInstance instance,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult)
    {
        return Map(
            instance,
            model,
            solveResult,
            new MathematicalSolutionMappingOptions());
    }

    /// <summary>
    /// Maps a mathematical-model solve result to a normalized
    /// lot-sizing solution using explicit mapping options.
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
    /// <returns>
    /// Normalized lot-sizing solution.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required arguments is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the solve result is not feasible, the source
    /// instance has no valid planning horizon, or the result
    /// cannot be mapped consistently.
    /// </exception>
    public LotSizingSolution Map(
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

        MathematicalSolutionMappingOptions normalizedOptions =
            options.Clone();

        normalizedOptions.EnsureValid();

        if (!solveResult.HasFeasibleSolution)
        {
            throw new InvalidOperationException(
                "A mathematical solver result without a feasible " +
                "solution cannot be mapped to LotSizingSolution.");
        }

        if (instance.PlanningHorizon <= 0)
        {
            throw new InvalidOperationException(
                "The source lot-sizing instance must have a " +
                "strictly positive planning horizon.");
        }

        var context =
            new MathematicalSolutionMappingContext(
                instance,
                model,
                solveResult,
                normalizedOptions);

        string solutionName =
            !string.IsNullOrWhiteSpace(
                solveResult.RunName)
                ? solveResult.RunName.Trim()
                : !string.IsNullOrWhiteSpace(
                    instance.Name)
                    ? instance.Name.Trim() +
                      " - solver solution"
                    : "Solver solution";

        var solution =
            new LotSizingSolution(
                solutionName,
                instance.InstanceId,
                instance.PlanningHorizon);

        foreach (
            IMathematicalDecisionMapper decisionMapper
            in _decisionMapperRegistry.GetAll())
        {
            decisionMapper.Map(
                context,
                solution);
        }

        MathematicalSolutionMetadataMapper.Apply(
            solution,
            solveResult);

        return solution;
    }
}
