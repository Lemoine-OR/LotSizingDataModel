using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.DellaertJeunet.XmlModel;

/// <summary>
/// Represents the root object of a Dellaert–Jeunet XML
/// benchmark instance.
/// </summary>
/// <remarks>
/// This class reproduces the complete source XML structure
/// before any conversion into the LotSizingDataModel domain
/// model.
///
/// A typical document has the following structure:
/// <code>
/// &lt;Instance&gt;
///   &lt;Article&gt;...&lt;/Article&gt;
///   &lt;ID&gt;40&lt;/ID&gt;
///   &lt;Name&gt;ph3in40st1de1mh0ms0&lt;/Name&gt;
///   &lt;InstanceType&gt;Large&lt;/InstanceType&gt;
///   &lt;BOMType&gt;General&lt;/BOMType&gt;
///   &lt;NBPeriods&gt;52&lt;/NBPeriods&gt;
///   &lt;Items&gt;
///     &lt;Item&gt;...&lt;/Item&gt;
///   &lt;/Items&gt;
/// &lt;/Instance&gt;
/// </code>
///
/// This type is deliberately independent from
/// <c>LotSizingDataModel.Core</c> and
/// <c>LotSizingDataModel.Instance</c>.
///
/// Source validation and conversion are performed by
/// dedicated importer services.
/// </remarks>
[Serializable]
[XmlRoot(
    "Instance",
    Namespace = "",
    IsNullable = false)]
[XmlType(
    TypeName = "Instance",
    AnonymousType = true)]
public sealed class DellaertJeunetXmlInstance
{
    private DellaertJeunetXmlArticle? _article;

    private int _id;

    private string _name =
        string.Empty;

    private string _instanceType =
        string.Empty;

    private string _bomType =
        string.Empty;

    private int _numberOfPeriods;

    private List<DellaertJeunetXmlItem> _items =
        new();

    /// <summary>
    /// Initializes an empty Dellaert–Jeunet XML instance.
    /// </summary>
    /// <remarks>
    /// This public parameterless constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public DellaertJeunetXmlInstance()
    {
    }

    /// <summary>
    /// Initializes a Dellaert–Jeunet XML instance.
    /// </summary>
    /// <param name="id">
    /// Source instance identifier.
    /// </param>
    /// <param name="name">
    /// Source instance name.
    /// </param>
    /// <param name="instanceType">
    /// Declared instance-size category.
    /// </param>
    /// <param name="bomType">
    /// Declared bill-of-material type.
    /// </param>
    /// <param name="numberOfPeriods">
    /// Number of planning periods.
    /// </param>
    /// <param name="items">
    /// Source items.
    /// </param>
    /// <param name="article">
    /// Optional bibliographic article metadata.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="items"/> is
    /// <see langword="null"/>.
    /// </exception>
    public DellaertJeunetXmlInstance(
        int id,
        string name,
        string instanceType,
        string bomType,
        int numberOfPeriods,
        IEnumerable<DellaertJeunetXmlItem> items,
        DellaertJeunetXmlArticle? article = null)
    {
        ArgumentNullException.ThrowIfNull(
            items);

        Id =
            id;

        Name =
            name;

        InstanceType =
            instanceType;

        BomType =
            bomType;

        NumberOfPeriods =
            numberOfPeriods;

        Items =
            items.ToList();

        Article =
            article;
    }

    /// <summary>
    /// Gets or sets the optional bibliographic article
    /// metadata associated with the benchmark instance.
    /// </summary>
    [XmlElement(
        "Article",
        Order = 0,
        IsNullable = false)]
    public DellaertJeunetXmlArticle? Article
    {
        get =>
            _article;

        set =>
            _article =
                value;
    }

    /// <summary>
    /// Gets or sets the source instance identifier.
    /// </summary>
    /// <remarks>
    /// The XML element is named <c>ID</c>, with uppercase
    /// letters.
    /// </remarks>
    [XmlElement(
        "ID",
        Order = 1)]
    public int Id
    {
        get =>
            _id;

        set =>
            _id =
                value;
    }

