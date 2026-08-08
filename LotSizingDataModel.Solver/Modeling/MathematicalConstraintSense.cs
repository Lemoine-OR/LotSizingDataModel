using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Identifies the relational sense of a mathematical
/// constraint.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalConstraintSense")]
public enum MathematicalConstraintSense
{
    /// <summary>
    /// The constraint sense is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The left-hand side must be less than or equal to the
    /// right-hand side.
    /// </summary>
    LessThanOrEqual = 1,

    /// <summary>
    /// The left-hand side must be equal to the right-hand side.
    /// </summary>
    Equal = 2,

    /// <summary>
    /// The left-hand side must be greater than or equal to the
    /// right-hand side.
    /// </summary>
    GreaterThanOrEqual = 3
}
