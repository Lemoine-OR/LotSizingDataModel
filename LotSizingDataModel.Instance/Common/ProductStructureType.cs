using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Common;

/// <summary>
/// Identifies the structural category of a product
/// bill-of-materials graph.
/// </summary>
/// <remarks>
/// The classification assumes that each directed arc goes
/// from a component item to an immediate parent item that
/// consumes the component.
///
/// Cyclic structures are not represented by a specific value.
/// A cycle makes the product structure invalid and must be
/// reported separately by the product-structure analyzer.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productStructureType")]
public enum ProductStructureType
{
    /// <summary>
    /// The product-structure type is unknown or has not
    /// yet been analyzed.
    /// </summary>
    [XmlEnum("unknown")]
    Unknown,

    /// <summary>
    /// The instance contains no component-to-parent
    /// bill-of-materials relationship.
    /// </summary>
    /// <remarks>
    /// All items are structurally independent from a
    /// bill-of-materials perspective.
    /// </remarks>
    [XmlEnum("independentItems")]
    IndependentItems,

    /// <summary>
    /// Every connected component of the bill-of-materials
    /// graph is a directed chain.
    /// </summary>
    /// <remarks>
    /// Each item has at most one immediate component and
    /// is used by at most one immediate parent item.
    /// </remarks>
    [XmlEnum("serial")]
    Serial,

    /// <summary>
    /// The bill-of-materials graph has an assembly structure.
    /// </summary>
    /// <remarks>
    /// An item may consume several immediate components,
    /// but each component is used by at most one immediate
    /// parent item.
    /// </remarks>
    [XmlEnum("assembly")]
    Assembly,

    /// <summary>
    /// The bill-of-materials graph has an arborescent
    /// structure.
    /// </summary>
    /// <remarks>
    /// An item has at most one immediate component, but it
    /// may be used by several immediate parent items.
    /// </remarks>
    [XmlEnum("arborescent")]
    Arborescent,

    /// <summary>
    /// The acyclic bill-of-materials graph does not satisfy
    /// the serial, assembly or arborescent restrictions.
    /// </summary>
    /// <remarks>
    /// A general structure may contain both items consuming
    /// several components and components shared by several
    /// parent items.
    /// </remarks>
    [XmlEnum("general")]
    General
}