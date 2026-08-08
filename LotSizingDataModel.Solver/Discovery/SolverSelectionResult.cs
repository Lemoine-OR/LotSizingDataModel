using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Represents the result of selecting a solver adapter for a
/// solve request.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverSelectionResult")]
public sealed class SolverSelectionResult
{
    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty solver-selection result.
    /// </summary>
    public SolverSelectionResult()
    {
        RequestedSolver =
            SolverKind.Unknown;

        SelectedSolver =
            SolverKind.Unknown;

        AdapterId =
            string.Empty;

        AdapterName =
            string.Empty;
    }

    /// <summary>
    /// Gets or sets the solver requested by the caller.
    /// </summary>
    [XmlAttribute("requestedSolver")]
    public SolverKind RequestedSolver
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver selected by the selection
    /// service.
    /// </summary>
    [XmlAttribute("selectedSolver")]
    public SolverKind SelectedSolver
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identifier of the selected adapter.
    /// </summary>
    [XmlElement("adapterId")]
    public string AdapterId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the display name of the selected adapter.
    /// </summary>
    [XmlElement("adapterName")]
    public string AdapterName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the selected live adapter instance.
    /// </summary>
    /// <remarks>
    /// The adapter is excluded from XML serialization because
    /// it is a runtime object.
    /// </remarks>
    [XmlIgnore]
    public ISolverAdapter? Adapter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the availability information associated
    /// with the selected solver.
    /// </summary>
    [XmlElement("availability")]
    public SolverAvailabilityInfo? Availability
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the diagnostics produced during solver selection.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether a usable adapter was
    /// selected.
    /// </summary>
    [XmlIgnore]
    public bool IsSuccessful =>
        Adapter is not null &&
        SelectedSolver is not
            SolverKind.Unknown and not
            SolverKind.Automatic &&
        Availability is not null &&
        Availability.IsUsable;

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
                "A solver-selection diagnostic cannot be empty.",
                nameof(message));
        }

        _diagnostics.Add(
            message.Trim());
    }

    /// <summary>
    /// Sets the selected adapter and copies its identity
    /// information into this result.
    /// </summary>
    /// <param name="adapter">
    /// Selected solver adapter.
    /// </param>
    /// <param name="availability">
    /// Availability information associated with the adapter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="adapter"/> or
    /// <paramref name="availability"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void SetSelection(
        ISolverAdapter adapter,
        SolverAvailabilityInfo availability)
    {
        ArgumentNullException.ThrowIfNull(
            adapter);

        ArgumentNullException.ThrowIfNull(
            availability);

        Adapter =
            adapter;

        Availability =
            availability;

        SelectedSolver =
            adapter.SolverKind;

        AdapterId =
            adapter.AdapterId;

        AdapterName =
            adapter.AdapterName;
    }
}
