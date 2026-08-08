using System;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Building;

/// <summary>
/// Builds a solver-independent mathematical model while
/// assigning unique variable and constraint identifiers.
/// </summary>
public sealed class MathematicalModelBuilder
{
    private readonly MathematicalModel _model;

    private int _nextVariableId =
        1;

    private int _nextConstraintId =
        1;

    /// <summary>
    /// Initializes a mathematical-model builder.
    /// </summary>
    /// <param name="modelName">
    /// Mathematical model name.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="modelName"/> is empty.
    /// </exception>
    public MathematicalModelBuilder(
        string modelName)
    {
        if (string.IsNullOrWhiteSpace(
                modelName))
        {
            throw new ArgumentException(
                "A mathematical model name is required.",
                nameof(modelName));
        }

        _model =
            new MathematicalModel
            {
                Name =
                    modelName.Trim()
            };
    }

    /// <summary>
    /// Gets the model currently being built.
    /// </summary>
    public MathematicalModel Model =>
        _model;

    /// <summary>
    /// Sets the model description.
    /// </summary>
    /// <param name="description">
    /// Model description.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalModelBuilder SetDescription(
        string description)
    {
        _model.Description =
            description?.Trim() ??
            string.Empty;

        return this;
    }

    /// <summary>
    /// Sets the mathematical objective.
    /// </summary>
    /// <param name="objective">
    /// Objective to assign.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="objective"/> is
    /// <see langword="null"/>.
    /// </exception>
    public MathematicalModelBuilder SetObjective(
        MathematicalObjective objective)
    {
        ArgumentNullException.ThrowIfNull(
            objective);

        objective.EnsureValid();

        _model.Objective =
            objective;

        return this;
    }

    /// <summary>
    /// Creates and adds a mathematical variable.
    /// </summary>
    /// <param name="name">
    /// Variable name.
    /// </param>
    /// <param name="variableType">
    /// Variable domain.
    /// </param>
    /// <param name="lowerBound">
    /// Variable lower bound.
    /// </param>
    /// <param name="upperBound">
    /// Variable upper bound.
    /// </param>
    /// <param name="domainKey">
    /// Optional business-domain key.
    /// </param>
    /// <param name="description">
    /// Optional description.
    /// </param>
    /// <returns>
    /// Created mathematical variable.
    /// </returns>
    public MathematicalVariable AddVariable(
        string name,
        MathematicalVariableType variableType,
        double lowerBound = 0.0,
        double upperBound = double.PositiveInfinity,
        string domainKey = "",
        string description = "")
    {
        var variable =
            new MathematicalVariable(
                _nextVariableId,
                name,
                variableType,
                lowerBound,
                upperBound)
            {
                DomainKey =
                    domainKey?.Trim() ??
                    string.Empty,

                Description =
                    description?.Trim() ??
                    string.Empty
            };

        _model.AddVariable(
            variable);

        _nextVariableId++;

        return variable;
    }

    /// <summary>
    /// Creates and adds a linear constraint.
    /// </summary>
    /// <param name="name">
    /// Constraint name.
    /// </param>
    /// <param name="leftHandSide">
    /// Left-hand-side expression.
    /// </param>
    /// <param name="sense">
    /// Constraint relational sense.
    /// </param>
    /// <param name="rightHandSide">
    /// Right-hand-side constant.
    /// </param>
    /// <param name="domainKey">
    /// Optional business-domain key.
    /// </param>
    /// <param name="description">
    /// Optional description.
    /// </param>
    /// <returns>
    /// Created linear constraint.
    /// </returns>
    public LinearConstraint AddConstraint(
        string name,
        LinearExpression leftHandSide,
        MathematicalConstraintSense sense,
        double rightHandSide,
        string domainKey = "",
        string description = "")
    {
        ArgumentNullException.ThrowIfNull(
            leftHandSide);

        var constraint =
            new LinearConstraint(
                _nextConstraintId,
                name,
                leftHandSide,
                sense,
                rightHandSide)
            {
                DomainKey =
                    domainKey?.Trim() ??
                    string.Empty,

                Description =
                    description?.Trim() ??
                    string.Empty
            };

        _model.AddConstraint(
            constraint);

        _nextConstraintId++;

        return constraint;
    }

    /// <summary>
    /// Builds and validates the mathematical model.
    /// </summary>
    /// <param name="clone">
    /// Indicates whether an independent clone should be
    /// returned.
    /// </param>
    /// <returns>
    /// Valid mathematical model.
    /// </returns>
    public MathematicalModel Build(
        bool clone = true)
    {
        _model.EnsureValid();

        return clone
            ? _model.Clone()
            : _model;
    }
}
