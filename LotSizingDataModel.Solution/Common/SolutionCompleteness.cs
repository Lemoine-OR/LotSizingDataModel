using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Common;

/// <summary>
/// Indicates whether a lot-sizing solution contains all
/// decision values expected for its associated instance.
/// </summary>
/// <remarks>
/// Completeness is independent of feasibility and optimality.
/// A complete solution may be infeasible, while a partial solution
/// may contain valid values for the decisions that are present.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solutionCompleteness")]
public enum SolutionCompleteness
{
    /// <summary>
    /// The completeness of the solution has not been evaluated.
    /// </summary>
    [XmlEnum("unknown")]
    Unknown,

    /// <summary>
    /// Only part of the decision values expected for the
    /// associated instance is available.
    /// </summary>
    [XmlEnum("partial")]
    Partial,

    /// <summary>
    /// Every decision value expected for the associated
    /// instance is available.
    /// </summary>
    [XmlEnum("complete")]
    Complete
}