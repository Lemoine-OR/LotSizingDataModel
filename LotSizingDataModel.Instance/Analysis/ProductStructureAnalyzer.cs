using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Metadata;

namespace LotSizingDataModel.Instance.Analysis;

/// <summary>
/// Analyzes the bill-of-materials graph of a supply-chain
/// instance and determines its product-structure type.
/// </summary>
/// <remarks>
/// Each directed graph arc goes from a component item to
/// an immediate parent item that consumes the component.
///
/// The analyzer does not modify the supplied
/// <see cref="SupplyChain"/>.
/// </remarks>
public static class ProductStructureAnalyzer
{
    /// <summary>
    /// Gets the current version of the product-structure
    /// classification rules.
    /// </summary>
    public const string CurrentVersion = "1.0";

    /// <summary>
    /// Analyzes the product structure of a supply-chain
    /// instance.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance to analyze.
    /// </param>
    /// <returns>
    /// Detailed product-structure analysis.
    /// </returns>
    public static ProductStructureAnalysis Analyze(
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        var errors =
            new List<string>();

        var warnings =
            new List<string>();

        ValidateItemIdentifiers(
            supplyChain,
            errors);

        int[] itemIds =
            supplyChain.Items
                .Select(item => item.Id)
                .Where(itemId => itemId > 0)
                .Distinct()
                .OrderBy(itemId => itemId)
                .ToArray();

        var itemIdSet =
            itemIds.ToHashSet();

        if (itemIds.Length == 0)
        {
            errors.Add(
                "The supply-chain instance does not contain " +
                "any valid item.");
        }

        /*
         * Directed graph orientation:
         *
         * component item -> immediate parent item
         */
        Dictionary<int, HashSet<int>>
            parentItemsByComponent =
                itemIds.ToDictionary(
                    itemId => itemId,
                    _ => new HashSet<int>());

        Dictionary<int, HashSet<int>>
            componentItemsByParent =
                itemIds.ToDictionary(
                    itemId => itemId,
                    _ => new HashSet<int>());

        BuildGraph(
            supplyChain,
            itemIdSet,
            parentItemsByComponent,
            componentItemsByParent,
            errors,
            warnings);

        int relationshipCount =
            parentItemsByComponent.Values.Sum(
                parentIds => parentIds.Count);

        int[] rootItemIds =
            itemIds
                .Where(
                    itemId =>
                        parentItemsByComponent[itemId]
                            .Count == 0)
                .ToArray();

        int[] leafItemIds =
            itemIds
                .Where(
                    itemId =>
                        componentItemsByParent[itemId]
                            .Count == 0)
                .ToArray();

        int[] isolatedItemIds =
            itemIds
                .Where(
                    itemId =>
                        parentItemsByComponent[itemId]
                            .Count == 0 &&
                        componentItemsByParent[itemId]
                            .Count == 0)
                .ToArray();

        int[] sharedComponentItemIds =
            itemIds
                .Where(
                    itemId =>
                        parentItemsByComponent[itemId]
                            .Count > 1)
                .ToArray();

        int maximumImmediateComponentCount =
            itemIds.Length == 0
                ? 0
                : itemIds.Max(
                    itemId =>
                        componentItemsByParent[itemId]
                            .Count);

        int maximumImmediateParentCount =
            itemIds.Length == 0
                ? 0
                : itemIds.Max(
                    itemId =>
                        parentItemsByComponent[itemId]
                            .Count);

        int connectedComponentCount =
            CountConnectedComponents(
                itemIds,
                parentItemsByComponent,
                componentItemsByParent);

        int[] cyclicItemIds =
            FindCyclicItemIds(
                itemIds,
                parentItemsByComponent);

        if (cyclicItemIds.Length > 0)
        {
            errors.Add(
                "The bill-of-materials graph contains at " +
                "least one directed cycle involving item " +
                "identifiers: " +
                string.Join(", ", cyclicItemIds) +
                ".");
        }

        int maximumDepth =
            cyclicItemIds.Length == 0
                ? CalculateMaximumDepth(
                    itemIds,
                    parentItemsByComponent,
                    componentItemsByParent)
                : 0;

        ProductStructureType detectedType =
            DetermineStructureType(
                relationshipCount,
                maximumImmediateComponentCount,
                maximumImmediateParentCount,
                errors,
                cyclicItemIds);

        return new ProductStructureAnalysis(
            detectedType:
                detectedType,

            itemCount:
                itemIds.Length,

            relationshipCount:
                relationshipCount,

            connectedComponentCount:
                connectedComponentCount,

            maximumDepth:
                maximumDepth,

            maximumImmediateComponentCount:
                maximumImmediateComponentCount,

            maximumImmediateParentCount:
                maximumImmediateParentCount,

            rootItemIds:
                rootItemIds,

            leafItemIds:
                leafItemIds,

            isolatedItemIds:
                isolatedItemIds,

            sharedComponentItemIds:
                sharedComponentItemIds,

            cyclicItemIds:
                cyclicItemIds,

            errors:
                errors,

            warnings:
                warnings);
    }

