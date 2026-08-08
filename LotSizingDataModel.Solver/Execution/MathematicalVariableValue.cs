using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Stores the value returned by a solver for one mathematical
/// decision variable.
/// </summary>
/// <remarks>
/// The variable identifier is the primary link to the
/// solver-independent mathematical model. The optional variable
/// name and domain key are copied for diagnostics, exports, and
/// later mapping back to lot-sizing decisions.
/// </remarks>
[Serializable]
[XmlType(TypeName = "mathematicalVariableValue")]
public sealed class MathematicalVariableValue
{
    /// <summary>
    /// Initializes an empty mathematical-variable value.
    /// </summary>
    public MathematicalVariableValue()
    {
        VariableName =
            string.Empty;

        DomainKey =
            string.Empty;
    }

    /// <summary>
    /// Initializes a mathematical-variable value.
    /// </summary>
    /// <param name="variableId">
    /// Identifier of the mathematical variable.
    /// </param>
    /// <param name="value">
    /// Value returned by the solver.
    /// </param>
    /// <param name="variableName">
    /// Optional mathematical-variable name.
    /// </param>
    /// <param name="domainKey">
    /// Optional business-domain key.
    /// </param>
    public MathematicalVariableValue(
        int variableId,
        double value,
        string variableName = "",
        string domainKey = "")
        : this()
    {
        VariableId =
            variableId;

        Value =
            value;

        VariableName =
            variableName?.Trim() ??
            string.Empty;

        DomainKey =
            domainKey?.Trim() ??
            string.Empty;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the identifier of the mathematical
    /// variable.
    /// </summary>
    [XmlAttribute("variableId")]
    public int VariableId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mathematical-variable name.
    /// </summary>
    [XmlAttribute("variableName")]
    public string VariableName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the business-domain key associated with the
    /// mathematical variable.
    /// </summary>
    [XmlElement("domainKey")]
    public string DomainKey
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the value returned by the solver.
    /// </summary>
    [XmlAttribute("value")]
    public double Value
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the mathematical-variable value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the variable identifier or returned value is
    /// invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (VariableId <= 0)
        {
            throw new InvalidOperationException(
                "A mathematical-variable value must reference a " +
                "strictly positive variable identifier.");
        }

        if (double.IsNaN(
                Value) ||
            double.IsInfinity(
                Value))
        {
            throw new InvalidOperationException(
                "A mathematical-variable value must be finite.");
        }

        VariableName =
            VariableName?.Trim() ??
            string.Empty;

        DomainKey =
            DomainKey?.Trim() ??
            string.Empty;
    }
}
