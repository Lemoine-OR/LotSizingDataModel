using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Monitoring;

/// <summary>
/// Defines how solver progress snapshots are retained in the
/// solution convergence history.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverProgressRecordingMode")]
public enum SolverProgressRecordingMode
{
    /// <summary>
    /// No progress snapshot is retained.
    /// </summary>
    None = 0,

    /// <summary>
    /// Only snapshots associated with a new incumbent solution
    /// are retained.
    /// </summary>
    IncumbentOnly = 1,

    /// <summary>
    /// Snapshots are retained at a configurable elapsed-time
    /// interval.
    /// </summary>
    TimeInterval = 2,

    /// <summary>
    /// Snapshots are retained at a configurable search-node
    /// interval.
    /// </summary>
    NodeInterval = 3,

    /// <summary>
    /// Snapshots are retained when the incumbent objective,
    /// best bound, or optimality gap changes.
    /// </summary>
    SignificantChanges = 4,

    /// <summary>
    /// Every snapshot reported by the solver adapter is
    /// retained.
    /// </summary>
    Full = 5
}
