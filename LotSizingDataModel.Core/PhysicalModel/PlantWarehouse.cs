using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a warehouse attached to a plant.
///
/// Corresponds to the UML class "Non Autonome",
/// derived from "Entrepot".
///
/// The owning plant will contain this warehouse.
/// No Plant reference is serialized here in order to avoid
/// circular XML object graphs.
/// </summary>
[Serializable]
[XmlType(TypeName = "plantWarehouse")]
public sealed partial class PlantWarehouse : Warehouse
{
    /// <summary>
    /// Initializes an empty plant warehouse.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public PlantWarehouse()
    {
    }

    /// <summary>
    /// Initializes a plant warehouse with a name.
    /// </summary>
    /// <param name="name">
    /// Warehouse name.
    /// </param>
    public PlantWarehouse(string name)
        : base(name)  // Initialize base warehouse with name
    {
    }
}