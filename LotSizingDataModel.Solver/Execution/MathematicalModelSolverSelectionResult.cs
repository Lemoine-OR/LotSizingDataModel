using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Adapters;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Discovery;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Represents the result of selecting a solver adapter that can
/// solve a solver-independent mathematical model.
/// </summary>
/// <remarks>
/// The general solver-selection result is preserved for
/// diagnostics and availability metadata, while
/// <see cref="Adapter"/> exposes the selected adapter through the
/// stronger mathematical-model solver contract.
/// </remarks>
[Serializable]
[XmlType(TypeName = "mathematicalModelSolverSelectionResult")]
public sealed class MathematicalModelSolverSelectionResult
{
    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty mathematical-model solver-selection
    /// result.
    /// </summary>
    public MathematicalModelSolverSelectionResult()
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
    /// Gets or sets the solver selected by the general
    /// solver-selection service.
    /// </summary>
    [XmlAttribute("selectedSolver")]
    public SolverKind SelectedSolver
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the selected adapter identifier.
    /// </summary>
    [XmlElement("adapterId")]
    public string AdapterId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the selected adapter display name.
    /// </summary>
    [XmlElement("adapterName")]
    public string AdapterName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the availability information associated with
    /// the selected solver.
    /// </summary>
    [XmlElement("availability")]
    public SolverAvailabilityInfo? Availability
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the selected live mathematical-model solver
    /// adapter.
    /// </summary>
    /// <remarks>
    /// Runtime adapter instances are intentionally excluded from
    /// XML serialization.
    /// </remarks>
    [XmlIgnore]
    public IMathematicalModelSolverAdapter? Adapter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the diagnostics produced during adapter selection.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether a usable
    /// mathematical-model solver adapter was selected.
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
    /// Adds a non-empty diagnostic message.
    /// </summary>
    /// <param name="message">
    /// Diagnostic message.
    /// </param>
    public void AddDiagnostic(
        string message)
    {
        if (!string.IsNullOrWhiteSpace(
                message))
        {
            _diagnostics.Add(
                message.Trim());
        }
    }

    /// <summary>
    /// Creates a mathematical-model solver-selection result from
    /// a general solver-selection result.
    /// </summary>
    /// <param name="selection">
    /// General solver-selection result.
    /// </param>
    /// <returns>
    /// Typed mathematical-model solver-selection result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="selection"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static MathematicalModelSolverSelectionResult From(
        SolverSelectionResult selection)
    {
        ArgumentNullException.ThrowIfNull(
            selection);

        var result =
            new MathematicalModelSolverSelectionResult
            {
                RequestedSolver =
                    selection.RequestedSolver,

                SelectedSolver =
                    selection.SelectedSolver,

                AdapterId =
                    selection.AdapterId,

                AdapterName =
                    selection.AdapterName,

                Availability =
                    selection.Availability
            };

        foreach (
            string diagnostic
            in selection.Diagnostics)
        {
            result.AddDiagnostic(
                diagnostic);
        }

        if (selection.Adapter is
            IMathematicalModelSolverAdapter mathematicalAdapter)
        {
            result.Adapter =
                mathematicalAdapter;

            return result;
        }

        if (selection.Adapter is not null)
        {
            result.AddDiagnostic(
                $"Selected adapter '{selection.AdapterName}' " +
                "does not implement " +
                $"{nameof(IMathematicalModelSolverAdapter)}.");
        }

        return result;
    }
}
