using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Identifies the type of warehouse referenced by a relationship.
/// </summary>
[Serializable]
[XmlType(TypeName = "warehouseReferenceKind")]
public enum WarehouseReferenceKind
{
    /// <summary>
    /// The reference identifies a standalone warehouse
    /// through its own warehouse identifier e.
    /// </summary>
    [XmlEnum("standaloneWarehouse")]
    StandaloneWarehouse = 0,

    /// <summary>
    /// The reference identifies a plant warehouse
    /// through the identifier u of its owning plant.
    /// </summary>
    [XmlEnum("plantWarehouse")]
    PlantWarehouse = 1
}

/// <summary>
/// Represents a serializable reference to a warehouse.
///
/// A standalone warehouse is referenced by its warehouse identifier.
/// A plant warehouse is referenced by the identifier of its owning plant.
///
/// This class avoids serializing complete Warehouse objects inside
/// relationships and therefore prevents object duplication and XML cycles.
/// </summary>
[Serializable]
[XmlType(TypeName = "warehouseReference")]
public sealed class WarehouseReference : ModelObject
{
    private WarehouseReferenceKind _kind;
    private int _referenceId;

    /// <summary>
    /// Initializes an empty warehouse reference.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public WarehouseReference()
    {
    }

    /// <summary>
    /// Initializes a warehouse reference.
    /// </summary>
    /// <param name="kind">
    /// Kind of warehouse being referenced.
    /// </param>
    /// <param name="referenceId">
    /// Standalone-warehouse identifier or owning-plant identifier,
    /// depending on <paramref name="kind"/>.
    /// </param>
    public WarehouseReference(
        WarehouseReferenceKind kind,
        int referenceId)
    {
        // Initialize warehouse reference properties (validation occurs in setters)
        Kind = kind;
        ReferenceId = referenceId;
    }

    /// <summary>
    /// Gets or sets the kind of warehouse being referenced.
    /// </summary>
    [XmlAttribute("kind")]
    public WarehouseReferenceKind Kind
    {
        get => _kind;
        set
        {
            // Validate that the enum value is defined
            if (!Enum.IsDefined(typeof(WarehouseReferenceKind), value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The warehouse reference kind is not valid.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _kind, value);
        }
    }

    /// <summary>
    /// Gets or sets the referenced identifier.
    ///
    /// For a standalone warehouse, this is its warehouse identifier e.
    /// For a plant warehouse, this is the identifier u of its owning plant.
    /// </summary>
    [XmlAttribute("id")]
    public int ReferenceId
    {
        get => _referenceId;
        set
        {
            // Validate that the identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The warehouse reference identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _referenceId, value);
        }
    }

    /// <summary>
    /// Creates a reference to a standalone warehouse.
    /// </summary>
    /// <param name="warehouseId">
    /// Identifier e of the standalone warehouse.
    /// </param>
    public static WarehouseReference ForStandaloneWarehouse(
        int warehouseId)
    {
        // Factory method: create a reference to a standalone warehouse
        return new WarehouseReference(
            WarehouseReferenceKind.StandaloneWarehouse,
            warehouseId);
    }

    /// <summary>
    /// Creates a reference to the warehouse attached to a plant.
    /// </summary>
    /// <param name="plantId">
    /// Identifier u of the plant owning the warehouse.
    /// </param>
    public static WarehouseReference ForPlantWarehouse(
        int plantId)
    {
        // Factory method: create a reference to a plant's warehouse
        return new WarehouseReference(
            WarehouseReferenceKind.PlantWarehouse,
            plantId);
    }

    /// <summary>
    /// Returns a readable representation of the warehouse reference.
    /// </summary>
    public override string ToString()
    {
        return $"{Kind}:{ReferenceId}";
    }
}