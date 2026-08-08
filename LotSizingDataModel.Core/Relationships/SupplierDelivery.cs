using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents the ability of a supplier to deliver a given item
/// to a given warehouse inventory.
///
/// Corresponds to the UML association class "Livraison"
/// between Stock and Fournisseur.
///
/// The referenced inventory is identified by:
/// - the item identifier;
/// - the warehouse reference.
/// </summary>
[Serializable]
[XmlType(TypeName = "supplierDelivery")]
public sealed partial class SupplierDelivery : ModelObject
{
    private int _supplierId;
    private int _itemId;
    private WarehouseReference _warehouse = new();
    private int _leadTime;

    /// <summary>
    /// Initializes an empty supplier-delivery relationship.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public SupplierDelivery()
    {
    }

    /// <summary>
    /// Initializes a supplier-delivery relationship.
    /// </summary>
    /// <param name="supplierId">
    /// Identifier of the supplier.
    /// </param>
    /// <param name="itemId">
    /// Identifier of the delivered item.
    /// </param>
    /// <param name="warehouse">
    /// Reference to the warehouse receiving the item.
    /// </param>
    /// <param name="leadTime">
    /// Delivery lead time, expressed in planning periods.
    /// Corresponds to l[f] in the source model.
    /// </param>
    public SupplierDelivery(
        int supplierId,
        int itemId,
        WarehouseReference warehouse,
        int leadTime)
    {
        // Initialize delivery properties (validation occurs in setters)
        SupplierId = supplierId;
        ItemId = itemId;
        Warehouse = warehouse;
        LeadTime = leadTime;
    }

    /// <summary>
    /// Gets or sets the identifier of the supplier.
    ///
    /// Corresponds to index f in the source model.
    /// </summary>
    [XmlAttribute("supplierId")]
    public int SupplierId
    {
        get => _supplierId;
        set
        {
            // Validate that the supplier identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The supplier identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _supplierId, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the delivered item.
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
    /// Gets or sets the warehouse receiving the item.
    ///
    /// For a plant warehouse, the warehouse is referenced through
    /// the identifier of its owning plant.
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
    /// Gets or sets the supplier-delivery lead time.
    ///
    /// Corresponds to l[f] in the UML model.
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
                    "The supplier-delivery lead time cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _leadTime, value);
        }
    }

    /// <summary>
    /// Gets the kind of referenced warehouse.
    ///
    /// This calculated property is not serialized.
    /// </summary>
    [XmlIgnore]
    public WarehouseReferenceKind WarehouseKind =>
        Warehouse.Kind;

    /// <summary>
    /// Gets the identifier used to reference the warehouse.
    ///
    /// This calculated property is not serialized.
    /// </summary>
    [XmlIgnore]
    public int WarehouseReferenceId =>
        Warehouse.ReferenceId;

    /// <summary>
    /// Creates a delivery relationship involving
    /// a standalone warehouse.
    /// </summary>
    public static SupplierDelivery ToStandaloneWarehouse(
        int supplierId,
        int itemId,
        int standaloneWarehouseId,
        int leadTime)
    {
        // Factory method that builds a warehouse reference for a standalone warehouse
        return new SupplierDelivery(
            supplierId,
            itemId,
            WarehouseReference.ForStandaloneWarehouse(
                standaloneWarehouseId),
            leadTime);
    }

    /// <summary>
    /// Creates a delivery relationship involving
    /// a warehouse attached to a plant.
    /// </summary>
    public static SupplierDelivery ToPlantWarehouse(
        int supplierId,
        int itemId,
        int plantId,
        int leadTime)
    {
        // Factory method that builds a warehouse reference for a plant warehouse
        return new SupplierDelivery(
            supplierId,
            itemId,
            WarehouseReference.ForPlantWarehouse(
                plantId),
            leadTime);
    }

    /// <summary>
    /// Determines whether this object represents the same
    /// supplier/inventory relationship as another object.
    /// </summary>
    public bool RefersToSameDelivery(
        SupplierDelivery? other)
    {
        // Compare supplier, item, and warehouse reference for equality
        return other is not null
               && SupplierId == other.SupplierId
               && ItemId == other.ItemId
               && Warehouse.Kind == other.Warehouse.Kind
               && Warehouse.ReferenceId ==
                  other.Warehouse.ReferenceId;
    }

    /// <summary>
    /// Determines whether this delivery concerns
    /// the specified supplier.
    /// </summary>
    public bool ConcernsSupplier(int supplierId)
    {
        // Check if the supplier ID matches
        return SupplierId == supplierId;
    }

    /// <summary>
    /// Determines whether this delivery concerns
    /// the specified item.
    /// </summary>
    public bool ConcernsItem(int itemId)
    {
        // Check if the item ID matches
        return ItemId == itemId;
    }

    /// <summary>
    /// Determines whether this delivery concerns
    /// the specified inventory.
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