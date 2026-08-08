using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents the ability of a distribution center to source
/// a given item from a given warehouse inventory.
///
/// Corresponds to the UML association class "Achat"
/// between Stock and Centre de distribution.
///
/// The referenced inventory is identified by:
/// - the item identifier;
/// - the warehouse reference.
/// </summary>
[Serializable]
[XmlType(TypeName = "distributionCenterSourcing")]
public sealed partial class DistributionCenterSourcing : ModelObject
{
    private int _distributionCenterId;
    private int _itemId;
    private WarehouseReference _warehouse = new();

    /// <summary>
    /// Initializes an empty distribution-center sourcing relationship.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public DistributionCenterSourcing()
    {
    }

    /// <summary>
    /// Initializes a distribution-center sourcing relationship.
    /// </summary>
    /// <param name="distributionCenterId">
    /// Identifier of the distribution center.
    /// </param>
    /// <param name="itemId">
    /// Identifier of the item obtained from the inventory.
    /// </param>
    /// <param name="warehouse">
    /// Reference to the warehouse holding the inventory.
    /// </param>
    public DistributionCenterSourcing(
        int distributionCenterId,
        int itemId,
        WarehouseReference warehouse)
    {
        // Initialize sourcing properties (validation occurs in setters)
        DistributionCenterId = distributionCenterId;
        ItemId = itemId;
        Warehouse = warehouse;
    }

    /// <summary>
    /// Gets or sets the identifier of the distribution center
    /// that can obtain the item from the referenced inventory.
    /// </summary>
    [XmlAttribute("distributionCenterId")]
    public int DistributionCenterId
    {
        get => _distributionCenterId;
        set
        {
            // Validate that the distribution center identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The distribution-center identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _distributionCenterId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the item contained
    /// in the referenced inventory.
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
    /// Gets or sets the warehouse holding the inventory
    /// from which the distribution center can obtain the item.
    /// </summary>
    [XmlElement("warehouse")]
    public WarehouseReference Warehouse
    {
        get => _warehouse;
        set
        {
            // Ensure the warehouse reference is never null
            ArgumentNullException.ThrowIfNull(value);

            // Update and notify dependent properties if the reference changed
            if (SetProperty(ref _warehouse, value))
            {
                OnPropertyChanged(nameof(WarehouseKind));
                OnPropertyChanged(nameof(WarehouseReferenceId));
            }
        }
    }

    /// <summary>
    /// Gets the kind of referenced warehouse.
    ///
    /// This convenience property is calculated
    /// and is not serialized.
    /// </summary>
    [XmlIgnore]
    public WarehouseReferenceKind WarehouseKind =>
        Warehouse.Kind;

    /// <summary>
    /// Gets the identifier used to reference the warehouse.
    ///
    /// For a standalone warehouse, this is its own identifier.
    /// For a plant warehouse, this is the owning-plant identifier.
    /// </summary>
    [XmlIgnore]
    public int WarehouseReferenceId =>
        Warehouse.ReferenceId;

    /// <summary>
    /// Creates a sourcing relationship involving
    /// a standalone warehouse.
    /// </summary>
    public static DistributionCenterSourcing
        FromStandaloneWarehouse(
            int distributionCenterId,
            int itemId,
            int standaloneWarehouseId)
    {
        // Factory method that builds a warehouse reference for a standalone warehouse
        return new DistributionCenterSourcing(
            distributionCenterId,
            itemId,
            WarehouseReference.ForStandaloneWarehouse(
                standaloneWarehouseId));
    }

    /// <summary>
    /// Creates a sourcing relationship involving
    /// a warehouse attached to a plant.
    /// </summary>
    public static DistributionCenterSourcing
        FromPlantWarehouse(
            int distributionCenterId,
            int itemId,
            int plantId)
    {
        // Factory method that builds a warehouse reference for a plant warehouse
        return new DistributionCenterSourcing(
            distributionCenterId,
            itemId,
            WarehouseReference.ForPlantWarehouse(
                plantId));
    }

    /// <summary>
    /// Determines whether this object represents the same
    /// distribution-center/inventory relationship as another object.
    /// </summary>
    public bool RefersToSameSourcing(
        DistributionCenterSourcing? other)
    {
        // Compare distribution center, item, and warehouse reference for equality
        return other is not null
               && DistributionCenterId ==
                  other.DistributionCenterId
               && ItemId == other.ItemId
               && Warehouse.Kind == other.Warehouse.Kind
               && Warehouse.ReferenceId ==
                  other.Warehouse.ReferenceId;
    }

    /// <summary>
    /// Determines whether this relationship concerns
    /// the specified distribution center.
    /// </summary>
    public bool ConcernsDistributionCenter(
        int distributionCenterId)
    {
        // Check if the distribution center ID matches
        return DistributionCenterId ==
               distributionCenterId;
    }

    /// <summary>
    /// Determines whether this relationship concerns
    /// the specified item.
    /// </summary>
    public bool ConcernsItem(int itemId)
    {
        // Check if the item ID matches
        return ItemId == itemId;
    }

    /// <summary>
    /// Determines whether this relationship concerns
    /// the specified warehouse inventory.
    /// </summary>
    public bool ConcernsInventory(
        int itemId,
        WarehouseReference warehouse)
    {
        // Ensure the warehouse parameter is not null
        ArgumentNullException.ThrowIfNull(warehouse);

        // Check if both item and warehouse reference match
        return ItemId == itemId
               && Warehouse.Kind == warehouse.Kind
               && Warehouse.ReferenceId ==
                  warehouse.ReferenceId;
    }
}