    /// <summary>
    /// Gets or sets the source instance name.
    /// </summary>
    [XmlElement(
        "Name",
        Order = 2)]
    public string Name
    {
        get =>
            _name;

        set =>
            _name =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the declared instance-size category.
    /// </summary>
    /// <remarks>
    /// Typical values include:
    /// <list type="bullet">
    /// <item>
    /// <description><c>Small</c>;</description>
    /// </item>
    /// <item>
    /// <description><c>Medium</c>;</description>
    /// </item>
    /// <item>
    /// <description><c>Large</c>.</description>
    /// </item>
    /// </list>
    ///
    /// The source value is preserved as text because future
    /// benchmark variants may use additional categories.
    /// </remarks>
    [XmlElement(
        "InstanceType",
        Order = 3)]
    public string InstanceType
    {
        get =>
            _instanceType;

        set =>
            _instanceType =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the bill-of-material type declared by the
    /// source instance.
    /// </summary>
    /// <remarks>
    /// The supplied instance declares the value
    /// <c>General</c>.
    ///
    /// The declared value must later be compared with the
    /// structure detected from the complete product graph.
    /// </remarks>
    [XmlElement(
        "BOMType",
        Order = 4)]
    public string BomType
    {
        get =>
            _bomType;

        set =>
            _bomType =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the number of planning periods.
    /// </summary>
    /// <remarks>
    /// The corresponding source XML element is named
    /// <c>NBPeriods</c>.
    /// </remarks>
    [XmlElement(
        "NBPeriods",
        Order = 5)]
    public int NumberOfPeriods
    {
        get =>
            _numberOfPeriods;

        set =>
            _numberOfPeriods =
                value;
    }

    /// <summary>
    /// Gets or sets the source item collection.
    /// </summary>
    /// <remarks>
    /// The outer XML element is named <c>Items</c>, and every
    /// contained element is named <c>Item</c>.
    /// </remarks>
    [XmlArray(
        "Items",
        Order = 6)]
    [XmlArrayItem(
        "Item",
        typeof(DellaertJeunetXmlItem),
        IsNullable = false)]
    public List<DellaertJeunetXmlItem> Items
    {
        get =>
            _items;

        set =>
            _items =
                value ??
                new List<DellaertJeunetXmlItem>();
    }

    /// <summary>
    /// Gets a value indicating whether bibliographic metadata
    /// is available.
    /// </summary>
    [XmlIgnore]
    public bool HasArticle =>
        Article is not null;

    /// <summary>
    /// Gets a value indicating whether the source instance
    /// identifier is strictly positive.
    /// </summary>
    [XmlIgnore]
    public bool HasValidId =>
        Id > 0;

    /// <summary>
    /// Gets a value indicating whether the source instance has
    /// a non-empty name.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(
            Name);

    /// <summary>
    /// Gets a value indicating whether an instance-size
    /// category is available.
    /// </summary>
    [XmlIgnore]
    public bool HasInstanceType =>
        !string.IsNullOrWhiteSpace(
            InstanceType);

    /// <summary>
    /// Gets a value indicating whether a declared
    /// bill-of-material type is available.
    /// </summary>
    [XmlIgnore]
    public bool HasBomType =>
        !string.IsNullOrWhiteSpace(
            BomType);

    /// <summary>
    /// Gets a value indicating whether the planning horizon is
    /// strictly positive.
    /// </summary>
    [XmlIgnore]
    public bool HasValidPlanningHorizon =>
        NumberOfPeriods > 0;

    /// <summary>
    /// Gets a value indicating whether at least one source
    /// item exists.
    /// </summary>
    [XmlIgnore]
    public bool HasItems =>
        Items.Count > 0;

    /// <summary>
    /// Gets the number of source items.
    /// </summary>
    [XmlIgnore]
    public int ItemCount =>
        Items.Count;

    /// <summary>
    /// Gets the total number of component relationships stored
    /// in the source document.
    /// </summary>
    [XmlIgnore]
    public int ComponentRelationshipCount =>
        Items
            .Where(
                item =>
                    item is not null)
            .Sum(
                item =>
                    item.ComponentCount);

    /// <summary>
    /// Gets the number of items carrying at least one positive
    /// external-demand value.
    /// </summary>
    [XmlIgnore]
    public int ExternallyDemandedItemCount =>
        Items.Count(
            item =>
                item is not null &&
                item.HasPositiveExternalDemand);

    /// <summary>
    /// Gets the number of source leaf items.
    /// </summary>
    [XmlIgnore]
    public int LeafItemCount =>
        Items.Count(
            item =>
                item is not null &&
                item.IsLeaf);

    /// <summary>
    /// Gets the maximum bill-of-material depth declared by the
    /// source items.
    /// </summary>
    [XmlIgnore]
    public int MaximumDeclaredDepth =>
        Items
            .Where(
                item =>
                    item is not null)
            .Select(
                item =>
                    item.DepthInBom)
            .DefaultIfEmpty(0)
            .Max();

    /// <summary>
    /// Gets the distinct source item identifiers in ascending
    /// order.
    /// </summary>
    [XmlIgnore]
    public IReadOnlyList<int> DistinctItemIds =>
        Items
            .Where(
                item =>
                    item is not null)
            .Select(
                item =>
                    item.Id)
            .Distinct()
            .OrderBy(
                itemId =>
                    itemId)
            .ToArray();

    /// <summary>
    /// Gets a value indicating whether the source contains a
    /// null item entry.
    /// </summary>
    [XmlIgnore]
    public bool HasNullItem =>
        Items.Any(
            item =>
                item is null);

    /// <summary>
    /// Gets a value indicating whether at least two source
    /// items share the same identifier.
    /// </summary>
    [XmlIgnore]
    public bool HasDuplicateItemIdentifiers =>
        Items
            .Where(
                item =>
                    item is not null)
            .GroupBy(
                item =>
                    item.Id)
            .Any(
                group =>
                    group.Count() > 1);

    /// <summary>
    /// Gets a value indicating whether all non-null source
    /// items have identifiers forming a contiguous sequence.
    /// </summary>
    /// <remarks>
    /// The sequence may begin with any positive identifier.
    /// For the supplied Dellaert–Jeunet file, the identifiers
    /// form the sequence from 1 to 500.
    /// </remarks>
    [XmlIgnore]
    public bool HasContiguousItemIdentifiers
    {
        get
        {
            int[] identifiers =
                DistinctItemIds.ToArray();

            if (identifiers.Length == 0)
            {
                return false;
            }

            if (identifiers[0] <= 0)
            {
                return false;
            }

            for (int index = 1;
                 index < identifiers.Length;
                 index++)
            {
                if (identifiers[index] !=
                    identifiers[index - 1] + 1)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Gets a value indicating whether source items are
    /// physically ordered by ascending identifier.
    /// </summary>
    [XmlIgnore]
    public bool AreItemsOrderedByIdentifier
    {
        get
        {
            int? previousIdentifier =
                null;

            foreach (DellaertJeunetXmlItem? item
                     in Items)
            {
                if (item is null)
                {
                    continue;
                }

                if (previousIdentifier.HasValue &&
                    item.Id <
                    previousIdentifier.Value)
                {
                    return false;
                }

                previousIdentifier =
                    item.Id;
            }

            return true;
        }
    }

    /// <summary>
    /// Gets a value indicating whether at least one source item
    /// has a demand-series length incompatible with the
    /// planning horizon.
    /// </summary>
    /// <remarks>
    /// Empty demand collections are accepted by this property,
    /// because their conversion depends on the selected import
    /// options.
    /// </remarks>
    [XmlIgnore]
    public bool HasInvalidDemandLength
    {
        get
        {
            if (!HasValidPlanningHorizon)
            {
                return true;
            }

            return Items.Any(
                item =>
                    item is null ||
                    !item.HasCompatibleDemandLength(
                        NumberOfPeriods,
                        allowEmptyDemand:
                            true));
        }
    }

    /// <summary>
    /// Gets a value indicating whether at least one source item
    /// or component relationship references an invalid
    /// identifier.
    /// </summary>
    [XmlIgnore]
    public bool HasMissingComponentReference
    {
        get
        {
            HashSet<int> itemIdentifiers =
                Items
                    .Where(
                        item =>
                            item is not null)
                    .Select(
                        item =>
                            item.Id)
                    .ToHashSet();

            return Items
                .Where(
                    item =>
                        item is not null)
                .SelectMany(
                    item =>
                        item.Components)
                .Any(
                    component =>
                        component is null ||
                        !itemIdentifiers.Contains(
                            component.ItemId));
        }
    }

    /// <summary>
    /// Gets a value indicating whether at least one source item
    /// is structurally invalid.
    /// </summary>
    [XmlIgnore]
    public bool HasStructurallyInvalidItem =>
        Items.Any(
            item =>
                item is null ||
                !item.IsStructurallyValid);

    /// <summary>
    /// Gets a value indicating whether the root source object
    /// is structurally valid without performing graph-level
    /// cycle analysis.
    /// </summary>
    [XmlIgnore]
    public bool IsStructurallyValid =>
        HasValidId &&
        HasName &&
        HasValidPlanningHorizon &&
        HasItems &&
        !HasNullItem &&
        !HasDuplicateItemIdentifiers &&
        !HasStructurallyInvalidItem &&
        !HasInvalidDemandLength &&
        !HasMissingComponentReference;

    /// <summary>
    /// Returns the source item having the supplied identifier.
    /// </summary>
    /// <param name="itemId">
    /// Source item identifier.
    /// </param>
    /// <returns>
    /// Matching source item, or <see langword="null"/> when no
    /// item has the supplied identifier.
    /// </returns>
    public DellaertJeunetXmlItem? FindItem(
        int itemId)
    {
        return Items
            .FirstOrDefault(
                item =>
                    item is not null &&
                    item.Id == itemId);
    }

    /// <summary>
    /// Determines whether the source instance contains an item
    /// having the supplied identifier.
    /// </summary>
    /// <param name="itemId">
    /// Source item identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the identifier exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsItem(
        int itemId)
    {
        return
            FindItem(itemId) is not null;
    }

    /// <summary>
    /// Returns all source items declared at the supplied
    /// bill-of-material depth.
    /// </summary>
    /// <param name="depth">
    /// Declared bill-of-material depth.
    /// </param>
    /// <returns>
    /// Matching source items.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="depth"/> is not strictly
    /// positive.
    /// </exception>
    public IReadOnlyList<DellaertJeunetXmlItem>
        GetItemsAtDepth(
            int depth)
    {
        if (depth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(depth),
                depth,
                "The bill-of-material depth must be " +
                "strictly positive.");
        }

        return Items
            .Where(
                item =>
                    item is not null &&
                    item.DepthInBom == depth)
            .OrderBy(
                item =>
                    item.Id)
            .ToArray();
    }

    /// <summary>
    /// Returns all source items carrying a positive external
    /// demand.
    /// </summary>
    /// <returns>
    /// Externally demanded source items.
    /// </returns>
    public IReadOnlyList<DellaertJeunetXmlItem>
        GetExternallyDemandedItems()
    {
        return Items
            .Where(
                item =>
                    item is not null &&
                    item.HasPositiveExternalDemand)
            .OrderBy(
                item =>
                    item.Id)
            .ToArray();
    }

    /// <summary>
    /// Returns all source leaf items.
    /// </summary>
    /// <returns>
    /// Source items without declared components.
    /// </returns>
    public IReadOnlyList<DellaertJeunetXmlItem>
        GetLeafItems()
    {
        return Items
            .Where(
                item =>
                    item is not null &&
                    item.IsLeaf)
            .OrderBy(
                item =>
                    item.Id)
            .ToArray();
    }

    /// <summary>
    /// Adds a source item to the instance.
    /// </summary>
    /// <param name="item">
    /// Source item to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddItem(
        DellaertJeunetXmlItem item)
    {
        ArgumentNullException.ThrowIfNull(
            item);

        Items.Add(
            item);
    }

    /// <summary>
    /// Removes all source items from the instance.
    /// </summary>
    public void ClearItems()
    {
        Items.Clear();
    }

    /// <summary>
    /// Validates the root XML object without performing
    /// complete graph-level analysis.
    /// </summary>
    /// <returns>
    /// Ordered collection of validation messages.
    /// </returns>
    public IReadOnlyList<string> ValidateStructure()
    {
        var errors =
            new List<string>();

        if (!HasValidId)
        {
            errors.Add(
                "The instance identifier must be strictly " +
                "positive.");
        }

        if (!HasName)
        {
            errors.Add(
                "The instance name is required.");
        }

        if (!HasValidPlanningHorizon)
        {
            errors.Add(
                "The number of planning periods must be " +
                "strictly positive.");
        }

        if (!HasItems)
        {
            errors.Add(
                "At least one source item is required.");
        }

        if (HasNullItem)
        {
            errors.Add(
                "The source item collection cannot contain a " +
                "null entry.");
        }

        if (HasDuplicateItemIdentifiers)
        {
            errors.Add(
                "Source item identifiers must be unique.");
        }

        if (HasStructurallyInvalidItem)
        {
            errors.Add(
                "At least one source item is structurally " +
                "invalid.");
        }

        if (HasInvalidDemandLength)
        {
            errors.Add(
                "At least one external-demand series has a " +
                "length incompatible with the planning " +
                "horizon.");
        }

        if (HasMissingComponentReference)
        {
            errors.Add(
                "At least one component relationship " +
                "references an item that does not exist.");
        }

        return errors
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                error =>
                    error,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates the root XML object and throws an exception
    /// when it is structurally inconsistent.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when at least one structural validation error
    /// exists.
    /// </exception>
    public void EnsureStructurallyValid()
    {
        IReadOnlyList<string> errors =
            ValidateStructure();

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "The Dellaert–Jeunet source instance is invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        "- " + error)));
    }

    /// <summary>
    /// Builds a compact summary of the source instance.
    /// </summary>
    /// <returns>
    /// Human-readable source-instance summary.
    /// </returns>
    public string BuildSummary()
    {
        return
            Name +
            "; ID " +
            Id.ToString(
                CultureInfo.InvariantCulture) +
            "; " +
            ItemCount.ToString(
                CultureInfo.InvariantCulture) +
            " item(s); " +
            NumberOfPeriods.ToString(
                CultureInfo.InvariantCulture) +
            " period(s); " +
            ComponentRelationshipCount.ToString(
                CultureInfo.InvariantCulture) +
            " component relationship(s); BOM type " +
            (
                HasBomType
                    ? BomType
                    : "not declared"
            );
    }

    /// <summary>
    /// Determines whether article metadata should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when article metadata exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeArticle()
    {
        return HasArticle;
    }

    /// <summary>
    /// Determines whether the instance type should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an instance type exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeInstanceType()
    {
        return HasInstanceType;
    }

    /// <summary>
    /// Determines whether the declared bill-of-material type
    /// should be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a bill-of-material type
    /// exists; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeBomType()
    {
        return HasBomType;
    }

    /// <summary>
    /// Determines whether the source item collection should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// Always <see langword="true"/> so that an empty source
    /// collection remains represented by an explicit
    /// <c>Items</c> element.
    /// </returns>
    public bool ShouldSerializeItems()
    {
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BuildSummary();
    }
}