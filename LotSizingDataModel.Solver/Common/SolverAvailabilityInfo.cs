using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Describes the availability and detected installation details
/// of a mathematical optimization solver.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverAvailabilityInfo")]
public sealed class SolverAvailabilityInfo
{
    private readonly List<string> _diagnostics =
        new();

    private readonly List<string> _limitations =
        new();

    /// <summary>
    /// Initializes an empty availability description.
    /// </summary>
    public SolverAvailabilityInfo()
    {
        SolverName =
            string.Empty;

        SolverVersion =
            string.Empty;

        InstallationPath =
            string.Empty;

        ManagedAssemblyPath =
            string.Empty;

        NativeLibraryPath =
            string.Empty;

        LicenseInformation =
            string.Empty;
    }

    /// <summary>
    /// Initializes an availability description for a solver.
    /// </summary>
    /// <param name="solverKind">
    /// Solver kind.
    /// </param>
    /// <param name="status">
    /// Availability status.
    /// </param>
    public SolverAvailabilityInfo(
        SolverKind solverKind,
        SolverAvailabilityStatus status)
        : this()
    {
        SolverKind =
            solverKind;

        Status =
            status;
    }

    /// <summary>
    /// Gets or sets the solver kind.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the detected availability status.
    /// </summary>
    [XmlAttribute("status")]
    public SolverAvailabilityStatus Status
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the display name of the solver.
    /// </summary>
    [XmlElement("solverName")]
    public string SolverName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the detected solver version.
    /// </summary>
    [XmlElement("solverVersion")]
    public string SolverVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the detected installation directory.
    /// </summary>
    [XmlElement("installationPath")]
    public string InstallationPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the path of the managed solver assembly,
    /// when applicable.
    /// </summary>
    [XmlElement("managedAssemblyPath")]
    public string ManagedAssemblyPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the path of the native solver library or
    /// executable, when applicable.
    /// </summary>
    [XmlElement("nativeLibraryPath")]
    public string NativeLibraryPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets information about the detected solver
    /// license.
    /// </summary>
    [XmlElement("licenseInformation")]
    public string LicenseInformation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the solver can
    /// currently be used.
    /// </summary>
    [XmlIgnore]
    public bool IsUsable =>
        Status is
            SolverAvailabilityStatus.Available or
            SolverAvailabilityStatus.AvailableWithLimitations;

    /// <summary>
    /// Gets the diagnostic messages produced during solver
    /// discovery and validation.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets the functional limitations detected for the solver.
    /// </summary>
    [XmlArray("limitations")]
    [XmlArrayItem("limitation")]
    public List<string> Limitations =>
        _limitations;

    /// <summary>
    /// Adds a diagnostic message.
    /// </summary>
    /// <param name="message">
    /// Diagnostic message.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="message"/> is empty.
    /// </exception>
    public void AddDiagnostic(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            throw new ArgumentException(
                "A diagnostic message cannot be empty.",
                nameof(message));
        }

        _diagnostics.Add(
            message.Trim());
    }

    /// <summary>
    /// Adds a functional limitation.
    /// </summary>
    /// <param name="limitation">
    /// Limitation description.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="limitation"/> is empty.
    /// </exception>
    public void AddLimitation(
        string limitation)
    {
        if (string.IsNullOrWhiteSpace(
                limitation))
        {
            throw new ArgumentException(
                "A limitation description cannot be empty.",
                nameof(limitation));
        }

        _limitations.Add(
            limitation.Trim());
    }
}
