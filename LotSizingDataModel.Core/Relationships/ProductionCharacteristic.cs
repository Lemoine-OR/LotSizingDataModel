using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents the production characteristics of an item
/// on a specific work center.
///
/// Corresponds to the UML association class
/// "Caracteristique Production" between Article and Poste de Charge.
///
/// Period-dependent capacity consumptions and costs will be added
/// through other partial declarations of this class.
/// </summary>
[Serializable]
[XmlType(TypeName = "productionCharacteristic")]
public sealed partial class ProductionCharacteristic : ModelObject
{
    private int _itemId;
    private WorkCenterReference _workCenter = new();

    /// <summary>
    /// Initializes an empty production characteristic.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public ProductionCharacteristic()
    {
    }

    /// <summary>
    /// Initializes a production characteristic.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the item being manufactured.
    /// </param>
    /// <param name="workCenter">
    /// Reference to the work center manufacturing the item.
    /// </param>
    public ProductionCharacteristic(
        int itemId,
        WorkCenterReference workCenter)
    {
        // Initialize production characteristic properties (validation occurs in setters)
        ItemId = itemId;
        WorkCenter = workCenter;
    }

    /// <summary>
    /// Initializes a production characteristic from identifiers.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the item being manufactured.
    /// </param>
    /// <param name="plantId">
    /// Identifier of the plant owning the work center.
    /// </param>
    /// <param name="workCenterId">
    /// Identifier of the work center inside the plant.
    /// </param>
    public ProductionCharacteristic(
        int itemId,
        int plantId,
        int workCenterId)
        : this(
            itemId,
            new WorkCenterReference(
                plantId,
                workCenterId))  // Create work center reference from IDs
    {
    }

    /// <summary>
    /// Gets or sets the identifier of the item manufactured
    /// on the referenced work center.
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
    /// Gets or sets the referenced work center.
    ///
    /// The reference contains both the plant identifier
    /// and the work-center identifier.
    /// </summary>
    [XmlElement("workCenter")]
    public WorkCenterReference WorkCenter
    {
        get => _workCenter;
        set
        {
            // Validate that the work center reference is not null
            ArgumentNullException.ThrowIfNull(value);

            // Update the backing field and notify property change if value differs
            SetProperty(ref _workCenter, value);
        }
    }

    /// <summary>
    /// Gets the identifier of the plant owning the work center.
    ///
    /// This convenience property is calculated and is not serialized.
    /// </summary>
    [XmlIgnore]
    public int PlantId => WorkCenter.PlantId;

    /// <summary>
    /// Gets the identifier of the referenced work center.
    ///
    /// This convenience property is calculated and is not serialized.
    /// </summary>
    [XmlIgnore]
    public int WorkCenterId => WorkCenter.WorkCenterId;

    /// <summary>
    /// Determines whether this object represents the same
    /// item/work-center association as another object.
    /// </summary>
    public bool RefersToSameAssociation(
        ProductionCharacteristic? other)
    {
        // Compare both item ID and work center reference for equality
        return other is not null
               && ItemId == other.ItemId
               && WorkCenter.RefersToSameWorkCenter(
                   other.WorkCenter);
    }

    /// <summary>
    /// Determines whether this production characteristic
    /// concerns a specified item.
    /// </summary>
    public bool ConcernsItem(int itemId)
    {
        return ItemId == itemId;
    }

    /// <summary>
    /// Determines whether this production characteristic
    /// concerns a specified work center.
    /// </summary>
    public bool ConcernsWorkCenter(
        int plantId,
        int workCenterId)
    {
        // Check if the work center reference matches both plant and work center IDs
        return WorkCenter.PlantId == plantId
               && WorkCenter.WorkCenterId == workCenterId;
    }
}