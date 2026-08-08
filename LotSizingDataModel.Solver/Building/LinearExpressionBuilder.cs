using System;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Building;

/// <summary>
/// Provides a fluent builder for linear expressions.
/// </summary>
public sealed class LinearExpressionBuilder
{
    private readonly LinearExpression _expression =
        new();

    /// <summary>
    /// Gets the expression currently being built.
    /// </summary>
    public LinearExpression Expression =>
        _expression;

    /// <summary>
    /// Adds a variable term to the expression.
    /// </summary>
    /// <param name="variable">
    /// Referenced mathematical variable.
    /// </param>
    /// <param name="coefficient">
    /// Linear coefficient.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variable"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LinearExpressionBuilder Add(
        MathematicalVariable variable,
        double coefficient = 1.0)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        variable.EnsureValid();

        _expression.AddTerm(
            variable.Id,
            coefficient);

        return this;
    }

    /// <summary>
    /// Subtracts a variable term from the expression.
    /// </summary>
    /// <param name="variable">
    /// Referenced mathematical variable.
    /// </param>
    /// <param name="coefficient">
    /// Positive coefficient to subtract.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variable"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="coefficient"/> is negative.
    /// </exception>
    public LinearExpressionBuilder Subtract(
        MathematicalVariable variable,
        double coefficient = 1.0)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        if (coefficient < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(coefficient),
                coefficient,
                "The coefficient to subtract cannot be negative.");
        }

        return Add(
            variable,
            -coefficient);
    }

    /// <summary>
    /// Adds a constant value to the expression.
    /// </summary>
    /// <param name="value">
    /// Constant value to add.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public LinearExpressionBuilder AddConstant(
        double value)
    {
        _expression.AddConstant(
            value);

        return this;
    }

    /// <summary>
    /// Subtracts a constant value from the expression.
    /// </summary>
    /// <param name="value">
    /// Constant value to subtract.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public LinearExpressionBuilder SubtractConstant(
        double value)
    {
        _expression.AddConstant(
            -value);

        return this;
    }

    /// <summary>
    /// Adds another linear expression.
    /// </summary>
    /// <param name="expression">
    /// Expression to add.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expression"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LinearExpressionBuilder Add(
        LinearExpression expression)
    {
        ArgumentNullException.ThrowIfNull(
            expression);

        _expression.Add(
            expression);

        return this;
    }

    /// <summary>
    /// Subtracts another linear expression.
    /// </summary>
    /// <param name="expression">
    /// Expression to subtract.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expression"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LinearExpressionBuilder Subtract(
        LinearExpression expression)
    {
        ArgumentNullException.ThrowIfNull(
            expression);

        LinearExpression negatedExpression =
            expression.Clone();

        negatedExpression.MultiplyBy(
            -1.0);

        _expression.Add(
            negatedExpression);

        return this;
    }

    /// <summary>
    /// Multiplies the entire expression by a scalar.
    /// </summary>
    /// <param name="factor">
    /// Scalar multiplier.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public LinearExpressionBuilder MultiplyBy(
        double factor)
    {
        _expression.MultiplyBy(
            factor);

        return this;
    }

    /// <summary>
    /// Clears the current expression.
    /// </summary>
    /// <returns>
    /// Current builder.
    /// </returns>
    public LinearExpressionBuilder Clear()
    {
        _expression.Clear();

        return this;
    }

    /// <summary>
    /// Builds and validates the linear expression.
    /// </summary>
    /// <param name="clone">
    /// Indicates whether an independent clone should be
    /// returned.
    /// </param>
    /// <returns>
    /// Valid linear expression.
    /// </returns>
    public LinearExpression Build(
        bool clone = true)
    {
        _expression.EnsureValid();

        return clone
            ? _expression.Clone()
            : _expression;
    }
}
