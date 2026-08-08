using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.DellaertJeunet.XmlModel;

/// <summary>
/// Represents one item declared in a Dellaert–Jeunet XML
/// benchmark instance.
/// </summary>
/// <remarks>
/// This class is a source-format data-transfer object.
///
/// It reproduces the structure of an <c>Item</c> element:
/// <code>
/// &lt;Item&gt;
///   &lt;Id&gt;1&lt;/Id&gt;
///   &lt;Name&gt;Item 1&lt;/Name&gt;
///   &lt;DepthInBOM&gt;1&lt;/DepthInBOM&gt;
///   &lt;Demand&gt;
///     &lt;int&gt;0&lt;/int&gt;
///     &lt;int&gt;105&lt;/int&gt;
///   &lt;/Demand&gt;
///   &lt;SetupCost&gt;1360&lt;/SetupCost&gt;
///   &lt;HoldingCost&gt;50.27&lt;/HoldingCost&gt;
///   &lt;ProductionCost&gt;0&lt;/ProductionCost&gt;
///   &lt;ListOfComponents&gt;
///     &lt;Component&gt;...&lt;/Component&gt;
///   &lt;/ListOfComponents&gt;
/// &lt;/Item&gt;
/// </code>
///
/// An empty <c>Demand</c> element is represented by an empty
/// collection. Its conversion into a zero-filled time series
/// is performed later by the importer, according to the
/// selected import options.
///
/// Similarly, an empty <c>ListOfComponents</c> element is
/// represented by an empty collection and normally identifies
/// a leaf item.
/// </remarks>
[Serializable]
[XmlType(
    TypeName = "Item",
    AnonymousType = true)]
public sealed class DellaertJeunetXmlItem
{
    private int _id;

    private string _name =
        string.Empty;

    private int _depthInBom;

    private List<int> _demand =
        new();

    private decimal _setupCost;

    private decimal _holdingCost;

    private decimal _productionCost;

    private List<DellaertJeunetXmlComponent> _components =
        new();

    /// <summary>
    /// Initializes an empty source item.
    /// </summary>
    /// <remarks>
    /// This public parameterless constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public DellaertJeunetXmlItem()
    {
    }

    /// <summary>
    /// Initializes a source item.
    /// </summary>
    /// <param name="id">
    /// Source item identifier.
    /// </param>
    /// <param name="name">
    /// Source item name.
    /// </param>
    /// <param name="depthInBom">
    /// Depth declared in the bill of materials.
    /// </param>
    /// <param name="demand">
    /// External-demand time series.
    /// </param>
    /// <param name="setupCost">
    /// Item setup cost.
    /// </param>
    /// <param name="holdingCost">
    /// Item holding cost.
    /// </param>
    /// <param name="productionCost">
    /// Item production cost.
    /// </param>
    /// <param name="components">
    /// Components required by this parent item.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="demand"/> or
    /// <paramref name="components"/> is
    /// <see langword="null"/>.
    /// </exception>
    public DellaertJeunetXmlItem(
        int id,
        string name,
        int depthInBom,
        IEnumerable<int> demand,
        decimal setupCost,
        decimal holdingCost,
        decimal productionCost,
        IEnumerable<DellaertJeunetXmlComponent> components)
    {
        ArgumentNullException.ThrowIfNull(
            demand);

        ArgumentNullException.ThrowIfNull(
            components);

        Id =
            id;

        Name =
            name;

        DepthInBom =
            depthInBom;

        Demand =
            demand.ToList();

        SetupCost =
            setupCost;

        HoldingCost =
            holdingCost;

        ProductionCost =
            productionCost;

        Components =
            components.ToList();
    }

    /// <summary>
    /// Gets or sets the source item identifier.
    /// </summary>
    /// <remarks>
    /// The source XML element is named <c>Id</c>.
    ///
    /// This differs from the uppercase <c>ID</c> used inside
    /// component relationships.
    /// </remarks>
    [XmlElement(
        "Id",
        Order = 0)]
    public int Id
    {
        get =>
            _id;

        set =>
            _id =
                value;
    }

    /// <summary>
    /// Gets or sets the source item name.
    /// </summary>
    [XmlElement(
        "Name",
        Order = 1)]
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
    /// Gets or sets the item depth declared in the bill of
    /// materials.
    /// </summary>
    /// <remarks>
    /// This value is preserved from the source document.
    ///
    /// It must not be considered authoritative until it has
    /// been compared with the depth calculated from the
    /// complete product-structure graph.
    /// </remarks>
    [XmlElement(
        "DepthInBOM",
        Order = 2)]
    public int DepthInBom
    {
        get =>
            _depthInBom;

        set =>
            _depthInBom =
                value;
    }

