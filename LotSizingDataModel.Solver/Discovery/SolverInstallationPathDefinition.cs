using System;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Describes a conventional installation path that may contain
/// a supported solver.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverInstallationPathDefinition")]
public sealed class SolverInstallationPathDefinition
{
    /// <summary>
    /// Initializes an empty installation-path definition.
    /// </summary>
    public SolverInstallationPathDefinition()
    {
        PathPattern =
            string.Empty;

        Description =
            string.Empty;

        SolverKind =
            SolverKind.Unknown;
    }

    /// <summary>
    /// Initializes an installation-path definition.
    /// </summary>
    /// <param name="solverKind">
    /// Solver associated with the path.
    /// </param>
    /// <param name="pathPattern">
    /// Absolute path or path pattern to inspect.
    /// </param>
    /// <param name="description">
    /// Human-readable description.
    /// </param>
    public SolverInstallationPathDefinition(
        SolverKind solverKind,
        string pathPattern,
        string description)
        : this()
    {
        SolverKind =
            solverKind;

        PathPattern =
            pathPattern;

        Description =
            description;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the solver associated with the path.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the installation path or path pattern.
    /// </summary>
    /// <remarks>
    /// The pattern may contain environment-variable
    /// placeholders such as <c>%ProgramFiles%</c>.
    /// </remarks>
    [XmlElement("pathPattern")]
    public string PathPattern
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
    /// Gets or sets the discovery priority associated with the
    /// path.
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
    /// Gets or sets a value indicating whether subdirectories
    /// should be searched recursively.
    /// </summary>
    [XmlAttribute("recursiveSearch")]
    public bool RecursiveSearch
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional file name expected below the
    /// installation path.
    /// </summary>
    [XmlElement("expectedFileName")]
    public string ExpectedFileName
    {
        get;
        set;
    } =
        string.Empty;

    /// <summary>
    /// Validates the installation-path definition.
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
                "An installation-path definition must target " +
                "a concrete solver kind.");
        }

        if (string.IsNullOrWhiteSpace(
                PathPattern))
        {
            throw new InvalidOperationException(
                "An installation path or path pattern is " +
                "required.");
        }

        if (Priority < 0)
        {
            throw new InvalidOperationException(
                "An installation-path priority cannot be " +
                "negative.");
        }
    }
}
