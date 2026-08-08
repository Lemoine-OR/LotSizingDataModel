using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Common;

/// <summary>
/// Indicates the available information about the optimality
/// of a lot-sizing solution.
/// </summary>
/// <remarks>
/// A feasible solution is not necessarily optimal.
/// Heuristic and metaheuristic methods generally produce
/// solutions without an optimality proof.
/// </remarks>
[Serializable]
[XmlType(TypeName = "optimalityStatus")]
public enum OptimalityStatus
{
    /// <summary>
    /// The solution has not been evaluated from an
    /// optimality perspective.
    /// </summary>
    [XmlEnum("notEvaluated")]
    NotEvaluated,

    /// <summary>
    /// No proof of optimality is available for the solution.
    /// </summary>
    /// <remarks>
    /// The solution may still be feasible and of high quality.
    /// </remarks>
    [XmlEnum("noProof")]
    NoProof,

    /// <summary>
    /// The solution has been proven optimal for the
    /// associated mathematical model.
    /// </summary>
    [XmlEnum("provenOptimal")]
    ProvenOptimal
}