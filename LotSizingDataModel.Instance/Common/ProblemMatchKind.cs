using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Common;

/// <summary>
/// Identifies how a supply-chain instance matches
/// a known lot-sizing problem family.
/// </summary>
/// <remarks>
/// A problem may produce several matches of different kinds.
/// For example, the complete problem may be a known extension
/// of a capacitated lot-sizing problem while containing
/// uncapacitated lot-sizing relaxations for individual items.
/// </remarks>
[Serializable]
[XmlType(TypeName = "problemMatchKind")]
public enum ProblemMatchKind
{
    /// <summary>
    /// The nature of the correspondence is unknown
    /// or has not been evaluated.
    /// </summary>
    [XmlEnum("unknown")]
    Unknown,

    /// <summary>
    /// The analyzed problem satisfies all defining conditions
    /// of the known problem family without any additional
    /// unclassified structural feature.
    /// </summary>
    /// <remarks>
    /// Numerical data such as costs, demands and capacities
    /// may naturally differ between instances.
    /// </remarks>
    [XmlEnum("exact")]
    Exact,

    /// <summary>
    /// The analyzed problem satisfies the defining conditions
    /// of a known family but also contains explicitly
    /// identified additional features or constraints.
    /// </summary>
    /// <remarks>
    /// Examples include a capacitated lot-sizing problem
    /// extended with transportation decisions, financial
    /// constraints or additional-capacity decisions.
    /// </remarks>
    [XmlEnum("knownExtension")]
    KnownExtension,

    /// <summary>
    /// A relaxation of the analyzed problem belongs to
    /// the known problem family.
    /// </summary>
    /// <remarks>
    /// The relaxation may be obtained by removing coupling,
    /// capacity, integrality or other constraints from the
    /// complete problem.
    /// </remarks>
    [XmlEnum("recognizedRelaxation")]
    RecognizedRelaxation,

    /// <summary>
    /// One or more identifiable parts of the analyzed problem
    /// belong to the known problem family.
    /// </summary>
    /// <remarks>
    /// For example, a multi-item problem may contain one
    /// recognizable single-item lot-sizing subproblem
    /// for each item.
    /// </remarks>
    [XmlEnum("recognizedSubproblem")]
    RecognizedSubproblem,

    /// <summary>
    /// The known family is the closest available description
    /// of the analyzed problem, but some defining conditions
    /// are not satisfied.
    /// </summary>
    /// <remarks>
    /// This match is informative only. It must not be treated
    /// as proof that algorithms dedicated to the known family
    /// are directly applicable to the complete problem.
    /// </remarks>
    [XmlEnum("closestKnownFamily")]
    ClosestKnownFamily
}