    /// <summary>
    /// Gets or sets the external-demand time series.
    /// </summary>
    /// <remarks>
    /// The source format stores demand inside a
    /// <c>Demand</c> element containing repeated
    /// <c>int</c> elements.
    ///
    /// An empty source element:
    /// <code>
    /// &lt;Demand /&gt;
    /// </code>
    /// produces an empty collection.
    /// </remarks>
    [XmlArray(
        "Demand",
        Order = 3)]
    [XmlArrayItem(
        "int",
        typeof(int),
        IsNullable = false)]
    public List<int> Demand
    {
        get =>
            _demand;

        set =>
            _demand =
                value ??
                new List<int>();
    }

    /// <summary>
    /// Gets or sets the fixed setup cost associated with a
    /// production launch of this item.
    /// </summary>
    [XmlElement(
        "SetupCost",
        Order = 4)]
    public decimal SetupCost
    {
        get =>
            _setupCost;

        set =>
            _setupCost =
                value;
    }

    /// <summary>
    /// Gets or sets the unit holding cost associated with this
    /// item.
    /// </summary>
    [XmlElement(
        "HoldingCost",
        Order = 5)]
    public decimal HoldingCost
    {
        get =>
            _holdingCost;

        set =>
            _holdingCost =
                value;
    }

    /// <summary>
    /// Gets or sets the unit production cost associated with
    /// this item.
    /// </summary>
    [XmlElement(
        "ProductionCost",
        Order = 6)]
    public decimal ProductionCost
    {
        get =>
            _productionCost;

        set =>
            _productionCost =
                value;
    }

    /// <summary>
    /// Gets or sets the components required to produce this
    /// parent item.
    /// </summary>
    /// <remarks>
    /// The outer XML element is named
    /// <c>ListOfComponents</c>, while each contained element
    /// is named <c>Component</c>.
    ///
    /// The current item is the parent of every relationship in
    /// this collection.
    /// </remarks>
    [XmlArray(
        "ListOfComponents",
        Order = 7)]
    [XmlArrayItem(
        "Component",
        typeof(DellaertJeunetXmlComponent),
        IsNullable = false)]
    public List<DellaertJeunetXmlComponent> Components
    {
        get =>
            _components;

        set =>
            _components =
                value ??
                new List<DellaertJeunetXmlComponent>();
    }

    /// <summary>
    /// Gets a value indicating whether the source item
    /// identifier is strictly positive.
    /// </summary>
    [XmlIgnore]
    public bool HasValidId =>
        Id > 0;

