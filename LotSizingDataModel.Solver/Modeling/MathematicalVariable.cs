using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Represents one decision variable in a mathematical
/// optimization model.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalVariable")]
public sealed class MathematicalVariable
{
    /// <summary>
    /// Initializes an empty mathematical variable.
    /// </summary>
    public MathematicalVariable()
    {
        Name =
            string.Empty;

        LowerBound =
            0.0;

        UpperBound =
            double.PositiveInfinity;

        VariableType =
            MathematicalVariableType.Continuous;
    }

    /// <summary>
    /// Initializes a mathematical variable.
    /// </summary>
    /// <param name="id">
    /// Unique variable identifier within the model.
    /// </param>
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
    public MathematicalVariable(
        int id,
        string name,
        MathematicalVariableType variableType,
        double lowerBound = 0.0,
        double upperBound = double.PositiveInfinity)
        : this()
    {
        Id =
            id;

        Name =
            name;

        VariableType =
            variableType;

        LowerBound =
            lowerBound;

        UpperBound =
            upperBound;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the unique variable identifier within the
    /// mathematical model.
    /// </summary>
    [XmlAttribute("id")]
    public int Id
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the variable name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the variable domain.
    /// </summary>
    [XmlAttribute("variableType")]
    public MathematicalVariableType VariableType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the variable lower bound.
    /// </summary>
    [XmlAttribute("lowerBound")]
    public double LowerBound
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the variable upper bound.
    /// </summary>
    [XmlAttribute("upperBound")]
    public double UpperBound
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional domain key used to map the
    /// mathematical variable back to a lot-sizing decision.
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
    /// Gets a value indicating whether the variable has a
    /// finite lower bound.
    /// </summary>
    [XmlIgnore]
    public bool HasFiniteLowerBound =>
        !double.IsNegativeInfinity(
            LowerBound);

    /// <summary>
    /// Gets a value indicating whether the variable has a
    /// finite upper bound.
    /// </summary>
    [XmlIgnore]
    public bool HasFiniteUpperBound =>
        !double.IsPositiveInfinity(
            UpperBound);

    /// <summary>
    /// Validates the mathematical variable.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the variable definition is invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (Id <= 0)
        {
            throw new InvalidOperationException(
                "A mathematical variable identifier must be " +
                "strictly positive.");
        }

        if (string.IsNullOrWhiteSpace(
                Name))
        {
            throw new InvalidOperationException(
                "A mathematical variable name is required.");
        }

        if (VariableType ==
            MathematicalVariableType.Unknown)
        {
            throw new InvalidOperationException(
                "A mathematical variable type is required.");
        }

        if (double.IsNaN(
                LowerBound) ||
            double.IsNaN(
                UpperBound))
        {
            throw new InvalidOperationException(
                "Mathematical variable bounds cannot be NaN.");
        }

        if (LowerBound >
            UpperBound)
        {
            throw new InvalidOperationException(
                "A mathematical variable lower bound cannot " +
                "exceed its upper bound.");
        }

        if (VariableType ==
            MathematicalVariableType.Binary &&
            (LowerBound < 0.0 ||
             UpperBound > 1.0))
        {
            throw new InvalidOperationException(
                "A binary variable must have bounds within " +
                "[0, 1].");
        }

        if (VariableType is
                MathematicalVariableType.Integer or
                MathematicalVariableType.Binary or
                MathematicalVariableType.SemiInteger)
        {
            if (HasFiniteLowerBound &&
                LowerBound !=
                    Math.Truncate(
                        LowerBound))
            {
                throw new InvalidOperationException(
                    "An integer variable must have an integer " +
                    "lower bound.");
            }

            if (HasFiniteUpperBound &&
                UpperBound !=
                    Math.Truncate(
                        UpperBound))
            {
                throw new InvalidOperationException(
                    "An integer variable must have an integer " +
                    "upper bound.");
            }
        }
    }
}
