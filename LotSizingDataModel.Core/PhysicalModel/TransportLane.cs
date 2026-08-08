using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a transport lane between two warehouses.
///
/// A transport lane has exactly one origin warehouse,
/// exactly one destination warehouse and a transport lead time.
///
/// Corresponds to the UML class "Liaison".
/// </summary>
[Serializable]
[XmlType(TypeName = "transportLane")]
public sealed partial class TransportLane : ModelObject
{
    private WarehouseReference _origin = new();
    private WarehouseReference _destination = new();
    private int _leadTime;

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
    /// <param name="leadTime">
    /// Number of planning periods required to complete
    /// transportation on this lane.
    /// </param>
    public TransportLane(
        WarehouseReference origin,
        WarehouseReference destination,
        int leadTime)
    {
        // Initialize all transport lane properties (validation occurs in setters)
        Origin = origin;
        Destination = destination;
        LeadTime = leadTime;
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

    /// <summary>
    /// Gets or sets the transport lead time.
    ///
    /// Corresponds to l[c] in the UML model.
    /// The value is expressed as a number of planning periods.
    /// </summary>
    [XmlAttribute("leadTime")]
    public int LeadTime
    {
        get => _leadTime;
        set
        {
            // Validate that the lead time is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The transport lead time cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _leadTime,
                value);
        }
    }
}