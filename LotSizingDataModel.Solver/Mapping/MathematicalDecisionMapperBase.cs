using System;
using System.Collections.Generic;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Provides a reusable base implementation for mathematical
/// decision mappers handling one domain-key category.
/// </summary>
public abstract class MathematicalDecisionMapperBase :
    IMathematicalDecisionMapper
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public abstract string Category
    {
        get;
    }

    /// <summary>
    /// Gets the absolute tolerance below which a solver value is
    /// considered equal to zero.
    /// </summary>
    protected virtual double ZeroTolerance =>
        1.0e-9;

    /// <summary>
    /// Maps the decision values handled by this mapper into a
    /// lot-sizing solution.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or
    /// <paramref name="solution"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mapper category or zero tolerance is
    /// invalid.
    /// </exception>
    public void Map(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            solution);

        EnsureConfigurationIsValid();

        double effectiveZeroTolerance =
            context.Options.ZeroTolerance;

        IReadOnlyList<MathematicalVariableValue> values =
            context.GetValuesByCategory(
                Category,
                context.Options.IncludeZeroValues,
                effectiveZeroTolerance);

        BeforeMap(
            context,
            solution,
            values);

        foreach (
            MathematicalVariableValue variableValue
            in values)
        {
            MathematicalDomainKey domainKey =
                MathematicalDomainKey.Parse(
                    variableValue.DomainKey);

            MapValue(
                context,
                solution,
                domainKey,
                variableValue);
        }

        AfterMap(
            context,
            solution,
            values);
    }

    /// <summary>
    /// Maps one mathematical-variable value selected by the
    /// active mapping options.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed business-domain key.
    /// </param>
    /// <param name="variableValue">
    /// Mathematical-variable value returned by the solver.
    /// </param>
    protected abstract void MapValue(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        MathematicalDomainKey domainKey,
        MathematicalVariableValue variableValue);

    /// <summary>
    /// Executes optional initialization before values are mapped.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="values">
    /// Values handled by this mapper.
    /// </param>
    protected virtual void BeforeMap(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        IReadOnlyList<MathematicalVariableValue> values)
    {
    }

    /// <summary>
    /// Executes optional finalization after all values have been
    /// mapped.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="values">
    /// Values handled by this mapper.
    /// </param>
    protected virtual void AfterMap(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        IReadOnlyList<MathematicalVariableValue> values)
    {
    }

    private void EnsureConfigurationIsValid()
    {
        if (string.IsNullOrWhiteSpace(
                Category))
        {
            throw new InvalidOperationException(
                "A mathematical decision-mapper category is " +
                "required.");
        }

        if (double.IsNaN(
                ZeroTolerance) ||
            double.IsInfinity(
                ZeroTolerance) ||
            ZeroTolerance < 0.0)
        {
            throw new InvalidOperationException(
                "The mathematical decision-mapper zero tolerance " +
                "must be finite and non-negative.");
        }
    }
}
