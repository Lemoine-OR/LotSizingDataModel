using System;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Describes a solver-specific environment variable that may
/// identify an installation directory, native library path, or
/// license location.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverEnvironmentVariableDefinition")]
public sealed class SolverEnvironmentVariableDefinition
{
    /// <summary>
    /// Initializes an empty environment-variable definition.
    /// </summary>
    public SolverEnvironmentVariableDefinition()
    {
        VariableName =
            string.Empty;

        Description =
            string.Empty;

        SolverKind =
            SolverKind.Unknown;
    }

    /// <summary>
    /// Initializes an environment-variable definition.
    /// </summary>
    /// <param name="solverKind">
    /// Solver associated with the variable.
    /// </param>
    /// <param name="variableName">
    /// Environment-variable name.
    /// </param>
    /// <param name="description">
    /// Human-readable description.
    /// </param>
    public SolverEnvironmentVariableDefinition(
        SolverKind solverKind,
        string variableName,
        string description)
        : this()
    {
        SolverKind =
            solverKind;

        VariableName =
            variableName;

        Description =
            description;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the solver associated with the variable.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the environment-variable name.
    /// </summary>
    [XmlAttribute("variableName")]
    public string VariableName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a human-readable description.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the variable is
    /// expected to contain a license location rather than an
    /// installation path.
    /// </summary>
    [XmlAttribute("isLicenseVariable")]
    public bool IsLicenseVariable
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the discovery priority associated with the
    /// variable.
    /// </summary>
    /// <remarks>
    /// Lower values represent higher priority.
    /// </remarks>
    [XmlAttribute("priority")]
    public int Priority
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the environment-variable definition.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required metadata is missing or invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (SolverKind is
            SolverKind.Unknown or
            SolverKind.Automatic)
        {
            throw new InvalidOperationException(
                "An environment-variable definition must " +
                "target a concrete solver kind.");
        }

        if (string.IsNullOrWhiteSpace(
                VariableName))
        {
            throw new InvalidOperationException(
                "An environment-variable name is required.");
        }

        if (Priority < 0)
        {
            throw new InvalidOperationException(
                "An environment-variable priority cannot be " +
                "negative.");
        }
    }
}
