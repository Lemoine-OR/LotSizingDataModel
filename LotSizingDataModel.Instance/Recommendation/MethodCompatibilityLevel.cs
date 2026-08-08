using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Indicates the compatibility level between a solution
/// method and a lot-sizing problem instance.
/// </summary>
/// <remarks>
/// Compatibility is determined from the problem-family
/// classification, extracted features, method limitations
/// and available implementation capabilities.
///
/// A compatible method is not necessarily the best method.
/// The <see cref="Recommended"/> value is reserved for methods
/// that are both compatible and particularly appropriate for
/// the analyzed instance.
/// </remarks>
[Serializable]
[XmlType(TypeName = "methodCompatibilityLevel")]
public enum MethodCompatibilityLevel
{
    /// <summary>
    /// Compatibility has not yet been evaluated.
    /// </summary>
    [XmlEnum("notEvaluated")]
    NotEvaluated = 0,

    /// <summary>
    /// The method cannot solve the complete problem instance
    /// under its current assumptions.
    /// </summary>
    /// <remarks>
    /// Typical causes include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// an unsupported problem family;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// an unsupported product structure;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the presence of a mandatory feature excluded by the
    /// method;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// an instance size exceeding a hard method limit.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// The method may still be relevant for a relaxation or
    /// subproblem, but it is incompatible with the complete
    /// problem.
    /// </remarks>
    [XmlEnum("incompatible")]
    Incompatible = 1,

    /// <summary>
    /// The method is applicable only to part of the problem
    /// or requires adaptations, decomposition, relaxation or
    /// additional modeling work.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// solving independent single-item subproblems;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// applying the method after relaxing capacity
    /// constraints;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// using the method inside a decomposition procedure;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// ignoring or approximating unsupported extensions.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlEnum("partiallyCompatible")]
    PartiallyCompatible = 2,

    /// <summary>
    /// The method supports the complete problem instance and
    /// none of its hard assumptions are violated.
    /// </summary>
    /// <remarks>
    /// Compatibility does not imply that the method is the
    /// most efficient choice or that it can prove optimality.
    /// </remarks>
    [XmlEnum("compatible")]
    Compatible = 3,

    /// <summary>
    /// The method is compatible and is considered particularly
    /// appropriate for the analyzed instance.
    /// </summary>
    /// <remarks>
    /// Recommendation may depend on:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// an exact match with a supported problem family;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the absence of unsupported extensions;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// instance dimensions that fit the method's effective
    /// operating range;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the method's ability to exploit the detected product
    /// structure;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the requested balance between solution quality,
    /// computation time and optimality guarantees.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlEnum("recommended")]
    Recommended = 4
}