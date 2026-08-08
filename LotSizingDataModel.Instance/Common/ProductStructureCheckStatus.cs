using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Common;

/// <summary>
/// Indicates the current status of the declaration,
/// automatic detection and verification of a product
/// bill-of-materials structure.
/// </summary>
/// <remarks>
/// The status compares an optional structure type declared
/// by an instance author with the type detected automatically
/// from the supply-chain bill-of-materials graph.
///
/// Cyclic or otherwise invalid product structures are reported
/// using <see cref="Invalid"/>.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productStructureCheckStatus")]
public enum ProductStructureCheckStatus
{
    /// <summary>
    /// No product-structure type has been declared and
    /// no automatic analysis has been performed.
    /// </summary>
    [XmlEnum("notAnalyzed")]
    NotAnalyzed,

    /// <summary>
    /// A product-structure type has been declared, but
    /// the supply-chain bill of materials has not yet
    /// been analyzed automatically.
    /// </summary>
    [XmlEnum("declaredOnly")]
    DeclaredOnly,

    /// <summary>
    /// A product-structure type has been detected
    /// automatically, but no declared type is available
    /// for comparison.
    /// </summary>
    [XmlEnum("detectedOnly")]
    DetectedOnly,

    /// <summary>
    /// The declared product-structure type is consistent
    /// with the automatically detected type.
    /// </summary>
    [XmlEnum("declaredAndConfirmed")]
    DeclaredAndConfirmed,

    /// <summary>
    /// The declared product-structure type differs from
    /// the automatically detected type.
    /// </summary>
    /// <remarks>
    /// Both values should be retained so that the discrepancy
    /// can be reviewed rather than silently corrected.
    /// </remarks>
    [XmlEnum("declaredAndContradicted")]
    DeclaredAndContradicted,

    /// <summary>
    /// The bill-of-materials graph is invalid and cannot
    /// be assigned a valid product-structure type.
    /// </summary>
    /// <remarks>
    /// Typical causes include cycles, unresolved item
    /// references or inconsistent component relationships.
    /// </remarks>
    [XmlEnum("invalid")]
    Invalid,

    /// <summary>
    /// A product-structure analysis exists, but the
    /// supply-chain data has changed since it was performed.
    /// </summary>
    /// <remarks>
    /// The product structure must be analyzed again before
    /// the detected type can be used reliably.
    /// </remarks>
    [XmlEnum("outdated")]
    Outdated
}