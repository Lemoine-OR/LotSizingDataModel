using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Common;

/// <summary>
/// Represents one named parameter used by a
/// solution-generation method.
/// </summary>
/// <remarks>
/// Parameter values are stored as strings so that the class
/// can represent parameters belonging to any algorithm,
/// solver, heuristic or external application.
/// </remarks>
[Serializable]
[XmlType(TypeName = "algorithmParameter")]
public sealed class AlgorithmParameter
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private string _dataType = string.Empty;
    private string _description = string.Empty;

    /// <summary>
    /// Initializes an empty algorithm parameter.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public AlgorithmParameter()
    {
    }

    /// <summary>
    /// Initializes a named algorithm parameter.
    /// </summary>
    /// <param name="name">
    /// Parameter name.
    /// </param>
    /// <param name="value">
    /// Serialized parameter value.
    /// </param>
    /// <param name="dataType">
    /// Optional description of the value type.
    /// </param>
    /// <param name="description">
    /// Optional human-readable description.
    /// </param>
    public AlgorithmParameter(
        string name,
        string value,
        string dataType = "",
        string description = "")
    {
        Name = name;
        Value = value;
        DataType = dataType;
        Description = description;
    }

    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "The parameter name cannot be empty.",
                    nameof(value));
            }

            _name = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the serialized parameter value.
    /// </summary>
    [XmlAttribute("value")]
    public string Value
    {
        get => _value;
        set => _value = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets an optional description of the value type.
    /// </summary>
    /// <remarks>
    /// Typical values include integer, double, boolean,
    /// string and enumeration.
    /// </remarks>
    [XmlAttribute("dataType")]
    public string DataType
    {
        get => _dataType;
        set => _dataType = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets an optional human-readable description
    /// of the parameter.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get => _description;
        set => _description = value ?? string.Empty;
    }

    /// <summary>
    /// Creates an integer algorithm parameter.
    /// </summary>
    /// <param name="name">
    /// Parameter name.
    /// </param>
    /// <param name="value">
    /// Integer parameter value.
    /// </param>
    /// <param name="description">
    /// Optional parameter description.
    /// </param>
    /// <returns>
    /// A parameter containing the serialized integer value.
    /// </returns>
    public static AlgorithmParameter FromInteger(
        string name,
        int value,
        string description = "")
    {
        return new AlgorithmParameter(
            name,
            value.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            "integer",
            description);
    }

    /// <summary>
    /// Creates a floating-point algorithm parameter.
    /// </summary>
    /// <param name="name">
    /// Parameter name.
    /// </param>
    /// <param name="value">
    /// Floating-point parameter value.
    /// </param>
    /// <param name="description">
    /// Optional parameter description.
    /// </param>
    /// <returns>
    /// A parameter containing the serialized floating-point value.
    /// </returns>
    public static AlgorithmParameter FromDouble(
        string name,
        double value,
        string description = "")
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "The parameter value must be finite.");
        }

        return new AlgorithmParameter(
            name,
            value.ToString(
                "R",
                System.Globalization.CultureInfo.InvariantCulture),
            "double",
            description);
    }

    /// <summary>
    /// Creates a Boolean algorithm parameter.
    /// </summary>
    /// <param name="name">
    /// Parameter name.
    /// </param>
    /// <param name="value">
    /// Boolean parameter value.
    /// </param>
    /// <param name="description">
    /// Optional parameter description.
    /// </param>
    /// <returns>
    /// A parameter containing the serialized Boolean value.
    /// </returns>
    public static AlgorithmParameter FromBoolean(
        string name,
        bool value,
        string description = "")
    {
        return new AlgorithmParameter(
            name,
            value ? "true" : "false",
            "boolean",
            description);
    }

    /// <summary>
    /// Creates a string algorithm parameter.
    /// </summary>
    /// <param name="name">
    /// Parameter name.
    /// </param>
    /// <param name="value">
    /// String parameter value.
    /// </param>
    /// <param name="description">
    /// Optional parameter description.
    /// </param>
    /// <returns>
    /// A parameter containing the string value.
    /// </returns>
    public static AlgorithmParameter FromString(
        string name,
        string value,
        string description = "")
    {
        return new AlgorithmParameter(
            name,
            value ?? string.Empty,
            "string",
            description);
    }

    /// <summary>
    /// Attempts to read the parameter value as an integer.
    /// </summary>
    /// <param name="value">
    /// Parsed integer value.
    /// </param>
    /// <returns>
    /// True when the value can be parsed; otherwise, false.
    /// </returns>
    public bool TryGetInteger(out int value)
    {
        return int.TryParse(
            Value,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    /// <summary>
    /// Attempts to read the parameter value as a
    /// floating-point number.
    /// </summary>
    /// <param name="value">
    /// Parsed floating-point value.
    /// </param>
    /// <returns>
    /// True when the value can be parsed and is finite;
    /// otherwise, false.
    /// </returns>
    public bool TryGetDouble(out double value)
    {
        bool parsed = double.TryParse(
            Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);

        return parsed &&
               double.IsFinite(value);
    }

    /// <summary>
    /// Attempts to read the parameter value as a Boolean.
    /// </summary>
    /// <param name="value">
    /// Parsed Boolean value.
    /// </param>
    /// <returns>
    /// True when the value can be parsed; otherwise, false.
    /// </returns>
    public bool TryGetBoolean(out bool value)
    {
        return bool.TryParse(
            Value,
            out value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(DataType)
            ? $"{Name} = {Value}"
            : $"{Name} = {Value} ({DataType})";
    }
}