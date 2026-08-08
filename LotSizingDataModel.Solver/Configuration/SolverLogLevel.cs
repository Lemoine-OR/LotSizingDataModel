using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Configuration;

/// <summary>
/// Defines the amount of information written by a solver during
/// an optimization run.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverLogLevel")]
public enum SolverLogLevel
{
    /// <summary>
    /// No solver log is requested.
    /// </summary>
    None = 0,

    /// <summary>
    /// Only errors are written.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Errors and important warnings are written.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Standard solver progress information is written.
    /// </summary>
    Information = 3,

    /// <summary>
    /// Detailed solver progress and diagnostic information is
    /// written.
    /// </summary>
    Detailed = 4,

    /// <summary>
    /// The most verbose solver output available is written.
    /// </summary>
    Trace = 5
}
