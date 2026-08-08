using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Represents a solver-independent mathematical optimization
/// model.
/// </summary>
/// <remarks>
/// The model can be translated by solver adapters into native
/// CPLEX, Gurobi, FICO Xpress, or COIN-OR CBC models.
/// </remarks>
[Serializable]
[XmlRoot("mathematicalModel")]
[XmlType(TypeName = "mathematicalModel")]
public sealed class MathematicalModel
{
    private readonly List<MathematicalVariable> _variables =
        new();

    private readonly List<LinearConstraint> _constraints =
        new();

    /// <summary>
    /// Initializes an empty mathematical model.
    /// </summary>
    public MathematicalModel()
    {
        Name =
            string.Empty;

        Description =
            string.Empty;

        Objective =
            new MathematicalObjective();
    }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional model description.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mathematical objective.
    /// </summary>
    [XmlElement("objective")]
    public MathematicalObjective Objective
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the mathematical variables.
    /// </summary>
    [XmlArray("variables")]
    [XmlArrayItem("variable")]
    public List<MathematicalVariable> Variables =>
        _variables;

    /// <summary>
    /// Gets the linear constraints.
    /// </summary>
    [XmlArray("constraints")]
    [XmlArrayItem("constraint")]
    public List<LinearConstraint> Constraints =>
        _constraints;

    /// <summary>
    /// Gets the number of mathematical variables.
    /// </summary>
    [XmlIgnore]
    public int VariableCount =>
        _variables.Count;

    /// <summary>
    /// Gets the number of enabled linear constraints.
    /// </summary>
    [XmlIgnore]
    public int EnabledConstraintCount =>
        _constraints.Count(
            constraint =>
                constraint.IsEnabled);

    /// <summary>
    /// Gets the number of binary variables.
    /// </summary>
    [XmlIgnore]
    public int BinaryVariableCount =>
        _variables.Count(
            variable =>
                variable.VariableType ==
                MathematicalVariableType.Binary);

    /// <summary>
    /// Gets the number of general integer variables.
    /// </summary>
    [XmlIgnore]
    public int IntegerVariableCount =>
        _variables.Count(
            variable =>
                variable.VariableType is
                    MathematicalVariableType.Integer or
                    MathematicalVariableType.SemiInteger);

    /// <summary>
    /// Gets the number of continuous variables.
    /// </summary>
    [XmlIgnore]
    public int ContinuousVariableCount =>
        _variables.Count(
            variable =>
                variable.VariableType is
                    MathematicalVariableType.Continuous or
                    MathematicalVariableType.SemiContinuous);

    /// <summary>
    /// Adds a mathematical variable.
    /// </summary>
    /// <param name="variable">
    /// Variable to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variable"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the variable identifier or name is already
    /// used.
    /// </exception>
    public void AddVariable(
        MathematicalVariable variable)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        variable.EnsureValid();

        if (_variables.Any(
                existingVariable =>
                    existingVariable.Id ==
                    variable.Id))
        {
            throw new InvalidOperationException(
                $"Variable identifier '{variable.Id}' is " +
                "already used.");
        }

        if (_variables.Any(
                existingVariable =>
                    string.Equals(
                        existingVariable.Name,
                        variable.Name,
                        StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Variable name '{variable.Name}' is already " +
                "used.");
        }

        _variables.Add(
            variable);
    }

    /// <summary>
    /// Adds a linear constraint.
    /// </summary>
    /// <param name="constraint">
    /// Constraint to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="constraint"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the constraint identifier or name is already
    /// used, or when the constraint references an unknown
    /// variable.
    /// </exception>
    public void AddConstraint(
        LinearConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(
            constraint);

        constraint.EnsureValid();

        if (_constraints.Any(
                existingConstraint =>
                    existingConstraint.Id ==
                    constraint.Id))
        {
            throw new InvalidOperationException(
                $"Constraint identifier '{constraint.Id}' is " +
                "already used.");
        }

        if (_constraints.Any(
                existingConstraint =>
                    string.Equals(
                        existingConstraint.Name,
                        constraint.Name,
                        StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Constraint name '{constraint.Name}' is " +
                "already used.");
        }

        EnsureExpressionReferencesKnownVariables(
            constraint.LeftHandSide,
            $"constraint '{constraint.Name}'");

        _constraints.Add(
            constraint);
    }

    /// <summary>
    /// Finds a mathematical variable by identifier.
    /// </summary>
    /// <param name="variableId">
    /// Variable identifier.
    /// </param>
    /// <returns>
    /// Matching variable, or <see langword="null"/> when no
    /// variable has the supplied identifier.
    /// </returns>
    public MathematicalVariable? FindVariableById(
        int variableId)
    {
        return _variables.FirstOrDefault(
            variable =>
                variable.Id ==
                variableId);
    }

    /// <summary>
    /// Finds a mathematical variable by name.
    /// </summary>
    /// <param name="name">
    /// Variable name.
    /// </param>
    /// <returns>
    /// Matching variable, or <see langword="null"/> when no
    /// variable has the supplied name.
    /// </returns>
    public MathematicalVariable? FindVariableByName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(
                name))
        {
            throw new ArgumentException(
                "A variable name is required.",
                nameof(name));
        }

