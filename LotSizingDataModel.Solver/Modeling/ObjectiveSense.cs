using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Identifies the optimization direction of a mathematical
/// objective.
/// </summary>
[Serializable]
[XmlType(TypeName = "objectiveSense")]
public enum ObjectiveSense
{
    /// <summary>
    /// The objective sense is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The objective value must be minimized.
    /// </summary>
    Minimize = 1,

    /// <summary>
    /// The objective value must be maximized.
    /// </summary>
    Maximize = 2
}
