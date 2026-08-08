using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Identifies the domain of a mathematical decision variable.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalVariableType")]
public enum MathematicalVariableType
{
    /// <summary>
    /// The variable type is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The variable may take any real value within its bounds.
    /// </summary>
    Continuous = 1,

    /// <summary>
    /// The variable may take only integer values within its
    /// bounds.
    /// </summary>
    Integer = 2,

    /// <summary>
    /// The variable may take only the values zero and one.
    /// </summary>
    Binary = 3,

    /// <summary>
    /// The variable is either zero or belongs to a continuous
    /// interval bounded away from zero.
    /// </summary>
    SemiContinuous = 4,

    /// <summary>
    /// The variable is either zero or belongs to an integer
    /// interval bounded away from zero.
    /// </summary>
    SemiInteger = 5
}
