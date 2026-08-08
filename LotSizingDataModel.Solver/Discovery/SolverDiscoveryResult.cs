using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Represents the complete result of solver and adapter
/// discovery on the current computer.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverDiscoveryResult")]
public sealed class SolverDiscoveryResult
{
    private readonly List<SolverDiscoveryCandidate>
        _candidates =
            new();

    private readonly List<SolverAdapterDescriptor>
        _adapterDescriptors =
            new();

    private readonly List<SolverAvailabilityInfo>
        _availabilityInformation =
            new();

    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty solver-discovery result.
    /// </summary>
    public SolverDiscoveryResult()
    {
        StartedAtUtc =
            DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which discovery
    /// started.
    /// </summary>
    [XmlElement("startedAtUtc")]
    public DateTime StartedAtUtc
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which discovery
    /// completed.
    /// </summary>
    [XmlElement("completedAtUtc")]
    public DateTime? CompletedAtUtc
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the elapsed discovery time in seconds.
    /// </summary>
    [XmlElement("elapsedSeconds")]
    public double ElapsedSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the discovered solver installation and adapter
    /// location candidates.
    /// </summary>
    [XmlArray("candidates")]
    [XmlArrayItem("candidate")]
    public List<SolverDiscoveryCandidate> Candidates =>
        _candidates;

    /// <summary>
    /// Gets the solver adapter descriptors discovered during
    /// the scan.
    /// </summary>
    [XmlArray("adapterDescriptors")]
    [XmlArrayItem("adapter")]
    public List<SolverAdapterDescriptor> AdapterDescriptors =>
        _adapterDescriptors;

    /// <summary>
    /// Gets the availability information reported for detected
    /// solvers.
    /// </summary>
    [XmlArray("availabilityInformation")]
    [XmlArrayItem("solver")]
    public List<SolverAvailabilityInfo>
        AvailabilityInformation =>
            _availabilityInformation;

    /// <summary>
    /// Gets the diagnostics produced during discovery.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether at least one solver is
    /// currently usable.
    /// </summary>
    [XmlIgnore]
    public bool HasUsableSolver =>
        _availabilityInformation.Exists(
            availability =>
                availability.IsUsable);

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
                "A discovery diagnostic cannot be empty.",
                nameof(message));
        }

        _diagnostics.Add(
            message.Trim());
    }

    /// <summary>
    /// Gets the availability information associated with a
    /// solver kind.
    /// </summary>
    /// <param name="solverKind">
    /// Solver kind to locate.
    /// </param>
    /// <returns>
    /// Matching availability information, or
    /// <see langword="null"/> when none is available.
    /// </returns>
    public SolverAvailabilityInfo? FindAvailability(
        SolverKind solverKind)
    {
        return _availabilityInformation.Find(
            availability =>
                availability.SolverKind ==
                solverKind);
    }
}
