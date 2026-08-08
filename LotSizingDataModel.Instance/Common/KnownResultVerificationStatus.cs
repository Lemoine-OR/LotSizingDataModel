using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Common;

/// <summary>
/// Indicates the level of verification associated with a
/// known result for a lot-sizing problem instance.
/// </summary>
/// <remarks>
/// The verification status is independent of the result's
/// feasibility and optimality claims.
///
/// A result may contain only an objective value reported by
/// a publication, without a detailed solution that can be
/// checked automatically.
/// </remarks>
[Serializable]
[XmlType(TypeName = "knownResultVerificationStatus")]
public enum KnownResultVerificationStatus
{
    /// <summary>
    /// No verification information has been recorded.
    /// </summary>
    [XmlEnum("notVerified")]
    NotVerified,

    /// <summary>
    /// The result is reported by an external source, but has
    /// not been reproduced or checked using the current data
    /// model.
    /// </summary>
    /// <remarks>
    /// This status is appropriate for an objective value
    /// extracted from an article when the detailed decision
    /// variables are unavailable.
    /// </remarks>
    [XmlEnum("sourceReported")]
    SourceReported,

    /// <summary>
    /// The result has been reproduced by executing the
    /// reported method or an equivalent implementation.
    /// </summary>
    /// <remarks>
    /// Reproduction does not necessarily constitute an
    /// independent verification when the same implementation,
    /// data transformation or assumptions are reused.
    /// </remarks>
    [XmlEnum("reproduced")]
    Reproduced,

    /// <summary>
    /// The result has been checked automatically against the
    /// current supply-chain instance.
    /// </summary>
    /// <remarks>
    /// Automatic verification may include reference checks,
    /// decision-domain checks, constraint evaluation and
    /// objective-value recomputation.
    /// </remarks>
    [XmlEnum("automaticallyVerified")]
    AutomaticallyVerified,

    /// <summary>
    /// The result has been independently verified using a
    /// separate implementation, method or verification
    /// process.
    /// </summary>
    [XmlEnum("independentlyVerified")]
    IndependentlyVerified,

    /// <summary>
    /// The result is disputed because available evidence is
    /// insufficient or conflicting.
    /// </summary>
    /// <remarks>
    /// A disputed result is retained for traceability and
    /// must not automatically be used as the best known
    /// result.
    /// </remarks>
    [XmlEnum("disputed")]
    Disputed,

    /// <summary>
    /// The result has been shown to be invalid.
    /// </summary>
    /// <remarks>
    /// Typical causes include an infeasible detailed solution,
    /// an incorrect objective value, incompatible instance
    /// data or an erroneous optimality claim.
    /// </remarks>
    [XmlEnum("invalidated")]
    Invalidated
}