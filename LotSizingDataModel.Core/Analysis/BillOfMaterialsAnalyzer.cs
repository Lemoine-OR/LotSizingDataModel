using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.LogicalModel;

namespace LotSizingDataModel.Core.Analysis;

/// <summary>
/// Provides analysis operations for the bill of materials.
///
/// The bill of materials is represented by directed relationships
/// from a parent item to its component items.
/// </summary>
public sealed class BillOfMaterialsAnalyzer
{
    private readonly Dictionary<int, List<ComponentRequirement>>
        _requirementsByParent = new();

    private readonly Dictionary<int, List<ComponentRequirement>>
        _requirementsByComponent = new();

    /// <summary>
    /// Initializes an analyzer and creates a new entity index.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain containing the bill of materials.
    /// </param>
    public BillOfMaterialsAnalyzer(SupplyChain supplyChain)
        : this(
            new SupplyChainIndex(
                supplyChain ??
                throw new ArgumentNullException(
                    nameof(supplyChain))))
    {
    }

    /// <summary>
    /// Initializes an analyzer using an existing entity index.
    /// </summary>
    /// <param name="index">
    /// Index used to resolve item references.
    /// </param>
    public BillOfMaterialsAnalyzer(
        SupplyChainIndex index)
    {
        Index = index ??
            throw new ArgumentNullException(nameof(index));

        SupplyChain = index.SupplyChain;

        Rebuild();
    }

    /// <summary>
    /// Gets the analyzed supply chain.
    /// </summary>
    public SupplyChain SupplyChain { get; }

    /// <summary>
    /// Gets the entity index used by the analyzer.
    /// </summary>
    public SupplyChainIndex Index { get; }

    /// <summary>
    /// Rebuilds the entity index and the bill-of-material indexes.
    ///
    /// Call this method after adding, removing or modifying
    /// component requirements.
    /// </summary>
    public void Rebuild()
    {
        Index.Rebuild();

        _requirementsByParent.Clear();
        _requirementsByComponent.Clear();

        foreach (ComponentRequirement requirement
                 in SupplyChain.ComponentRequirements)
        {
            /*
             * These calls ensure that both referenced items exist.
             */
            Index.GetRequiredItem(
                requirement.ParentItemId);

            Index.GetRequiredItem(
                requirement.ComponentItemId);

            AddRequirement(
                _requirementsByParent,
                requirement.ParentItemId,
                requirement);

            AddRequirement(
                _requirementsByComponent,
                requirement.ComponentItemId,
                requirement);
        }
    }

    #region Direct relationships

    /// <summary>
    /// Gets the direct component requirements of a parent item.
    /// </summary>
    public IReadOnlyList<ComponentRequirement>
        GetDirectRequirements(int parentItemId)
    {
        Index.GetRequiredItem(parentItemId);

        if (!_requirementsByParent.TryGetValue(
                parentItemId,
                out List<ComponentRequirement>? requirements))
        {
            return Array.Empty<ComponentRequirement>();
        }

        return requirements.ToArray();
    }

    /// <summary>
    /// Gets the direct components of a parent item.
    /// </summary>
    public IReadOnlyList<Item> GetDirectComponents(
        int parentItemId)
    {
        return GetDirectRequirements(parentItemId)
            .Select(
                requirement =>
                    Index.GetRequiredItem(
                        requirement.ComponentItemId))
            .ToArray();
    }

    /// <summary>
    /// Gets the requirements in which an item is used
    /// as a direct component.
    /// </summary>
    public IReadOnlyList<ComponentRequirement>
        GetDirectParentRequirements(int componentItemId)
    {
        Index.GetRequiredItem(componentItemId);

        if (!_requirementsByComponent.TryGetValue(
                componentItemId,
                out List<ComponentRequirement>? requirements))
        {
            return Array.Empty<ComponentRequirement>();
        }

        return requirements.ToArray();
    }

    /// <summary>
    /// Gets the direct parent items using a component.
    /// </summary>
    public IReadOnlyList<Item> GetDirectParents(
        int componentItemId)
    {
        return GetDirectParentRequirements(componentItemId)
            .Select(
                requirement =>
                    Index.GetRequiredItem(
                        requirement.ParentItemId))
            .ToArray();
    }

    /// <summary>
    /// Determines whether an item has no component.
    /// </summary>
    public bool IsLeafItem(int itemId)
    {
        Index.GetRequiredItem(itemId);

        return !_requirementsByParent.ContainsKey(itemId);
    }