        return _variables.FirstOrDefault(
            variable =>
                string.Equals(
                    variable.Name,
                    name.Trim(),
                    StringComparison.Ordinal));
    }

    /// <summary>
    /// Validates the complete mathematical model.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the model contains inconsistent or invalid
    /// definitions.
    /// </exception>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                Name))
        {
            throw new InvalidOperationException(
                "A mathematical model name is required.");
        }

        ArgumentNullException.ThrowIfNull(
            Objective);

        Objective.EnsureValid();

        EnsureUniqueVariables();
        EnsureUniqueConstraints();

        foreach (
            MathematicalVariable variable
            in _variables)
        {
            if (variable is null)
            {
                throw new InvalidOperationException(
                    "The variable collection cannot contain a " +
                    "null entry.");
            }

            variable.EnsureValid();
        }

        foreach (
            LinearConstraint constraint
            in _constraints)
        {
            if (constraint is null)
            {
                throw new InvalidOperationException(
                    "The constraint collection cannot contain " +
                    "a null entry.");
            }

            constraint.EnsureValid();

            EnsureExpressionReferencesKnownVariables(
                constraint.LeftHandSide,
                $"constraint '{constraint.Name}'");
        }

        EnsureExpressionReferencesKnownVariables(
            Objective.Expression,
            "objective");
    }

    /// <summary>
    /// Creates an independent copy of the mathematical model.
    /// </summary>
    /// <returns>
    /// Cloned mathematical model.
    /// </returns>
    public MathematicalModel Clone()
    {
        var clone =
            new MathematicalModel
            {
                Name =
                    Name,

                Description =
                    Description,

                Objective =
                    Objective.Clone()
            };

        foreach (
            MathematicalVariable variable
            in _variables)
        {
            clone.Variables.Add(
                new MathematicalVariable
                {
                    Id =
                        variable.Id,

                    Name =
                        variable.Name,

                    VariableType =
                        variable.VariableType,

                    LowerBound =
                        variable.LowerBound,

                    UpperBound =
                        variable.UpperBound,

                    DomainKey =
                        variable.DomainKey,

                    Description =
                        variable.Description
                });
        }

        foreach (
            LinearConstraint constraint
            in _constraints)
        {
            clone.Constraints.Add(
                constraint.Clone());
        }

        return clone;
    }

    private void EnsureExpressionReferencesKnownVariables(
        LinearExpression expression,
        string context)
    {
        foreach (
            LinearTerm term
            in expression.Terms)
        {
            if (FindVariableById(
                    term.VariableId) is null)
            {
                throw new InvalidOperationException(
                    $"The {context} references unknown variable " +
                    $"identifier '{term.VariableId}'.");
            }
        }
    }

    private void EnsureUniqueVariables()
    {
        if (_variables
            .GroupBy(
                variable =>
                    variable.Id)
            .Any(
                group =>
                    group.Count() > 1))
        {
            throw new InvalidOperationException(
                "Mathematical variable identifiers must be " +
                "unique.");
        }

        if (_variables
            .GroupBy(
                variable =>
                    variable.Name,
                StringComparer.Ordinal)
            .Any(
                group =>
                    group.Count() > 1))
        {
            throw new InvalidOperationException(
                "Mathematical variable names must be unique.");
        }
    }

    private void EnsureUniqueConstraints()
    {
        if (_constraints
            .GroupBy(
                constraint =>
                    constraint.Id)
            .Any(
                group =>
                    group.Count() > 1))
        {
            throw new InvalidOperationException(
                "Linear constraint identifiers must be unique.");
        }

        if (_constraints
            .GroupBy(
                constraint =>
                    constraint.Name,
                StringComparer.Ordinal)
            .Any(
                group =>
                    group.Count() > 1))
        {
            throw new InvalidOperationException(
                "Linear constraint names must be unique.");
        }
    }
}
