using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Represents one coefficient-variable term in a linear
/// expression.
/// </summary>
[Serializable]
[XmlType(TypeName = "linearTerm")]
public sealed class LinearTerm
{
    /// <summary>
    /// Initializes an empty linear term.
    /// </summary>
    public LinearTerm()
    {
    }

    /// <summary>
    /// Initializes a linear term.
    /// </summary>
    /// <param name="variableId">
    /// Identifier of the referenced mathematical variable.
    /// </param>
    /// <param name="coefficient">
    /// Linear coefficient.
    /// </param>
    public LinearTerm(
        int variableId,
        double coefficient)
    {
        VariableId =
            variableId;

        Coefficient =
            coefficient;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the identifier of the referenced
    /// mathematical variable.
    /// </summary>
    [XmlAttribute("variableId")]
    public int VariableId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the coefficient multiplying the referenced
    /// variable.
    /// </summary>
    [XmlAttribute("coefficient")]
    public double Coefficient
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the linear term.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the variable identifier or coefficient is
    /// invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (VariableId <= 0)
        {
            throw new InvalidOperationException(
                "A linear term must reference a strictly " +
                "positive variable identifier.");
        }

        if (double.IsNaN(
                Coefficient) ||
            double.IsInfinity(
                Coefficient))
        {
            throw new InvalidOperationException(
                "A linear coefficient must be a finite number.");
        }
    }
}