    /// <summary>
    /// Determines whether an item is not used as a component
    /// of another item.
    /// </summary>
    public bool IsRootItem(int itemId)
    {
        Index.GetRequiredItem(itemId);

        return !_requirementsByComponent.ContainsKey(itemId);
    }

    /// <summary>
    /// Gets all root items of the bill of materials.
    /// </summary>
    public IReadOnlyList<Item> GetRootItems()
    {
        return SupplyChain.Items
            .Where(item => IsRootItem(item.Id))
            .OrderBy(item => item.Id)
            .ToArray();
    }

    /// <summary>
    /// Gets all leaf items of the bill of materials.
    /// </summary>
    public IReadOnlyList<Item> GetLeafItems()
    {
        return SupplyChain.Items
            .Where(item => IsLeafItem(item.Id))
            .OrderBy(item => item.Id)
            .ToArray();
    }

    #endregion

    #region Topological order and cycle detection

    /// <summary>
    /// Determines whether the bill of materials contains
    /// at least one circular dependency.
    /// </summary>
    public bool HasCycle =>
        !TryGetTopologicalOrder(out _);

    /// <summary>
    /// Attempts to calculate a topological order in which
    /// every parent item appears before its components.
    /// </summary>
    /// <param name="items">
    /// Calculated order, or an empty collection when a cycle exists.
    /// </param>
    public bool TryGetTopologicalOrder(
        out IReadOnlyList<Item> items)
    {
        Dictionary<int, int> incomingEdgeCounts =
            SupplyChain.Items.ToDictionary(
                item => item.Id,
                _ => 0);

        foreach (ComponentRequirement requirement
                 in SupplyChain.ComponentRequirements)
        {
            incomingEdgeCounts[
                requirement.ComponentItemId]++;
        }

        var availableItemIds =
            new SortedSet<int>(
                incomingEdgeCounts
                    .Where(pair => pair.Value == 0)
                    .Select(pair => pair.Key));

        var orderedItems =
            new List<Item>(
                SupplyChain.Items.Count);

        while (availableItemIds.Count > 0)
        {
            int currentItemId =
                availableItemIds.First();

            availableItemIds.Remove(currentItemId);

            orderedItems.Add(
                Index.GetRequiredItem(currentItemId));

            if (!_requirementsByParent.TryGetValue(
                    currentItemId,
                    out List<ComponentRequirement>? requirements))
            {
                continue;
            }

            foreach (ComponentRequirement requirement
                     in requirements)
            {
                int componentItemId =
                    requirement.ComponentItemId;

                incomingEdgeCounts[componentItemId]--;

                if (incomingEdgeCounts[componentItemId] == 0)
                {
                    availableItemIds.Add(componentItemId);
                }
            }
        }

        if (orderedItems.Count != SupplyChain.Items.Count)
        {
            items = Array.Empty<Item>();
            return false;
        }

        items = orderedItems;
        return true;
    }

    /// <summary>
    /// Gets a topological order in which parent items appear
    /// before their components.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the bill of materials contains a cycle.
    /// </exception>
    public IReadOnlyList<Item> GetTopologicalOrder()
    {
        if (TryGetTopologicalOrder(
                out IReadOnlyList<Item> items))
        {
            return items;
        }

        throw new InvalidOperationException(
            "The bill of materials contains a circular dependency.");
    }

    /// <summary>
    /// Gets an order in which components appear before
    /// their parent items.
    ///
    /// This order is useful for bottom-up calculations.
    /// </summary>
    public IReadOnlyList<Item> GetReverseTopologicalOrder()
    {
        return GetTopologicalOrder()
            .Reverse()
            .ToArray();
    }

    #endregion

    #region Level calculation

    /// <summary>
    /// Calculates the bill-of-material level of every item.
    ///
    /// Root items receive level zero. The level of a component
    /// is the maximum parent level plus one.
    /// </summary>
    public IReadOnlyDictionary<int, int> CalculateLevels()
    {
        IReadOnlyList<Item> topologicalOrder =
            GetTopologicalOrder();

        Dictionary<int, int> levels =
            SupplyChain.Items.ToDictionary(
                item => item.Id,
                _ => 0);

        foreach (Item parentItem in topologicalOrder)
        {
            if (!_requirementsByParent.TryGetValue(
                    parentItem.Id,
                    out List<ComponentRequirement>? requirements))
            {
                continue;
            }

            int componentLevel =
                checked(levels[parentItem.Id] + 1);

            foreach (ComponentRequirement requirement
                     in requirements)
            {
                int currentLevel =
                    levels[requirement.ComponentItemId];

                if (componentLevel > currentLevel)
                {
                    levels[requirement.ComponentItemId] =
                        componentLevel;
                }
            }
        }

        return levels;
    }

