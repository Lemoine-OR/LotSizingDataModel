using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Represents one linear constraint in a mathematical
/// optimization model.
/// </summary>
[Serializable]
[XmlType(TypeName = "linearConstraint")]
public sealed class LinearConstraint
{
    /// <summary>
    /// Initializes an empty linear constraint.
    /// </summary>
    public LinearConstraint()
    {
        Name =
            string.Empty;

        LeftHandSide =
            new LinearExpression();

        Sense =
            MathematicalConstraintSense.Equal;
    }

    /// <summary>
    /// Initializes a linear constraint.
    /// </summary>
    /// <param name="id">
    /// Unique constraint identifier within the model.
    /// </param>
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
    /// Right-hand-side constant value.
    /// </param>
    public LinearConstraint(
        int id,
        string name,
        LinearExpression leftHandSide,
        MathematicalConstraintSense sense,
        double rightHandSide)
        : this()
    {
        ArgumentNullException.ThrowIfNull(
            leftHandSide);

        Id =
            id;

        Name =
            name;

        LeftHandSide =
            leftHandSide;

        Sense =
            sense;

        RightHandSide =
            rightHandSide;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the unique constraint identifier within the
    /// mathematical model.
    /// </summary>
    [XmlAttribute("id")]
    public int Id
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the constraint name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the left-hand-side linear expression.
    /// </summary>
    [XmlElement("leftHandSide")]
    public LinearExpression LeftHandSide
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the relational sense of the constraint.
    /// </summary>
    [XmlAttribute("sense")]
    public MathematicalConstraintSense Sense
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the right-hand-side constant value.
    /// </summary>
    [XmlAttribute("rightHandSide")]
    public double RightHandSide
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional domain key used to identify the
    /// business meaning of the constraint.
    /// </summary>
    [XmlElement("domainKey")]
    public string DomainKey
    {
        get;
        set;
    } =
        string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get;
        set;
    } =
        string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the constraint
    /// is enabled.
    /// </summary>
    [XmlAttribute("isEnabled")]
    public bool IsEnabled
    {
        get;
        set;
    } =
        true;

    /// <summary>
    /// Validates the linear constraint.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the constraint definition is invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (Id <= 0)
        {
            throw new InvalidOperationException(
                "A linear constraint identifier must be " +
                "strictly positive.");
        }

        if (string.IsNullOrWhiteSpace(
                Name))
        {
            throw new InvalidOperationException(
                "A linear constraint name is required.");
        }

        ArgumentNullException.ThrowIfNull(
            LeftHandSide);

        LeftHandSide.EnsureValid();

        if (Sense ==
            MathematicalConstraintSense.Unknown)
        {
            throw new InvalidOperationException(
                "A mathematical constraint sense is required.");
        }

        if (double.IsNaN(
                RightHandSide) ||
            double.IsInfinity(
                RightHandSide))
        {
            throw new InvalidOperationException(
                "A linear constraint right-hand side must be a " +
                "finite number.");
        }
    }

    /// <summary>
    /// Creates an independent copy of this constraint.
    /// </summary>
    /// <returns>
    /// Cloned linear constraint.
    /// </returns>
    public LinearConstraint Clone()
    {
        return new LinearConstraint
        {
            Id =
                Id,

            Name =
                Name,

            LeftHandSide =
                LeftHandSide.Clone(),

            Sense =
                Sense,

            RightHandSide =
                RightHandSide,

            DomainKey =
                DomainKey,

            Description =
                Description,

            IsEnabled =
                IsEnabled
        };
    }
}
