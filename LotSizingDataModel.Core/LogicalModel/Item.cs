using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.LogicalModel;

/// <summary>
/// Represents a finished or semi-finished item handled by the supply chain.
///
/// Corresponds to the UML class "Article".
/// </summary>
[Serializable]
[XmlType(TypeName = "item")]
public sealed class Item : IdentifiedEntity
{
    private int _billOfMaterialsLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="Item"/> class.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public Item()
    {
    }

    /// <summary>
    /// Initializes an item with its identifier, name
    /// and bill-of-materials level.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier corresponding to i in the source model.
    /// </param>
    /// <param name="name">
    /// Item name.
    /// </param>
    /// <param name="billOfMaterialsLevel">
    /// Bill-of-materials level corresponding to Γᵢ.
    /// </param>
    public Item(
        int id,
        string name,
        int billOfMaterialsLevel)
    {
        // Initialize all item properties
        Id = id;
        Name = name;
        BillOfMaterialsLevel = billOfMaterialsLevel;  // Validation occurs in the setter
    }

    /// <summary>
    /// Gets or sets the item's bill-of-materials level.
    ///
    /// Corresponds to Γᵢ in the source model.
    /// Level zero generally represents a finished product.
    /// Higher values represent lower levels in the bill of materials.
    /// </summary>
    [XmlAttribute("billOfMaterialsLevel")]
    public int BillOfMaterialsLevel
    {
        get => _billOfMaterialsLevel;
        set
        {
            // Validate that the level is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The bill-of-materials level cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _billOfMaterialsLevel,
                value);
        }
    }
}