    /// <summary>
    /// Recalculates and updates the BillOfMaterialsLevel property
    /// of every item.
    /// </summary>
    public void ApplyCalculatedLevels()
    {
        IReadOnlyDictionary<int, int> levels =
            CalculateLevels();

        foreach (Item item in SupplyChain.Items)
        {
            item.BillOfMaterialsLevel =
                levels[item.Id];
        }
    }

    /// <summary>
    /// Determines whether the levels currently stored in the items
    /// match the levels calculated from the bill of materials.
    /// </summary>
    public bool HasConsistentStoredLevels()
    {
        IReadOnlyDictionary<int, int> calculatedLevels =
            CalculateLevels();

        return SupplyChain.Items.All(
            item =>
                item.BillOfMaterialsLevel ==
                calculatedLevels[item.Id]);
    }

    #endregion

    #region Cumulative requirements

    /// <summary>
    /// Calculates the cumulative component coefficients required
    /// to manufacture one unit of a parent item.
    ///
    /// When a component is reached through several branches,
    /// all contributions are added.
    /// </summary>
    /// <param name="parentItemId">
    /// Identifier of the manufactured parent item.
    /// </param>
    /// <returns>
    /// Component identifiers and cumulative integer coefficients.
    /// The parent item itself is excluded.
    /// </returns>
    public IReadOnlyDictionary<int, long>
        GetCumulativeRequirementCoefficients(
            int parentItemId)
    {
        Index.GetRequiredItem(parentItemId);

        IReadOnlyList<Item> topologicalOrder =
            GetTopologicalOrder();

        var quantities =
            new Dictionary<int, long>
            {
                [parentItemId] = 1L
            };

        foreach (Item currentItem in topologicalOrder)
        {
            if (!quantities.TryGetValue(
                    currentItem.Id,
                    out long currentQuantity))
            {
                continue;
            }

            if (!_requirementsByParent.TryGetValue(
                    currentItem.Id,
                    out List<ComponentRequirement>? requirements))
            {
                continue;
            }

            foreach (ComponentRequirement requirement
                     in requirements)
            {
                long contribution;

                try
                {
                    contribution = checked(
                        currentQuantity *
                        requirement.Quantity);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidOperationException(
                        "The cumulative bill-of-material coefficient " +
                        "exceeds the supported integer range.",
                        exception);
                }

                quantities.TryGetValue(
                    requirement.ComponentItemId,
                    out long existingQuantity);

                try
                {
                    quantities[requirement.ComponentItemId] =
                        checked(
                            existingQuantity +
                            contribution);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidOperationException(
                        "The cumulative bill-of-material coefficient " +
                        "exceeds the supported integer range.",
                        exception);
                }
            }
        }

        quantities.Remove(parentItemId);

        return quantities;
    }

    /// <summary>
    /// Calculates the gross component requirements for a given
    /// production quantity of a parent item.
    /// </summary>
    /// <param name="parentItemId">
    /// Identifier of the manufactured parent item.
    /// </param>
    /// <param name="parentQuantity">
    /// Non-negative finite production quantity.
    /// </param>
    public IReadOnlyDictionary<int, double>
        CalculateGrossComponentRequirements(
            int parentItemId,
            double parentQuantity)
    {
        if (!double.IsFinite(parentQuantity) ||
            parentQuantity < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parentQuantity),
                parentQuantity,
                "The parent-item quantity must be finite " +
                "and non-negative.");
        }

        IReadOnlyDictionary<int, long> coefficients =
            GetCumulativeRequirementCoefficients(
                parentItemId);

        var requirements =
            new Dictionary<int, double>();

        foreach (KeyValuePair<int, long> pair
                 in coefficients)
        {
            double quantity =
                pair.Value * parentQuantity;

            if (!double.IsFinite(quantity))
            {
                throw new InvalidOperationException(
                    "A gross component requirement exceeds " +
                    "the supported numerical range.");
            }

            requirements[pair.Key] = quantity;
        }

        return requirements;
    }

    #endregion

    private static void AddRequirement(
        IDictionary<int, List<ComponentRequirement>> dictionary,
        int itemId,
        ComponentRequirement requirement)
    {
        if (!dictionary.TryGetValue(
                itemId,
                out List<ComponentRequirement>? requirements))
        {
            requirements =
                new List<ComponentRequirement>();

            dictionary.Add(
                itemId,
                requirements);
        }

        requirements.Add(requirement);
    }
}