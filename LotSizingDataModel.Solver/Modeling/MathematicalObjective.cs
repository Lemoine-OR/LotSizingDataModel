using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Represents the objective function of a mathematical
/// optimization model.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalObjective")]
public sealed class MathematicalObjective
{
    /// <summary>
    /// Initializes an empty mathematical objective.
    /// </summary>
    public MathematicalObjective()
    {
        Name =
            "objective";

        Sense =
            ObjectiveSense.Minimize;

        Expression =
            new LinearExpression();
    }

    /// <summary>
    /// Initializes a mathematical objective.
    /// </summary>
    /// <param name="name">
    /// Objective name.
    /// </param>
    /// <param name="sense">
    /// Optimization direction.
    /// </param>
    /// <param name="expression">
    /// Linear objective expression.
    /// </param>
    public MathematicalObjective(
        string name,
        ObjectiveSense sense,
        LinearExpression expression)
        : this()
    {
        ArgumentNullException.ThrowIfNull(
            expression);

        Name =
            name;

        Sense =
            sense;

        Expression =
            expression;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the objective name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the optimization direction.
    /// </summary>
    [XmlAttribute("sense")]
    public ObjectiveSense Sense
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the linear objective expression.
    /// </summary>
    [XmlElement("expression")]
    public LinearExpression Expression
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional description of the objective.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get;
        set;
    } =
        string.Empty;

    /// <summary>
    /// Validates the mathematical objective.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the objective definition is invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                Name))
        {
            throw new InvalidOperationException(
                "A mathematical objective name is required.");
        }

        if (Sense ==
            ObjectiveSense.Unknown)
        {
            throw new InvalidOperationException(
                "A mathematical objective sense is required.");
        }

        ArgumentNullException.ThrowIfNull(
            Expression);

        Expression.EnsureValid();
    }

    /// <summary>
    /// Creates an independent copy of this objective.
    /// </summary>
    /// <returns>
    /// Cloned mathematical objective.
    /// </returns>
    public MathematicalObjective Clone()
    {
        return new MathematicalObjective
        {
            Name =
                Name,

            Sense =
                Sense,

            Expression =
                Expression.Clone(),

            Description =
                Description
        };
    }
}
