using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Common;

/// <summary>
/// Identifies the part of a lot-sizing instance to which
/// a known-problem-family match applies.
/// </summary>
/// <remarks>
/// The scope is independent of the nature of the match.
///
/// For example, a known family may exactly describe the complete
/// problem, describe a relaxation of it, or occur as a subproblem
/// associated with one item, facility or resource.
/// </remarks>
[Serializable]
[XmlType(TypeName = "problemClassificationScope")]
public enum ProblemClassificationScope
{
    /// <summary>
    /// The scope of the classification match is unknown
    /// or has not been specified.
    /// </summary>
    [XmlEnum("unknown")]
    Unknown,

    /// <summary>
    /// The classification match applies to the complete
    /// lot-sizing problem represented by the instance.
    /// </summary>
    [XmlEnum("completeProblem")]
    CompleteProblem,

    /// <summary>
    /// The classification match applies to a relaxation
    /// of the complete problem.
    /// </summary>
    /// <remarks>
    /// The relaxation may result from removing capacity,
    /// coupling, integrality, transportation or other
    /// constraints.
    /// </remarks>
    [XmlEnum("problemRelaxation")]
    ProblemRelaxation,

    /// <summary>
    /// The classification match applies to a subproblem
    /// associated with one item.
    /// </summary>
    /// <remarks>
    /// The corresponding item identifier should be recorded
    /// in the classification-match object.
    /// </remarks>
    [XmlEnum("singleItem")]
    SingleItem,

    /// <summary>
    /// The classification match applies to a subproblem
    /// associated with a subset of items.
    /// </summary>
    [XmlEnum("itemSubset")]
    ItemSubset,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one production plant.
    /// </summary>
    [XmlEnum("plant")]
    Plant,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one work center.
    /// </summary>
    [XmlEnum("workCenter")]
    WorkCenter,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one warehouse.
    /// </summary>
    [XmlEnum("warehouse")]
    Warehouse,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one transport resource.
    /// </summary>
    [XmlEnum("transportResource")]
    TransportResource,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one directed transport lane.
    /// </summary>
    [XmlEnum("transportLane")]
    TransportLane,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one supplier.
    /// </summary>
    [XmlEnum("supplier")]
    Supplier,

    /// <summary>
    /// The classification match applies to the decisions
    /// associated with one distribution center.
    /// </summary>
    [XmlEnum("distributionCenter")]
    DistributionCenter,

    /// <summary>
    /// The classification match applies to a connected
    /// segment or subsystem of the supply chain.
    /// </summary>
    /// <remarks>
    /// Examples include one product family, one echelon,
    /// one production-distribution branch or one connected
    /// component of the product-structure graph.
    /// </remarks>
    [XmlEnum("supplyChainSegment")]
    SupplyChainSegment,

    /// <summary>
    /// The classification match applies to a custom subset
    /// that cannot be represented by another scope value.
    /// </summary>
    /// <remarks>
    /// The affected entities should be described explicitly
    /// in the classification-match object.
    /// </remarks>
    [XmlEnum("customSubset")]
    CustomSubset
}