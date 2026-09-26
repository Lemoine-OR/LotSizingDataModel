using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a transport lane between two warehouses.
///
/// A transport lane has exactly one origin warehouse,
/// exactly one destination warehouse and a stable identifier.
///
/// Corresponds to the UML class "Liaison".
/// </summary>
[Serializable]
[XmlType(TypeName = "transportLane")]
public sealed partial class TransportLane : IdentifiedEntity
{
    private WarehouseReference _origin = new();
    private WarehouseReference _destination = new();

    /// <summary>
    /// Initializes an empty transport lane.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public TransportLane()
    {
    }

    /// <summary>
    /// Initializes a transport lane.
    /// </summary>
    /// <param name="origin">
    /// Reference to the origin warehouse.
    /// </param>
    /// <param name="destination">
    /// Reference to the destination warehouse.
    /// </param>
    /// <param name="id">Stable lane identifier.</param>
    public TransportLane(int id, WarehouseReference origin, WarehouseReference destination)
    {
        Id = id;
        Origin = origin;
        Destination = destination;
    }

    /// <summary>
    /// Gets or sets the origin warehouse.
    /// </summary>
    [XmlElement("origin")]
    public WarehouseReference Origin
    {
        get => _origin;
        set
        {
            // Validate that the origin warehouse reference is not null
            ArgumentNullException.ThrowIfNull(value);

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _origin,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the destination warehouse.
    /// </summary>
    [XmlElement("destination")]
    public WarehouseReference Destination
    {
        get => _destination;
        set
        {
            // Validate that the destination warehouse reference is not null
            ArgumentNullException.ThrowIfNull(value);

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _destination,
                value);
        }
    }

}