    /// <summary>
    /// Analyzes a supply-chain product structure and applies
    /// the result to a persistent descriptor.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance to analyze.
    /// </param>
    /// <param name="descriptor">
    /// Descriptor to update with the analysis result.
    /// </param>
    /// <param name="supplyChainFingerprint">
    /// Optional fingerprint of the analyzed supply chain.
    /// </param>
    /// <returns>
    /// Detailed product-structure analysis.
    /// </returns>
    public static ProductStructureAnalysis AnalyzeAndUpdate(
        SupplyChain supplyChain,
        ProductStructureDescriptor descriptor,
        string supplyChainFingerprint = "")
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(descriptor);

        ProductStructureAnalysis analysis =
            Analyze(supplyChain);

        ApplyAnalysis(
            analysis,
            descriptor,
            supplyChainFingerprint);

        return analysis;
    }

    /// <summary>
    /// Applies an existing analysis result to a persistent
    /// product-structure descriptor.
    /// </summary>
    /// <param name="analysis">
    /// Analysis result to apply.
    /// </param>
    /// <param name="descriptor">
    /// Descriptor to update.
    /// </param>
    /// <param name="supplyChainFingerprint">
    /// Optional fingerprint of the analyzed supply chain.
    /// </param>
    public static void ApplyAnalysis(
        ProductStructureAnalysis analysis,
        ProductStructureDescriptor descriptor,
        string supplyChainFingerprint = "")
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(descriptor);

        descriptor.DetectedType =
            analysis.DetectedType;

        descriptor.HasCycle =
            analysis.HasCycle;

        descriptor.MaximumDepth =
            analysis.MaximumDepth;

        descriptor.ReplaceAnalyzedItemSets(
            analysis.RootItemIds,
            analysis.LeafItemIds,
            analysis.SharedComponentItemIds);

        descriptor.AnalyzedAtUtc =
            DateTime.UtcNow;

        descriptor.AnalyzerVersion =
            CurrentVersion;

        descriptor.SupplyChainFingerprint =
            supplyChainFingerprint?.Trim() ??
            string.Empty;

        descriptor.AnalysisComment =
            BuildAnalysisComment(analysis);

        descriptor.CheckStatus =
            DetermineCheckStatus(
                descriptor.DeclaredType,
                analysis);
    }

    private static void ValidateItemIdentifiers(
        SupplyChain supplyChain,
        ICollection<string> errors)
    {
        for (int index = 0;
             index < supplyChain.Items.Count;
             index++)
        {
            int itemId =
                supplyChain.Items[index].Id;

            if (itemId <= 0)
            {
                errors.Add(
                    $"Item at index {index} has an invalid " +
                    $"identifier ({itemId}).");
            }
        }

        int[] duplicateItemIds =
            supplyChain.Items
                .GroupBy(item => item.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(itemId => itemId)
                .ToArray();

        foreach (int duplicateItemId
                 in duplicateItemIds)
        {
            errors.Add(
                $"Item identifier {duplicateItemId} " +
                "is duplicated.");
        }
    }

    private static void BuildGraph(
        SupplyChain supplyChain,
        IReadOnlySet<int> itemIds,
        IDictionary<int, HashSet<int>>
            parentItemsByComponent,
        IDictionary<int, HashSet<int>>
            componentItemsByParent,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        for (int index = 0;
             index <
             supplyChain.ComponentRequirements.Count;
             index++)
        {
            var requirement =
                supplyChain.ComponentRequirements[index];

            int parentItemId =
                requirement.ParentItemId;

            int componentItemId =
                requirement.ComponentItemId;

            string relationshipDescription =
                $"component requirement at index {index}";

            bool hasValidIdentifiers =
                true;

            if (parentItemId <= 0)
            {
                errors.Add(
                    $"The {relationshipDescription} has an " +
                    $"invalid parent-item identifier " +
                    $"({parentItemId}).");

                hasValidIdentifiers = false;
            }
            else if (!itemIds.Contains(parentItemId))
            {
                errors.Add(
                    $"The {relationshipDescription} refers " +
                    $"to unknown parent item " +
                    $"{parentItemId}.");

                hasValidIdentifiers = false;
            }

            if (componentItemId <= 0)
            {
                errors.Add(
                    $"The {relationshipDescription} has an " +
                    $"invalid component-item identifier " +
                    $"({componentItemId}).");

                hasValidIdentifiers = false;
            }
            else if (!itemIds.Contains(componentItemId))
            {
                errors.Add(
                    $"The {relationshipDescription} refers " +
                    $"to unknown component item " +
                    $"{componentItemId}.");

                hasValidIdentifiers = false;
            }

            if (requirement.Quantity <= 0)
            {
                errors.Add(
                    $"The {relationshipDescription} has a " +
                    $"non-positive requirement quantity " +
                    $"({requirement.Quantity}).");
            }

            if (!hasValidIdentifiers)
            {
                continue;
            }

            bool relationshipAdded =
                parentItemsByComponent[
                    componentItemId]
                .Add(parentItemId);

            if (!relationshipAdded)
            {
                warnings.Add(
                    $"The relationship from component " +
                    $"{componentItemId} to parent " +
                    $"{parentItemId} is duplicated.");

                continue;
            }

            componentItemsByParent[
                parentItemId]
                .Add(componentItemId);
        }
    }

    private static int CountConnectedComponents(
        IEnumerable<int> itemIds,
        IReadOnlyDictionary<int, HashSet<int>>
            parentItemsByComponent,
        IReadOnlyDictionary<int, HashSet<int>>
            componentItemsByParent)
    {
        var visited =
            new HashSet<int>();

        int connectedComponentCount =
            0;

        foreach (int startItemId
                 in itemIds)
        {
            if (!visited.Add(startItemId))
            {
                continue;
            }

            connectedComponentCount++;

            var pendingItemIds =
                new Stack<int>();

            pendingItemIds.Push(
                startItemId);

            while (pendingItemIds.Count > 0)
            {
                int currentItemId =
                    pendingItemIds.Pop();

                IEnumerable<int> adjacentItemIds =
                    parentItemsByComponent[
                        currentItemId]
                    .Concat(
                        componentItemsByParent[
                            currentItemId]);

                foreach (int adjacentItemId
                         in adjacentItemIds)
                {
                    if (visited.Add(
                            adjacentItemId))
                    {
                        pendingItemIds.Push(
                            adjacentItemId);
                    }
                }
            }
        }

        return connectedComponentCount;
    }

    private static int[] FindCyclicItemIds(
        IEnumerable<int> itemIds,
        IReadOnlyDictionary<int, HashSet<int>>
            parentItemsByComponent)
    {
        /*
         * Tarjan's strongly connected component algorithm.
         *
         * An item belongs to a cycle when:
         * - its strongly connected component contains
         *   several items; or
         * - it has a self-loop.
         */
        int nextIndex =
            0;

        var indices =
            new Dictionary<int, int>();

        var lowLinks =
            new Dictionary<int, int>();

        var stack =
            new Stack<int>();

        var itemsOnStack =
            new HashSet<int>();

        var cyclicItemIds =
            new HashSet<int>();

        void Visit(int itemId)
        {
            indices[itemId] =
                nextIndex;

            lowLinks[itemId] =
                nextIndex;

            nextIndex++;

            stack.Push(itemId);
            itemsOnStack.Add(itemId);

            foreach (int parentItemId
                     in parentItemsByComponent[itemId])
            {
                if (!indices.ContainsKey(
                        parentItemId))
                {
                    Visit(parentItemId);

                    lowLinks[itemId] =
                        Math.Min(
                            lowLinks[itemId],
                            lowLinks[parentItemId]);
                }
                else if (itemsOnStack.Contains(
                             parentItemId))
                {
                    lowLinks[itemId] =
                        Math.Min(
                            lowLinks[itemId],
                            indices[parentItemId]);
                }
            }

            if (lowLinks[itemId] !=
                indices[itemId])
            {
                return;
            }

            var stronglyConnectedComponent =
                new List<int>();

            while (stack.Count > 0)
            {
                int currentItemId =
                    stack.Pop();

                itemsOnStack.Remove(
                    currentItemId);

                stronglyConnectedComponent.Add(
                    currentItemId);

                if (currentItemId == itemId)
                {
                    break;
                }
            }

            bool isCycle =
                stronglyConnectedComponent.Count > 1;

            if (!isCycle &&
                stronglyConnectedComponent.Count == 1)
            {
                int singleItemId =
                    stronglyConnectedComponent[0];

                isCycle =
                    parentItemsByComponent[
                        singleItemId]
                    .Contains(singleItemId);
            }

            if (!isCycle)
            {
                return;
            }

            foreach (int cyclicItemId
                     in stronglyConnectedComponent)
            {
                cyclicItemIds.Add(
                    cyclicItemId);
            }
        }

        foreach (int itemId
                 in itemIds.OrderBy(
                     currentItemId =>
                         currentItemId))
        {
            if (!indices.ContainsKey(itemId))
            {
                Visit(itemId);
            }
        }

        return cyclicItemIds
            .OrderBy(itemId => itemId)
            .ToArray();
    }

    private static int CalculateMaximumDepth(
        IEnumerable<int> itemIds,
        IReadOnlyDictionary<int, HashSet<int>>
            parentItemsByComponent,
        IReadOnlyDictionary<int, HashSet<int>>
            componentItemsByParent)
    {
        /*
         * Kahn topological traversal.
         *
         * Because arcs go from components to parents,
         * items without components form the initial queue.
         */
        int[] normalizedItemIds =
            itemIds.ToArray();

        Dictionary<int, int> remainingComponentCounts =
            normalizedItemIds.ToDictionary(
                itemId => itemId,
                itemId =>
                    componentItemsByParent[itemId]
                        .Count);

        Dictionary<int, int> depths =
            normalizedItemIds.ToDictionary(
                itemId => itemId,
                _ => 0);

        var readyItemIds =
            new SortedSet<int>(
                normalizedItemIds.Where(
                    itemId =>
                        remainingComponentCounts[itemId]
                            == 0));

        int processedItemCount =
            0;

        int maximumDepth =
            0;

        while (readyItemIds.Count > 0)
        {
            int itemId =
                readyItemIds.Min;

            readyItemIds.Remove(itemId);
            processedItemCount++;

            maximumDepth =
                Math.Max(
                    maximumDepth,
                    depths[itemId]);

            foreach (int parentItemId
                     in parentItemsByComponent[itemId])
            {
                depths[parentItemId] =
                    Math.Max(
                        depths[parentItemId],
                        depths[itemId] + 1);

                remainingComponentCounts[
                    parentItemId]--;

                if (remainingComponentCounts[
                        parentItemId] == 0)
                {
                    readyItemIds.Add(
                        parentItemId);
                }
            }
        }

        /*
         * This should only happen if a cycle was not detected
         * before calling this method.
         */
        return processedItemCount ==
               normalizedItemIds.Length
            ? maximumDepth
            : 0;
    }

    private static ProductStructureType
        DetermineStructureType(
            int relationshipCount,
            int maximumImmediateComponentCount,
            int maximumImmediateParentCount,
            IReadOnlyCollection<string> errors,
            IReadOnlyCollection<int> cyclicItemIds)
    {
        if (errors.Count > 0 ||
            cyclicItemIds.Count > 0)
        {
            return ProductStructureType.Unknown;
        }

        if (relationshipCount == 0)
        {
            return
                ProductStructureType.IndependentItems;
        }

        bool hasAssemblyNode =
            maximumImmediateComponentCount > 1;

        bool hasDivergentComponent =
            maximumImmediateParentCount > 1;

        if (!hasAssemblyNode &&
            !hasDivergentComponent)
        {
            return ProductStructureType.Serial;
        }

        if (hasAssemblyNode &&
            !hasDivergentComponent)
        {
            return ProductStructureType.Assembly;
        }

        if (!hasAssemblyNode &&
            hasDivergentComponent)
        {
            return
                ProductStructureType.Arborescent;
        }

        return ProductStructureType.General;
    }

    private static ProductStructureCheckStatus
        DetermineCheckStatus(
            ProductStructureType declaredType,
            ProductStructureAnalysis analysis)
    {
        if (!analysis.IsValid)
        {
            return
                ProductStructureCheckStatus.Invalid;
        }

        if (declaredType ==
            ProductStructureType.Unknown)
        {
            return
                ProductStructureCheckStatus.DetectedOnly;
        }

        if (declaredType ==
            analysis.DetectedType)
        {
            return ProductStructureCheckStatus
                .DeclaredAndConfirmed;
        }

        return ProductStructureCheckStatus
            .DeclaredAndContradicted;
    }

    private static string BuildAnalysisComment(
        ProductStructureAnalysis analysis)
    {
        var sections =
            new List<string>();

        if (analysis.Errors.Count > 0)
        {
            sections.Add(
                "Errors: " +
                string.Join(
                    " | ",
                    analysis.Errors));
        }

        if (analysis.Warnings.Count > 0)
        {
            sections.Add(
                "Warnings: " +
                string.Join(
                    " | ",
                    analysis.Warnings));
        }

        return string.Join(
            Environment.NewLine,
            sections);
    }
}