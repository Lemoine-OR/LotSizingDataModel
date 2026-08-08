using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Analysis;

/// <summary>
/// Represents the detailed result of an automatic analysis
/// of a product bill-of-materials graph.
/// </summary>
/// <remarks>
/// The analyzed graph uses directed arcs from component items
/// to the immediate parent items that consume them.
///
/// This class is an immutable analysis snapshot. Persistent
/// instance metadata is stored separately in
/// ProductStructureDescriptor.
/// </remarks>
[Serializable]
public sealed class ProductStructureAnalysis
{
    private readonly int[] _rootItemIds;
    private readonly int[] _leafItemIds;
    private readonly int[] _isolatedItemIds;
    private readonly int[] _sharedComponentItemIds;
    private readonly int[] _cyclicItemIds;
    private readonly string[] _errors;
    private readonly string[] _warnings;

    /// <summary>
    /// Initializes a product-structure analysis result.
    /// </summary>
    /// <param name="detectedType">
    /// Product-structure type detected by the analyzer.
    /// </param>
    /// <param name="itemCount">
    /// Number of items considered by the analysis.
    /// </param>
    /// <param name="relationshipCount">
    /// Number of distinct component-to-parent relationships.
    /// </param>
    /// <param name="connectedComponentCount">
    /// Number of connected components in the underlying
    /// undirected product-structure graph.
    /// </param>
    /// <param name="maximumDepth">
    /// Maximum number of relationships on a directed path
    /// from a leaf item to a root item.
    /// </param>
    /// <param name="maximumImmediateComponentCount">
    /// Largest number of immediate components consumed
    /// by a single parent item.
    /// </param>
    /// <param name="maximumImmediateParentCount">
    /// Largest number of immediate parent items consuming
    /// a single component.
    /// </param>
    /// <param name="rootItemIds">
    /// Identifiers of root items.
    /// </param>
    /// <param name="leafItemIds">
    /// Identifiers of leaf items.
    /// </param>
    /// <param name="isolatedItemIds">
    /// Identifiers of items having no bill-of-materials
    /// relationship.
    /// </param>
    /// <param name="sharedComponentItemIds">
    /// Identifiers of components consumed by several
    /// immediate parent items.
    /// </param>
    /// <param name="cyclicItemIds">
    /// Identifiers of items belonging to at least one cycle.
    /// </param>
    /// <param name="errors">
    /// Errors detected during the analysis.
    /// </param>
    /// <param name="warnings">
    /// Non-fatal warnings detected during the analysis.
    /// </param>
    internal ProductStructureAnalysis(
        ProductStructureType detectedType,
        int itemCount,
        int relationshipCount,
        int connectedComponentCount,
        int maximumDepth,
        int maximumImmediateComponentCount,
        int maximumImmediateParentCount,
        IEnumerable<int> rootItemIds,
        IEnumerable<int> leafItemIds,
        IEnumerable<int> isolatedItemIds,
        IEnumerable<int> sharedComponentItemIds,
        IEnumerable<int> cyclicItemIds,
        IEnumerable<string> errors,
        IEnumerable<string> warnings)
    {
        ValidateNonNegativeCount(
            itemCount,
            nameof(itemCount));

        ValidateNonNegativeCount(
            relationshipCount,
            nameof(relationshipCount));

        ValidateNonNegativeCount(
            connectedComponentCount,
            nameof(connectedComponentCount));

        ValidateNonNegativeCount(
            maximumDepth,
            nameof(maximumDepth));

        ValidateNonNegativeCount(
            maximumImmediateComponentCount,
            nameof(maximumImmediateComponentCount));

        ValidateNonNegativeCount(
            maximumImmediateParentCount,
            nameof(maximumImmediateParentCount));

        DetectedType = detectedType;
        ItemCount = itemCount;
        RelationshipCount = relationshipCount;

        ConnectedComponentCount =
            connectedComponentCount;

        MaximumDepth = maximumDepth;

        MaximumImmediateComponentCount =
            maximumImmediateComponentCount;

        MaximumImmediateParentCount =
            maximumImmediateParentCount;

        _rootItemIds =
            NormalizeItemIds(
                rootItemIds,
                nameof(rootItemIds));

        _leafItemIds =
            NormalizeItemIds(
                leafItemIds,
                nameof(leafItemIds));

        _isolatedItemIds =
            NormalizeItemIds(
                isolatedItemIds,
                nameof(isolatedItemIds));

        _sharedComponentItemIds =
            NormalizeItemIds(
                sharedComponentItemIds,
                nameof(sharedComponentItemIds));

        _cyclicItemIds =
            NormalizeItemIds(
                cyclicItemIds,
                nameof(cyclicItemIds));

        _errors =
            NormalizeMessages(
                errors,
                nameof(errors));

        _warnings =
            NormalizeMessages(
                warnings,
                nameof(warnings));

        if (_cyclicItemIds.Length > 0 &&
            DetectedType != ProductStructureType.Unknown)
        {
            throw new ArgumentException(
                "A cyclic product structure cannot have " +
                "a valid detected structure type.",
                nameof(detectedType));
        }
    }

    /// <summary>
    /// Gets the product-structure type detected
    /// by the analyzer.
    /// </summary>
    /// <remarks>
    /// The value is <see cref="ProductStructureType.Unknown"/>
    /// when the graph is invalid or cannot be classified.
    /// </remarks>
    public ProductStructureType DetectedType { get; }

