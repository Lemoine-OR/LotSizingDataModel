using System;
using System.Globalization;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.DellaertJeunet.XmlModel;

/// <summary>
/// Represents one component relationship declared inside an
/// item of a Dellaert–Jeunet XML benchmark instance.
/// </summary>
/// <remarks>
/// This class is a source-format data-transfer object.
///
/// In the source XML, a component relationship is represented
/// as follows:
/// <code>
/// &lt;Component&gt;
///   &lt;ID&gt;12&lt;/ID&gt;
///   &lt;Quantity&gt;1&lt;/Quantity&gt;
/// &lt;/Component&gt;
/// </code>
///
/// The parent item is not stored directly in this object. It
/// is determined by the <c>Item</c> element containing the
/// component.
///
/// Consequently, when item <c>1</c> contains component
/// <c>12</c> with quantity <c>1</c>, the relationship means:
/// <code>
/// Parent item:    1
/// Component item: 12
/// Quantity:       1
/// </code>
///
/// The direction of this relationship must not be reversed
/// during conversion to the domain model.
/// </remarks>
[Serializable]
[XmlType(
    TypeName = "Component",
    AnonymousType = true)]
public sealed class DellaertJeunetXmlComponent
{
    private int _itemId;

    private decimal _quantity;

    /// <summary>
    /// Initializes an empty source component relationship.
    /// </summary>
    /// <remarks>
    /// This public parameterless constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public DellaertJeunetXmlComponent()
    {
    }

    /// <summary>
    /// Initializes a source component relationship.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the component item.
    /// </param>
    /// <param name="quantity">
    /// Number of component units required to produce one unit
    /// of the parent item.
    /// </param>
    public DellaertJeunetXmlComponent(
        int itemId,
        decimal quantity)
    {
        ItemId =
            itemId;

        Quantity =
            quantity;
    }

    /// <summary>
    /// Gets or sets the identifier of the component item.
    /// </summary>
    /// <remarks>
    /// The source XML element is named <c>ID</c>, using
    /// uppercase letters. The explicit XML mapping must
    /// therefore be preserved.
    ///
    /// The identifier refers to another item declared in the
    /// instance-level <c>Items</c> collection.
    /// </remarks>
    [XmlElement(
        "ID",
        Order = 0)]
    public int ItemId
    {
        get =>
            _itemId;

        set =>
            _itemId =
                value;
    }

    /// <summary>
    /// Gets or sets the quantity of the component required for
    /// one unit of the parent item.
    /// </summary>
    /// <remarks>
    /// Although the provided Dellaert–Jeunet instance uses
    /// integer quantities equal to one, a decimal type is used
    /// here so that compatible format variants can represent
    /// non-integer coefficients without changing the public
    /// import API.
    ///
    /// Validation of strictly positive quantities is performed
    /// by the source validator.
    /// </remarks>
    [XmlElement(
        "Quantity",
        Order = 1)]
    public decimal Quantity
    {
        get =>
            _quantity;

        set =>
            _quantity =
                value;
    }

    /// <summary>
    /// Gets a value indicating whether a component item
    /// identifier has been supplied.
    /// </summary>
    [XmlIgnore]
    public bool HasItemId =>
        ItemId > 0;

    /// <summary>
    /// Gets a value indicating whether the component quantity
    /// is strictly positive.
    /// </summary>
    [XmlIgnore]
    public bool HasPositiveQuantity =>
        Quantity > 0m;

    /// <summary>
    /// Gets a value indicating whether the component quantity
    /// is equal to zero.
    /// </summary>
    [XmlIgnore]
    public bool HasZeroQuantity =>
        Quantity == 0m;

    /// <summary>
    /// Gets a value indicating whether the component quantity
    /// is negative.
    /// </summary>
    [XmlIgnore]
    public bool HasNegativeQuantity =>
        Quantity < 0m;

    /// <summary>
    /// Gets a value indicating whether the component quantity
    /// is an integer value.
    /// </summary>
    [XmlIgnore]
    public bool HasIntegerQuantity =>
        decimal.Truncate(
            Quantity) ==
        Quantity;

    /// <summary>
    /// Gets a value indicating whether the source component
    /// relationship is structurally valid.
    /// </summary>
    /// <remarks>
    /// A relationship is considered structurally valid when:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// the component item identifier is strictly positive;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the component quantity is strictly positive.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// This validation does not check whether the referenced
    /// item exists. Reference validation requires access to
    /// the complete source instance.
    /// </remarks>
    [XmlIgnore]
    public bool IsValid =>
        HasItemId &&
        HasPositiveQuantity;

    /// <summary>
    /// Determines whether the relationship references the
    /// supplied parent item itself.
    /// </summary>
    /// <param name="parentItemId">
    /// Identifier of the parent item containing this component
    /// relationship.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the parent item and
    /// component item have the same identifier; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool IsSelfReference(
        int parentItemId)
    {
        return
            parentItemId > 0 &&
            ItemId == parentItemId;
    }

    /// <summary>
    /// Determines whether this relationship references the
    /// supplied component item identifier.
    /// </summary>
    /// <param name="componentItemId">
    /// Component item identifier to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the identifiers match;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ReferencesItem(
        int componentItemId)
    {
        return
            ItemId ==
            componentItemId;
    }

    /// <summary>
    /// Builds a stable textual key for the relationship.
    /// </summary>
    /// <param name="parentItemId">
    /// Identifier of the parent item containing this component
    /// relationship.
    /// </param>
    /// <returns>
    /// Stable relationship key using the form
    /// <c>parentId-&gt;componentId</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="parentItemId"/> is not
    /// strictly positive.
    /// </exception>
    public string BuildRelationshipKey(
        int parentItemId)
    {
        if (parentItemId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parentItemId),
                parentItemId,
                "The parent item identifier must be " +
                "strictly positive.");
        }

        return
            parentItemId.ToString(
                CultureInfo.InvariantCulture) +
            "->" +
            ItemId.ToString(
                CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Validates the structural consistency of the component
    /// relationship.
    /// </summary>
    /// <returns>
    /// Empty string when the relationship is valid; otherwise,
    /// a human-readable validation message.
    /// </returns>
    public string Validate()
    {
        if (!HasItemId)
        {
            return
                "The component item identifier must be " +
                "strictly positive.";
        }

        if (!HasPositiveQuantity)
        {
            return
                "The component quantity must be strictly " +
                "positive.";
        }

        return string.Empty;
    }

    /// <summary>
    /// Validates the component relationship and throws an
    /// exception when it is structurally inconsistent.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the component item identifier or quantity
    /// is invalid.
    /// </exception>
    public void EnsureValid()
    {
        string validationMessage =
            Validate();

        if (string.IsNullOrWhiteSpace(
                validationMessage))
        {
            return;
        }

        throw new InvalidOperationException(
            validationMessage);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            "Component " +
            ItemId.ToString(
                CultureInfo.InvariantCulture) +
            " × " +
            Quantity.ToString(
                CultureInfo.InvariantCulture);
    }
}