using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Common;

/// <summary>
/// Indicates the evaluated feasibility status of a
/// lot-sizing solution candidate.
/// </summary>
/// <remarks>
/// This status describes whether the solution satisfies the
/// constraints of its associated instance. It is independent
/// of the method used to generate the solution and of any
/// optimality information.
/// </remarks>
[Serializable]
[XmlType(TypeName = "feasibilityStatus")]
public enum FeasibilityStatus
{
    /// <summary>
    /// The solution has not been evaluated for feasibility.
    /// </summary>
    [XmlEnum("notEvaluated")]
    NotEvaluated,

    /// <summary>
    /// Only part of the solution constraints has been evaluated.
    ///
    /// No conclusion can be drawn about the complete feasibility
    /// of the solution.
    /// </summary>
    [XmlEnum("partiallyEvaluated")]
    PartiallyEvaluated,

    /// <summary>
    /// Every evaluated constraint is satisfied and the solution
    /// has been declared feasible.
    /// </summary>
    [XmlEnum("feasible")]
    Feasible,

    /// <summary>
    /// At least one constraint is violated and the solution
    /// has been declared infeasible.
    /// </summary>
    [XmlEnum("infeasible")]
    Infeasible
}