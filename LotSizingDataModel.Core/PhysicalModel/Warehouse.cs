using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a warehouse in the supply chain.
///
/// A warehouse is either:
/// - a standalone warehouse;
/// - or a warehouse attached to a plant.
///
/// Corresponds to the UML class "Entrepot".
/// </summary>
[Serializable]
[XmlType(TypeName = "warehouse")]
public abstract partial class Warehouse : NamedEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Warehouse"/> class.
    ///
    /// This constructor is protected because the abstract Warehouse class
    /// cannot be instantiated directly.
    /// </summary>
    protected Warehouse()
    {
    }

    /// <summary>
    /// Initializes a warehouse with a name.
    /// </summary>
    /// <param name="name">
    /// Warehouse name.
    /// </param>
    protected Warehouse(string name)
    {
        // Initialize warehouse name property
        Name = name;
    }
}