    /// <summary>
    /// Gets a value indicating whether the source item has a
    /// non-empty name.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(
            Name);

    /// <summary>
    /// Gets a value indicating whether the declared
    /// bill-of-material depth is strictly positive.
    /// </summary>
    [XmlIgnore]
    public bool HasValidDeclaredDepth =>
        DepthInBom > 0;

    /// <summary>
    /// Gets a value indicating whether the item has at least
    /// one explicitly stored external-demand value.
    /// </summary>
    [XmlIgnore]
    public bool HasDemandValues =>
        Demand.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the source demand is
    /// empty.
    /// </summary>
    [XmlIgnore]
    public bool HasEmptyDemand =>
        Demand.Count == 0;

    /// <summary>
    /// Gets the number of demand values stored in the source
    /// document.
    /// </summary>
    [XmlIgnore]
    public int DemandValueCount =>
        Demand.Count;

    /// <summary>
    /// Gets the sum of the external-demand values.
    /// </summary>
    [XmlIgnore]
    public long TotalExternalDemand =>
        Demand.Aggregate(
            0L,
            (total, value) =>
                total + value);

    /// <summary>
    /// Gets a value indicating whether at least one external
    /// demand value is strictly positive.
    /// </summary>
    [XmlIgnore]
    public bool HasPositiveExternalDemand =>
        Demand.Any(
            value =>
                value > 0);

    /// <summary>
    /// Gets a value indicating whether at least one external
    /// demand value is negative.
    /// </summary>
    [XmlIgnore]
    public bool HasNegativeDemand =>
        Demand.Any(
            value =>
                value < 0);

    /// <summary>
    /// Gets a value indicating whether the setup cost is
    /// negative.
    /// </summary>
    [XmlIgnore]
    public bool HasNegativeSetupCost =>
        SetupCost < 0m;

    /// <summary>
    /// Gets a value indicating whether the holding cost is
    /// negative.
    /// </summary>
    [XmlIgnore]
    public bool HasNegativeHoldingCost =>
        HoldingCost < 0m;

    /// <summary>
    /// Gets a value indicating whether the production cost is
    /// negative.
    /// </summary>
    [XmlIgnore]
    public bool HasNegativeProductionCost =>
        ProductionCost < 0m;

    /// <summary>
    /// Gets a value indicating whether at least one item cost
    /// is negative.
    /// </summary>
    [XmlIgnore]
    public bool HasNegativeCost =>
        HasNegativeSetupCost ||
        HasNegativeHoldingCost ||
        HasNegativeProductionCost;

    /// <summary>
    /// Gets a value indicating whether the item declares at
    /// least one component.
    /// </summary>
    [XmlIgnore]
    public bool HasComponents =>
        Components.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the item is a leaf
    /// according to the source component list.
    /// </summary>
    [XmlIgnore]
    public bool IsLeaf =>
        Components.Count == 0;

    /// <summary>
    /// Gets the number of component relationships declared by
    /// this item.
    /// </summary>
    [XmlIgnore]
    public int ComponentCount =>
        Components.Count;

    /// <summary>
    /// Gets the distinct identifiers of all referenced
    /// component items.
    /// </summary>
    [XmlIgnore]
    public IReadOnlyList<int> DistinctComponentIds =>
        Components
            .Where(
                component =>
                    component is not null)
            .Select(
                component =>
                    component.ItemId)
            .Distinct()
            .OrderBy(
                componentId =>
                    componentId)
            .ToArray();

    /// <summary>
    /// Gets a value indicating whether this item contains a
    /// component relationship referencing itself.
    /// </summary>
    [XmlIgnore]
    public bool HasSelfReference =>
        Components.Any(
            component =>
                component is not null &&
                component.IsSelfReference(Id));

    /// <summary>
    /// Gets a value indicating whether the same component item
    /// identifier is declared more than once.
    /// </summary>
    [XmlIgnore]
    public bool HasDuplicateComponentReferences =>
        Components
            .Where(
                component =>
                    component is not null)
            .GroupBy(
                component =>
                    component.ItemId)
            .Any(
                group =>
                    group.Count() > 1);

    /// <summary>
    /// Gets a value indicating whether at least one component
    /// relationship is structurally invalid.
    /// </summary>
    [XmlIgnore]
    public bool HasInvalidComponent =>
        Components.Any(
            component =>
                component is null ||
                !component.IsValid);

    /// <summary>
    /// Gets a value indicating whether the item is
    /// structurally valid independently from the complete
    /// source instance.
    /// </summary>
    /// <remarks>
    /// This validation checks:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// a strictly positive item identifier;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a strictly positive declared depth;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// non-negative demand values;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// non-negative costs;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// structurally valid component relationships;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// absence of direct self-references;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// absence of duplicate component identifiers.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// The existence of referenced component items and the
    /// consistency of demand length with the planning horizon
    /// require validation at instance level.
    /// </remarks>
    [XmlIgnore]
    public bool IsStructurallyValid =>
        HasValidId &&
        HasValidDeclaredDepth &&
        !HasNegativeDemand &&
        !HasNegativeCost &&
        !HasInvalidComponent &&
        !HasSelfReference &&
        !HasDuplicateComponentReferences;

    /// <summary>
    /// Determines whether the demand time-series length is
    /// compatible with the supplied planning horizon.
    /// </summary>
    /// <param name="numberOfPeriods">
    /// Number of periods in the source instance.
    /// </param>
    /// <param name="allowEmptyDemand">
    /// Value indicating whether an empty demand collection is
    /// accepted.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the demand is empty and
    /// empty demand is allowed, or when the number of demand
    /// values equals the planning horizon; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="numberOfPeriods"/> is not
    /// strictly positive.
    /// </exception>
    public bool HasCompatibleDemandLength(
        int numberOfPeriods,
        bool allowEmptyDemand = true)
    {
        if (numberOfPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfPeriods),
                numberOfPeriods,
                "The number of periods must be strictly " +
                "positive.");
        }

        if (HasEmptyDemand)
        {
            return allowEmptyDemand;
        }

        return
            Demand.Count ==
            numberOfPeriods;
    }

    /// <summary>
    /// Returns the external-demand series expected by the
    /// target model.
    /// </summary>
    /// <param name="numberOfPeriods">
    /// Number of planning periods.
    /// </param>
    /// <param name="convertEmptyDemandToZeroSeries">
    /// Value indicating whether an empty source demand must be
    /// replaced by a zero-filled series.
    /// </param>
    /// <returns>
    /// Copy of the source demand or a zero-filled series.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="numberOfPeriods"/> is not
    /// strictly positive.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when demand is empty and conversion is disabled,
    /// or when its length differs from the planning horizon.
    /// </exception>
    public IReadOnlyList<int> BuildDemandSeries(
        int numberOfPeriods,
        bool convertEmptyDemandToZeroSeries)
    {
        if (numberOfPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfPeriods),
                numberOfPeriods,
                "The number of periods must be strictly " +
                "positive.");
        }

        if (HasEmptyDemand)
        {
            if (!convertEmptyDemandToZeroSeries)
            {
                throw new InvalidOperationException(
                    $"Item {Id} has an empty external-demand " +
                    "series.");
            }

            return
                Enumerable
                    .Repeat(
                        0,
                        numberOfPeriods)
                    .ToArray();
        }

        if (Demand.Count != numberOfPeriods)
        {
            throw new InvalidOperationException(
                $"Item {Id} contains {Demand.Count} demand " +
                $"value(s), while {numberOfPeriods} were " +
                "expected.");
        }

        return Demand.ToArray();
    }

    /// <summary>
    /// Determines whether this item references the supplied
    /// component item.
    /// </summary>
    /// <param name="componentItemId">
    /// Component item identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a matching component
    /// relationship exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ReferencesComponent(
        int componentItemId)
    {
        return Components.Any(
            component =>
                component is not null &&
                component.ReferencesItem(
                    componentItemId));
    }

    /// <summary>
    /// Returns all relationships referencing the supplied
    /// component identifier.
    /// </summary>
    /// <param name="componentItemId">
    /// Component item identifier.
    /// </param>
    /// <returns>
    /// Matching component relationships.
    /// </returns>
    public IReadOnlyList<DellaertJeunetXmlComponent>
        GetComponentRelationships(
            int componentItemId)
    {
        return Components
            .Where(
                component =>
                    component is not null &&
                    component.ItemId ==
                    componentItemId)
            .ToArray();
    }

    /// <summary>
    /// Adds a component relationship to this item.
    /// </summary>
    /// <param name="component">
    /// Component relationship to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="component"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddComponent(
        DellaertJeunetXmlComponent component)
    {
        ArgumentNullException.ThrowIfNull(
            component);

        Components.Add(
            component);
    }

    /// <summary>
    /// Removes all component relationships from this item.
    /// </summary>
    public void ClearComponents()
    {
        Components.Clear();
    }

    /// <summary>
    /// Replaces the external-demand series.
    /// </summary>
    /// <param name="values">
    /// New demand values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="values"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void SetDemand(
        IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(
            values);

        Demand =
            values.ToList();
    }

    /// <summary>
    /// Removes all external-demand values.
    /// </summary>
    public void ClearDemand()
    {
        Demand.Clear();
    }

    /// <summary>
    /// Validates this item independently from the complete
    /// source instance.
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
                "The item identifier must be strictly " +
                "positive.");
        }

        if (!HasValidDeclaredDepth)
        {
            errors.Add(
                "The declared bill-of-material depth must be " +
                "strictly positive.");
        }

        if (HasNegativeDemand)
        {
            errors.Add(
                "External-demand values cannot be negative.");
        }

        if (HasNegativeSetupCost)
        {
            errors.Add(
                "The setup cost cannot be negative.");
        }

        if (HasNegativeHoldingCost)
        {
            errors.Add(
                "The holding cost cannot be negative.");
        }

        if (HasNegativeProductionCost)
        {
            errors.Add(
                "The production cost cannot be negative.");
        }

        if (HasInvalidComponent)
        {
            errors.Add(
                "At least one component relationship is " +
                "invalid.");
        }

        if (HasSelfReference)
        {
            errors.Add(
                "The item cannot reference itself as a " +
                "component.");
        }

        if (HasDuplicateComponentReferences)
        {
            errors.Add(
                "The same component item cannot be declared " +
                "more than once for one parent item.");
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
    /// Validates this item and throws an exception when it is
    /// structurally inconsistent.
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
            $"Source item {Id} is invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        "- " + error)));
    }

    /// <summary>
    /// Determines whether the demand element should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// Always <see langword="true"/> so that an empty source
    /// demand remains represented by <c>&lt;Demand /&gt;</c>.
    /// </returns>
    public bool ShouldSerializeDemand()
    {
        return true;
    }

    /// <summary>
    /// Determines whether the component-list element should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// Always <see langword="true"/> so that leaf items retain
    /// an explicit empty <c>ListOfComponents</c> element.
    /// </returns>
    public bool ShouldSerializeComponents()
    {
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            Id.ToString(
                CultureInfo.InvariantCulture) +
            " — " +
            (
                HasName
                    ? Name
                    : "Unnamed item"
            ) +
            "; depth " +
            DepthInBom.ToString(
                CultureInfo.InvariantCulture) +
            "; " +
            ComponentCount.ToString(
                CultureInfo.InvariantCulture) +
            " component(s)";
    }
}