    /// <summary>
    /// Gets the number of items considered by the analysis.
    /// </summary>
    public int ItemCount { get; }

    /// <summary>
    /// Gets the number of distinct component-to-parent
    /// relationships.
    /// </summary>
    public int RelationshipCount { get; }

    /// <summary>
    /// Gets the number of connected components in the
    /// underlying undirected product-structure graph.
    /// </summary>
    public int ConnectedComponentCount { get; }

    /// <summary>
    /// Gets the maximum number of relationships on a directed
    /// path from a leaf item to a root item.
    /// </summary>
    /// <remarks>
    /// The value is zero when the instance contains no
    /// bill-of-materials relationship or when a valid depth
    /// cannot be calculated.
    /// </remarks>
    public int MaximumDepth { get; }

    /// <summary>
    /// Gets the largest number of immediate components
    /// consumed by a single parent item.
    /// </summary>
    public int MaximumImmediateComponentCount { get; }

    /// <summary>
    /// Gets the largest number of immediate parent items
    /// consuming a single component.
    /// </summary>
    public int MaximumImmediateParentCount { get; }

    /// <summary>
    /// Gets the identifiers of root items.
    /// </summary>
    /// <remarks>
    /// A root item is not consumed as a component by another
    /// item. An isolated item is therefore also a root.
    /// </remarks>
    public IReadOnlyList<int> RootItemIds =>
        _rootItemIds;

    /// <summary>
    /// Gets the identifiers of leaf items.
    /// </summary>
    /// <remarks>
    /// A leaf item does not consume another component.
    /// An isolated item is therefore also a leaf.
    /// </remarks>
    public IReadOnlyList<int> LeafItemIds =>
        _leafItemIds;

    /// <summary>
    /// Gets the identifiers of items having no
    /// bill-of-materials relationship.
    /// </summary>
    public IReadOnlyList<int> IsolatedItemIds =>
        _isolatedItemIds;

    /// <summary>
    /// Gets the identifiers of components consumed by more
    /// than one immediate parent item.
    /// </summary>
    public IReadOnlyList<int> SharedComponentItemIds =>
        _sharedComponentItemIds;

    /// <summary>
    /// Gets the identifiers of items belonging to at least
    /// one directed cycle.
    /// </summary>
    public IReadOnlyList<int> CyclicItemIds =>
        _cyclicItemIds;

    /// <summary>
    /// Gets the errors detected during the analysis.
    /// </summary>
    public IReadOnlyList<string> Errors =>
        _errors;

    /// <summary>
    /// Gets the non-fatal warnings detected during
    /// the analysis.
    /// </summary>
    public IReadOnlyList<string> Warnings =>
        _warnings;

    /// <summary>
    /// Gets a value indicating whether the product-structure
    /// graph contains at least one relationship.
    /// </summary>
    public bool HasRelationships =>
        RelationshipCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one item
    /// has no bill-of-materials relationship.
    /// </summary>
    public bool HasIsolatedItems =>
        IsolatedItemIds.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one component
    /// is consumed by several immediate parent items.
    /// </summary>
    public bool HasSharedComponents =>
        SharedComponentItemIds.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the product-structure
    /// graph contains at least one directed cycle.
    /// </summary>
    public bool HasCycle =>
        CyclicItemIds.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the product structure
    /// contains more than one bill-of-materials level.
    /// </summary>
    public bool IsMultiLevel =>
        MaximumDepth > 1;

    /// <summary>
    /// Gets a value indicating whether at least one parent
    /// item consumes several immediate components.
    /// </summary>
    public bool HasAssemblyNodes =>
        MaximumImmediateComponentCount > 1;

    /// <summary>
    /// Gets a value indicating whether at least one component
    /// is consumed by several immediate parent items.
    /// </summary>
    public bool HasDivergentComponents =>
        MaximumImmediateParentCount > 1;

    /// <summary>
    /// Gets a value indicating whether at least one error
    /// was detected.
    /// </summary>
    public bool HasErrors =>
        Errors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one warning
    /// was detected.
    /// </summary>
    public bool HasWarnings =>
        Warnings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the analyzed graph
    /// is structurally valid.
    /// </summary>
    /// <remarks>
    /// A valid graph contains no detected error and no
    /// directed cycle.
    /// </remarks>
    public bool IsValid =>
        !HasErrors &&
        !HasCycle;

    /// <inheritdoc/>
    public override string ToString()
    {
        string validity =
            IsValid
                ? "valid"
                : "invalid";

        return
            $"{DetectedType} — {validity}; " +
            $"{ItemCount} items; " +
            $"{RelationshipCount} relationships; " +
            $"depth {MaximumDepth}";
    }

    private static int[] NormalizeItemIds(
        IEnumerable<int> itemIds,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        int[] normalizedItemIds =
            itemIds
                .Distinct()
                .OrderBy(
                    itemId =>
                        itemId)
                .ToArray();

        if (normalizedItemIds.Any(
                itemId =>
                    itemId <= 0))
        {
            throw new ArgumentException(
                "Every item identifier must be " +
                "strictly positive.",
                parameterName);
        }

        return normalizedItemIds;
    }

    private static string[] NormalizeMessages(
        IEnumerable<string> messages,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(messages);

        return messages
            .Where(
                message =>
                    !string.IsNullOrWhiteSpace(
                        message))
            .Select(
                message =>
                    message.Trim())
            .Distinct(
                StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateNonNegativeCount(
        int value,
        string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value cannot be negative.");
        }
    }
}