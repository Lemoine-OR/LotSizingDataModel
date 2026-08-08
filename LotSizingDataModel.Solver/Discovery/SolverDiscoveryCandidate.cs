using System;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Represents one candidate solver installation or adapter
/// location discovered on the current computer.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverDiscoveryCandidate")]
public sealed class SolverDiscoveryCandidate
{
    /// <summary>
    /// Initializes an empty solver-discovery candidate.
    /// </summary>
    public SolverDiscoveryCandidate()
    {
        Path =
            string.Empty;

        Description =
            string.Empty;

        VersionHint =
            string.Empty;

        Source =
            SolverDiscoverySource.Unknown;

        SolverKind =
            SolverKind.Unknown;
    }

    /// <summary>
    /// Gets or sets the solver kind associated with the
    /// candidate.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the discovery source.
    /// </summary>
    [XmlAttribute("source")]
    public SolverDiscoverySource Source
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the discovered directory, assembly,
    /// native-library, or executable path.
    /// </summary>
    [XmlElement("path")]
    public string Path
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// candidate.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional solver-version hint inferred
    /// from the discovered path or file metadata.
    /// </summary>
    [XmlElement("versionHint")]
    public string VersionHint
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the priority assigned to the candidate.
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
    /// Gets or sets a value indicating whether the candidate
    /// path currently exists.
    /// </summary>
    [XmlAttribute("exists")]
    public bool Exists
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the discovery candidate.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required information is missing or invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (SolverKind is
            SolverKind.Unknown or
            SolverKind.Automatic)
        {
            throw new InvalidOperationException(
                "A discovery candidate must target a concrete " +
                "solver kind.");
        }

        if (Source ==
            SolverDiscoverySource.Unknown)
        {
            throw new InvalidOperationException(
                "A discovery source is required.");
        }

        if (string.IsNullOrWhiteSpace(
                Path))
        {
            throw new InvalidOperationException(
                "A discovery candidate path is required.");
        }

        if (Priority < 0)
        {
            throw new InvalidOperationException(
                "A discovery candidate priority cannot be " +
                "negative.");
        }
    }
}
