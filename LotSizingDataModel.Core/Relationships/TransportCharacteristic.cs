using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents the transport characteristics of an item
/// for a specific transport resource.
///
/// Corresponds to the UML association class
/// "Caracteristique Transport" between Article
/// and Moyen de transport.
///
/// Period-dependent capacities and costs will be added
/// through another partial declaration of this class.
/// </summary>
[Serializable]
[XmlType(TypeName = "transportCharacteristic")]
public sealed partial class TransportCharacteristic : ModelObject
{
    private int _itemId;
    private int _transportResourceId;

    /// <summary>
    /// Initializes an empty transport characteristic.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public TransportCharacteristic()
    {
    }

    /// <summary>
    /// Initializes a transport characteristic.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the transported item.
    /// </param>
    /// <param name="transportResourceId">
    /// Identifier of the transport resource.
    /// </param>
    public TransportCharacteristic(
        int itemId,
        int transportResourceId)
    {
        // Initialize transport characteristic properties (validation occurs in setters)
        ItemId = itemId;
        TransportResourceId = transportResourceId;
    }

    /// <summary>
    /// Gets or sets the identifier of the item transported
    /// by the referenced transport resource.
    /// </summary>
    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set
        {
            // Validate that the item identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The item identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _itemId, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the transport resource
    /// associated with the item.
    /// </summary>
    [XmlAttribute("transportResourceId")]
    public int TransportResourceId
    {
        get => _transportResourceId;
        set
        {
            // Validate that the transport resource identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The transport-resource identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _transportResourceId,
                value);
        }
    }

    /// <summary>
    /// Determines whether this object represents the same
    /// item/transport-resource association as another object.
    /// </summary>
    public bool RefersToSameAssociation(
        TransportCharacteristic? other)
    {
        // Compare both item ID and transport resource ID for equality
        return other is not null
               && ItemId == other.ItemId
               && TransportResourceId ==
                  other.TransportResourceId;
    }

    /// <summary>
    /// Determines whether this transport characteristic
    /// concerns the specified item.
    /// </summary>
    public bool ConcernsItem(int itemId)
    {
        return ItemId == itemId;
    }

    /// <summary>
    /// Determines whether this transport characteristic
    /// concerns the specified transport resource.
    /// </summary>
    public bool ConcernsTransportResource(
        int transportResourceId)
    {
        // Check if the transport resource ID matches
        return TransportResourceId ==
               transportResourceId;
    }
}