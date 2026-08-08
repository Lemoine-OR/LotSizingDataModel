using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents the inventory of an item in a warehouse.
///
/// This class is the association between an Item and a Warehouse.
/// It corresponds to the UML class "Stock".
///
/// Its initial-inventory property corresponds to I[i,0]
/// in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "inventory")]
public sealed partial class Inventory : ModelObject
{
    private int _itemId;
    private WarehouseReference _warehouse = new();
    private double _initialInventory;

    /// <summary>
    /// Initializes an empty inventory association.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public Inventory()
    {
    }

    /// <summary>
    /// Initializes an inventory association.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the item stored in the warehouse.
    /// </param>
    /// <param name="warehouse">
    /// Reference to the warehouse storing the item.
    /// </param>
    /// <param name="initialInventory">
    /// Initial inventory level I[i,0].
    /// </param>
    public Inventory(
        int itemId,
        WarehouseReference warehouse,
        double initialInventory = 0.0)
    {
        // Initialize all inventory properties (validation occurs in setters)
        ItemId = itemId;
        Warehouse = warehouse;
        InitialInventory = initialInventory;
    }

    /// <summary>
    /// Initializes an inventory association for an item
    /// stored in a standalone warehouse.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the stored item.
    /// </param>
    /// <param name="standaloneWarehouseId">
    /// Identifier e of the standalone warehouse.
    /// </param>
    /// <param name="initialInventory">
    /// Initial inventory level I[i,0].
    /// </param>
    public static Inventory ForStandaloneWarehouse(
        int itemId,
        int standaloneWarehouseId,
        double initialInventory = 0.0)
    {
        // Factory method: create inventory for a standalone warehouse
        return new Inventory(
            itemId,
            WarehouseReference.ForStandaloneWarehouse(
                standaloneWarehouseId),
            initialInventory);
    }

    /// <summary>
    /// Initializes an inventory association for an item
    /// stored in a warehouse attached to a plant.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the stored item.
    /// </param>
    /// <param name="plantId">
    /// Identifier u of the plant owning the warehouse.
    /// </param>
    /// <param name="initialInventory">
    /// Initial inventory level I[i,0].
    /// </param>
    public static Inventory ForPlantWarehouse(
        int itemId,
        int plantId,
        double initialInventory = 0.0)
    {
        // Factory method: create inventory for a plant warehouse
        return new Inventory(
            itemId,
            WarehouseReference.ForPlantWarehouse(plantId),
            initialInventory);
    }

    /// <summary>
    /// Gets or sets the identifier of the item
    /// stored in the warehouse.
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
    /// Gets or sets the warehouse storing the item.
    ///
    /// A plant warehouse is identified by its owning plant.
    /// A standalone warehouse is identified by its own identifier.
    /// </summary>
    [XmlElement("warehouse")]
    public WarehouseReference Warehouse
    {
        get => _warehouse;
        set
        {
            // Validate that the warehouse reference is not null
            ArgumentNullException.ThrowIfNull(value);

            // Update and notify dependent properties if value changed
            if (SetProperty(ref _warehouse, value))
            {
                OnPropertyChanged(nameof(WarehouseKind));
                OnPropertyChanged(nameof(WarehouseReferenceId));
            }
        }
    }

    /// <summary>
    /// Gets or sets the initial inventory level.
    ///
    /// Corresponds to I[i,0] in the source model.
    /// </summary>
    [XmlAttribute("initialInventory")]
    public double InitialInventory
    {
        get => _initialInventory;
        set
        {
            // Validate that the value is non-negative and finite
            ValidateNonNegativeFiniteValue(
                value,
                nameof(value));

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _initialInventory,
                value);
        }
    }

    /// <summary>
    /// Gets the kind of referenced warehouse.
    ///
    /// This is a convenience property and is not serialized.
    /// </summary>
    [XmlIgnore]
    public WarehouseReferenceKind WarehouseKind =>
        Warehouse.Kind;

    /// <summary>
    /// Gets the identifier used to reference the warehouse.
    ///
    /// This is either the standalone-warehouse identifier
    /// or the owning-plant identifier.
    /// </summary>
    [XmlIgnore]
    public int WarehouseReferenceId =>
        Warehouse.ReferenceId;

    /// <summary>
    /// Determines whether this object represents the same
    /// item/warehouse association as another inventory object.
    /// </summary>
    public bool RefersToSameInventory(
        Inventory? other)
    {
        // Compare item ID and warehouse reference (kind and ID) for equality
        return other is not null
               && ItemId == other.ItemId
               && Warehouse.Kind == other.Warehouse.Kind
               && Warehouse.ReferenceId ==
                  other.Warehouse.ReferenceId;
    }

    /// <summary>
    /// Determines whether this inventory concerns
    /// the specified item.
    /// </summary>
    public bool ConcernsItem(int itemId)
    {
        return ItemId == itemId;
    }

    /// <summary>
    /// Determines whether this inventory concerns
    /// the specified warehouse.
    /// </summary>
    public bool ConcernsWarehouse(
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        // Check if the warehouse reference matches both kind and ID
        return Warehouse.Kind == warehouse.Kind
               && Warehouse.ReferenceId ==
                  warehouse.ReferenceId;
    }

    /// <summary>
    /// Validates that a double value is finite and non-negative.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name for error messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the value is NaN, infinite, or negative.
    /// </exception>
    private static void ValidateNonNegativeFiniteValue(
        double value,
        string parameterName)
    {
        // Check if the value is finite (not NaN or Infinity)
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The initial inventory must be a finite number.");
        }

        // Check if the value is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The initial inventory cannot be negative.");
        }
    }
}