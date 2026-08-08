using System;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;
using NativeCplex = global::ILOG.CPLEX.Cplex;
using LotSizingDataModel.Solver.Modeling;
using GenericObjectiveSense = global::LotSizingDataModel.Solver.Modeling.ObjectiveSense;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Translates a solver-independent linear mathematical model to
/// IBM ILOG Concert/CPLEX objects.
/// </summary>
public sealed class CplexModelTranslator
{
    /// <summary>
    /// Translates a complete mathematical model.
    /// </summary>
    /// <param name="model">
    /// Validated generic mathematical model.
    /// </param>
    /// <returns>
    /// Native CPLEX model and variable mapping.
    /// </returns>
    public CplexModelTranslationResult Translate(
        MathematicalModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.EnsureValid();

        var cplex =
            new NativeCplex();

        try
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                cplex.Name =
                    model.Name;
            }

            var variablesById =
                new Dictionary<int, INumVar>(
                    model.VariableCount);

            foreach (MathematicalVariable variable in model.Variables)
            {
                INumVar nativeVariable =
                    CreateVariable(
                        cplex,
                        variable);

                variablesById.Add(
                    variable.Id,
                    nativeVariable);
            }

            AddObjective(
                cplex,
                model.Objective,
                variablesById);

            foreach (LinearConstraint constraint in model.Constraints)
            {
                if (!constraint.IsEnabled)
                {
                    continue;
                }

                AddConstraint(
                    cplex,
                    constraint,
                    variablesById);
            }

            return new CplexModelTranslationResult(
                cplex,
                variablesById);
        }
        catch
        {
            cplex.End();
            throw;
        }
    }

    private static INumVar CreateVariable(
        NativeCplex cplex,
        MathematicalVariable variable)
    {
        double lowerBound =
            NormalizeLowerBound(
                variable.LowerBound);

        double upperBound =
            NormalizeUpperBound(
                variable.UpperBound);

        return variable.VariableType switch
        {
            MathematicalVariableType.Continuous =>
                cplex.NumVar(
                    lowerBound,
                    upperBound,
                    NumVarType.Float,
                    variable.Name),

            MathematicalVariableType.Integer =>
                cplex.NumVar(
                    lowerBound,
                    upperBound,
                    NumVarType.Int,
                    variable.Name),

            MathematicalVariableType.Binary =>
                cplex.BoolVar(
                    variable.Name),

            MathematicalVariableType.SemiContinuous =>
                throw new NotSupportedException(
                    "The first CPLEX adapter version does not " +
                    "translate semi-continuous variables."),

            MathematicalVariableType.SemiInteger =>
                throw new NotSupportedException(
                    "The first CPLEX adapter version does not " +
                    "translate semi-integer variables."),

            _ =>
                throw new NotSupportedException(
                    $"Unsupported mathematical variable type " +
                    $"'{variable.VariableType}'.")
        };
    }

    private static void AddObjective(
        NativeCplex cplex,
        MathematicalObjective objective,
        IReadOnlyDictionary<int, INumVar> variablesById)
    {
        INumExpr expression =
            BuildExpression(
                cplex,
                objective.Expression,
                variablesById);

        switch (objective.Sense)
        {
            case GenericObjectiveSense.Minimize:
                cplex.AddMinimize(
                    expression,
                    objective.Name);
                break;

            case GenericObjectiveSense.Maximize:
                cplex.AddMaximize(
                    expression,
                    objective.Name);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported objective sense " +
                    $"'{objective.Sense}'.");
        }
    }

    private static void AddConstraint(
        NativeCplex cplex,
        LinearConstraint constraint,
        IReadOnlyDictionary<int, INumVar> variablesById)
    {
        ILinearNumExpr leftHandSide =
            BuildLinearExpression(
                cplex,
                constraint.LeftHandSide,
                variablesById);

        double adjustedRightHandSide =
            constraint.RightHandSide -
            constraint.LeftHandSide.Constant;

        switch (constraint.Sense)
        {
            case MathematicalConstraintSense.LessThanOrEqual:
                cplex.AddLe(
                    leftHandSide,
                    adjustedRightHandSide,
                    constraint.Name);
                break;

            case MathematicalConstraintSense.Equal:
                cplex.AddEq(
                    leftHandSide,
                    adjustedRightHandSide,
                    constraint.Name);
                break;

            case MathematicalConstraintSense.GreaterThanOrEqual:
                cplex.AddGe(
                    leftHandSide,
                    adjustedRightHandSide,
                    constraint.Name);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported constraint sense " +
                    $"'{constraint.Sense}'.");
        }
    }

    private static INumExpr BuildExpression(
        NativeCplex cplex,
        LinearExpression expression,
        IReadOnlyDictionary<int, INumVar> variablesById)
    {
        ILinearNumExpr linear =
            BuildLinearExpression(
                cplex,
                expression,
                variablesById);

        if (expression.Constant == 0.0)
        {
            return linear;
        }

        return cplex.Sum(
            linear,
            expression.Constant);
    }

    private static ILinearNumExpr BuildLinearExpression(
        NativeCplex cplex,
        LinearExpression expression,
        IReadOnlyDictionary<int, INumVar> variablesById)
    {
        var result =
            cplex.LinearNumExpr();

        foreach (LinearTerm term in expression.Terms)
        {
            if (!variablesById.TryGetValue(
                    term.VariableId,
                    out INumVar? variable))
            {
                throw new InvalidOperationException(
                    $"Expression references unknown mathematical " +
                    $"variable identifier {term.VariableId}.");
            }

            result.AddTerm(
                term.Coefficient,
                variable);
        }

        return result;
    }

    private static double NormalizeLowerBound(
        double value)
    {
        return double.IsNegativeInfinity(value)
            ? -double.MaxValue
            : value;
    }

    private static double NormalizeUpperBound(
        double value)
    {
        return double.IsPositiveInfinity(value)
            ? double.MaxValue
            : value;
    }
}
