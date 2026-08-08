using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Common;

/// <summary>
/// Indicates the current status of the automatic
/// classification of a lot-sizing problem instance.
/// </summary>
/// <remarks>
/// This status describes the result and validity of the
/// classification process.
///
/// It is independent of the feasibility or optimality of any
/// solution associated with the instance.
/// </remarks>
[Serializable]
[XmlType(TypeName = "problemClassificationStatus")]
public enum ProblemClassificationStatus
{
    /// <summary>
    /// The supply-chain instance has not yet been analyzed
    /// by the lot-sizing problem classifier.
    /// </summary>
    [XmlEnum("notAnalyzed")]
    NotAnalyzed,

    /// <summary>
    /// The complete problem has been associated with a known
    /// lot-sizing family or with a clearly identified extension
    /// of a known family.
    /// </summary>
    /// <remarks>
    /// A classified problem may still contain additional
    /// explicitly recorded features.
    /// </remarks>
    [XmlEnum("classified")]
    Classified,

    /// <summary>
    /// Some known lot-sizing structures, relaxations or
    /// subproblems have been identified, but no classification
    /// fully describes the complete problem.
    /// </summary>
    /// <remarks>
    /// For example, each item may contain an LS-U relaxation
    /// while the complete instance also includes shared
    /// capacities, transportation and multi-level interactions.
    /// </remarks>
    [XmlEnum("partiallyClassified")]
    PartiallyClassified,

    /// <summary>
    /// Several known problem families match the instance
    /// equally well and no unique primary classification
    /// can be selected automatically.
    /// </summary>
    /// <remarks>
    /// The candidate classifications and their evidence
    /// should be retained for manual review.
    /// </remarks>
    [XmlEnum("ambiguous")]
    Ambiguous,

    /// <summary>
    /// The analysis completed successfully, but no known
    /// problem family provides a meaningful classification
    /// of the complete instance or one of its substructures.
    /// </summary>
    [XmlEnum("unclassified")]
    Unclassified,

    /// <summary>
    /// The supply-chain data is structurally invalid and
    /// therefore cannot be classified reliably.
    /// </summary>
    /// <remarks>
    /// Examples include cyclic bills of materials, unresolved
    /// references or inconsistent planning horizons.
    /// </remarks>
    [XmlEnum("invalid")]
    Invalid,

    /// <summary>
    /// A classification exists, but the supply-chain instance
    /// has changed since that classification was produced.
    /// </summary>
    /// <remarks>
    /// The problem must be analyzed again before the recorded
    /// classification can be used to select a solution method.
    /// </remarks>
    [XmlEnum("outdated")]
    Outdated
}