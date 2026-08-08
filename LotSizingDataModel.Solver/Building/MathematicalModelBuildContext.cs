using System;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Building;

/// <summary>
/// Provides the shared state required while constructing a
/// solver-independent mathematical model.
/// </summary>
/// <remarks>
/// The context combines the mathematical model builder and the
/// domain-key variable registry. It ensures that every business
/// decision variable added through the context is immediately
/// registered and can later be retrieved while generating
/// constraints.
/// </remarks>
public sealed class MathematicalModelBuildContext
{
    /// <summary>
    /// Initializes a mathematical-model build context.
    /// </summary>
    /// <param name="modelName">
    /// Mathematical model name.
    /// </param>
    public MathematicalModelBuildContext(
        string modelName)
    {
        ModelBuilder =
            new MathematicalModelBuilder(
                modelName);

        VariableRegistry =
            new MathematicalVariableRegistry();
    }

    /// <summary>
    /// Gets the mathematical model builder.
    /// </summary>
    public MathematicalModelBuilder ModelBuilder
    {
        get;
    }

    /// <summary>
    /// Gets the registry that maps business-domain keys to
    /// mathematical variables.
    /// </summary>
    public MathematicalVariableRegistry VariableRegistry
    {
        get;
    }

    /// <summary>
    /// Gets the mathematical model currently being built.
    /// </summary>
    public MathematicalModel Model =>
        ModelBuilder.Model;

    /// <summary>
    /// Sets the model description.
    /// </summary>
    /// <param name="description">
    /// Model description.
    /// </param>
    /// <returns>
    /// Current build context.
    /// </returns>
    public MathematicalModelBuildContext SetDescription(
        string description)
    {
        ModelBuilder.SetDescription(
            description);

        return this;
    }

    /// <summary>
    /// Sets the mathematical objective.
    /// </summary>
    /// <param name="objective">
    /// Objective to assign.
    /// </param>
    /// <returns>
    /// Current build context.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="objective"/> is
    /// <see langword="null"/>.
    /// </exception>
    public MathematicalModelBuildContext SetObjective(
        MathematicalObjective objective)
    {
        ArgumentNullException.ThrowIfNull(
            objective);

        ModelBuilder.SetObjective(
            objective);

        return this;
    }

    /// <summary>
    /// Creates, adds, and registers a mathematical variable.
    /// </summary>
    /// <param name="name">
    /// Variable name.
    /// </param>
    /// <param name="domainKey">
    /// Unique business-domain key.
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
    /// <param name="description">
    /// Optional variable description.
    /// </param>
    /// <returns>
    /// Created and registered mathematical variable.
    /// </returns>
    public MathematicalVariable AddVariable(
        string name,
        string domainKey,
        MathematicalVariableType variableType,
        double lowerBound = 0.0,
        double upperBound = double.PositiveInfinity,
        string description = "")
    {
        MathematicalVariable variable =
            ModelBuilder.AddVariable(
                name,
                variableType,
                lowerBound,
                upperBound,
                domainKey,
                description);

        try
        {
            VariableRegistry.Register(
                variable);
        }
        catch
        {
            ModelBuilder.Model.Variables.Remove(
                variable);

            throw;
        }

        return variable;
    }

    /// <summary>
    /// Adds a linear constraint to the mathematical model.
    /// </summary>
    /// <param name="name">
    /// Constraint name.
    /// </param>
    /// <param name="leftHandSide">
    /// Left-hand-side linear expression.
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
    /// Optional constraint description.
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
        return ModelBuilder.AddConstraint(
            name,
            leftHandSide,
            sense,
            rightHandSide,
            domainKey,
            description);
    }

    /// <summary>
    /// Gets a required mathematical variable from its
    /// business-domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Business-domain key.
    /// </param>
    /// <returns>
    /// Registered mathematical variable.
    /// </returns>
    public MathematicalVariable GetVariable(
        string domainKey)
    {
        return VariableRegistry.GetRequired(
            domainKey);
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
        return ModelBuilder.Build(
            clone);
    }
}
