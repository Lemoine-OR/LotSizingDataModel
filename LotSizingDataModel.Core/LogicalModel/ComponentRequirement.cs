using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.LogicalModel;

/// <summary>
/// Represents a bill-of-materials relationship between two items.
///
/// A quantity of the component item j is required to manufacture
/// one unit of the parent item i.
///
/// Corresponds to A[j,i] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "componentRequirement")]
public sealed class ComponentRequirement : ModelObject
{
    private int _parentItemId;
    private int _componentItemId;
    private int _quantity;

    /// <summary>
    /// Initializes an empty component requirement.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public ComponentRequirement()
    {
    }

    /// <summary>
    /// Initializes a bill-of-materials relationship.
    /// </summary>
    /// <param name="parentItemId">
    /// Identifier of the item being manufactured, corresponding to i.
    /// </param>
    /// <param name="componentItemId">
    /// Identifier of the required component, corresponding to j.
    /// </param>
    /// <param name="quantity">
    /// Quantity of component j required to manufacture one unit
    /// of parent item i, corresponding to A[j,i].
    /// </param>
    public ComponentRequirement(
        int parentItemId,
        int componentItemId,
        int quantity)
    {
        // Initialize all properties (validation occurs in setters)
        ParentItemId = parentItemId;
        ComponentItemId = componentItemId;
        Quantity = quantity;
    }

    /// <summary>
    /// Gets or sets the identifier of the item being manufactured.
    ///
    /// Corresponds to index i in A[j,i].
    /// </summary>
    [XmlAttribute("parentItemId")]
    public int ParentItemId
    {
        get => _parentItemId;
        set
        {
            // Validate that the identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The parent item identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _parentItemId, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the required component.
    ///
    /// Corresponds to index j in A[j,i].
    /// </summary>
    [XmlAttribute("componentItemId")]
    public int ComponentItemId
    {
        get => _componentItemId;
        set
        {
            // Validate that the identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The component item identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _componentItemId, value);
        }
    }

    /// <summary>
    /// Gets or sets the quantity of the component required
    /// to manufacture one unit of the parent item.
    ///
    /// Corresponds to A[j,i] in the source model.
    /// </summary>
    [XmlAttribute("quantity")]
    public int Quantity
    {
        get => _quantity;
        set
        {
            // Validate that the quantity is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The component quantity cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _quantity, value);
        }
    }
}