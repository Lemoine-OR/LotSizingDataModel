using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.DellaertJeunet.XmlModel;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Validates a Dellaert–Jeunet XML source instance before its
/// conversion into the LotSizingDataModel domain model.
/// </summary>
/// <remarks>
/// This validator operates exclusively on source-format data
/// transfer objects.
///
/// It checks:
/// <list type="bullet">
/// <item>
/// <description>
/// root metadata and planning-horizon consistency;
/// </description>
/// </item>
/// <item>
/// <description>
/// item identifiers, names, costs and demand series;
/// </description>
/// </item>
/// <item>
/// <description>
/// component quantities and references;
/// </description>
/// </item>
/// <item>
/// <description>
/// duplicate and self-referencing relationships;
/// </description>
/// </item>
/// <item>
/// <description>
/// identifier ordering and contiguity;
/// </description>
/// </item>
/// <item>
/// <description>
/// product-structure acyclicity;
/// </description>
/// </item>
/// <item>
/// <description>
/// consistency between declared and computed depths.
/// </description>
/// </item>
/// </list>
///
/// The validator does not modify the source object.
/// </remarks>
public sealed class DellaertJeunetSourceValidator
{
    private const string GenericSourcePath =
        "/Instance";

    /// <summary>
    /// Initializes a new source validator.
    /// </summary>
    public DellaertJeunetSourceValidator()
    {
    }

    /// <summary>
    /// Validates a Dellaert–Jeunet source instance.
    /// </summary>
    /// <param name="source">
    /// Source instance to validate.
    /// </param>
    /// <param name="options">
    /// Import options controlling validation behavior.
    /// </param>
    /// <returns>
    /// Ordered collection of diagnostics.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public IReadOnlyList<ImportDiagnostic> Validate(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            options);

        options.EnsureValid();

        var diagnostics =
            new List<ImportDiagnostic>();

        ValidateRoot(
            source,
            options,
            diagnostics);

        if (ShouldStop(
                diagnostics,
                options))
        {
            return FinalizeDiagnostics(
                diagnostics,
                options);
        }

        ValidateBibliographicMetadata(
            source,
            options,
            diagnostics);

        if (ShouldStop(
                diagnostics,
                options))
        {
            return FinalizeDiagnostics(
                diagnostics,
                options);
        }

        ValidateItems(
            source,
            options,
            diagnostics);

        if (ShouldStop(
                diagnostics,
                options))
        {
            return FinalizeDiagnostics(
                diagnostics,
                options);
        }

        ValidateReferences(
            source,
            options,
            diagnostics);

        if (ShouldStop(
                diagnostics,
                options))
        {
            return FinalizeDiagnostics(
                diagnostics,
                options);
        }

        ValidateIdentifierSequence(
            source,
            options,
            diagnostics);

        if (ShouldStop(
                diagnostics,
                options))
        {
            return FinalizeDiagnostics(
                diagnostics,
                options);
        }

        ValidateGraph(
            source,
            options,
            diagnostics);

        AddSummaryDiagnostics(
            source,
            options,
            diagnostics);

        return FinalizeDiagnostics(
            diagnostics,
            options);
    }

    private static void ValidateRoot(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        if (!source.HasValidId)
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2001",
                "The source instance identifier must be " +
                "strictly positive.",
                GenericSourcePath + "/ID");
        }

        if (!source.HasName)
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2002",
                "The source instance name is required.",
                GenericSourcePath + "/Name");
        }

        if (!source.HasValidPlanningHorizon)
        {
            Add(
                diagnostics,
                ImportSeverity.Fatal,
                "DJ2003",
                "The number of planning periods must be " +
                "strictly positive.",
                GenericSourcePath + "/NBPeriods");
        }

        if (!source.HasItems)
        {
            Add(
                diagnostics,
                ImportSeverity.Fatal,
                "DJ2004",
                "The source document does not contain any " +
                "item.",
                GenericSourcePath + "/Items");
        }

        if (!source.HasInstanceType)
        {
            Add(
                diagnostics,
                ImportSeverity.Warning,
                "DJ1001",
                "The source instance-size category is " +
                "missing.",
                GenericSourcePath + "/InstanceType");
        }

        if (!source.HasBomType)
        {
            ImportSeverity severity =
                options.VerifyDeclaredBomType
                    ? ImportSeverity.Warning
                    : ImportSeverity.Information;

            Add(
                diagnostics,
                severity,
                "DJ1002",
                "The source bill-of-material type is not " +
                "declared.",
                GenericSourcePath + "/BOMType");
        }
    }

    private static void ValidateBibliographicMetadata(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        if (!source.HasArticle)
        {
            if (options.RequireBibliographicMetadata)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Error,
                    "DJ2101",
                    "Bibliographic metadata is required but " +
                    "the Article element is missing.",
                    GenericSourcePath + "/Article");
            }
            else if (options.PreserveBibliographicMetadata)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Warning,
                    "DJ1101",
                    "No bibliographic metadata is available " +
                    "to preserve.",
                    GenericSourcePath + "/Article");
            }

            return;
        }

        DellaertJeunetXmlArticle article =
            source.Article!;

        if (!article.HasName)
        {
            Add(
                diagnostics,
                options.RequireBibliographicMetadata
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning,
                "DJ1102",
                "The bibliographic article title is missing.",
                GenericSourcePath + "/Article/Name");
        }

        if (!article.HasYear)
        {
            Add(
                diagnostics,
                options.RequireBibliographicMetadata
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning,
                "DJ1103",
                "The bibliographic publication year is " +
                "missing or invalid.",
                GenericSourcePath + "/Article/Year");
        }

        if (article.HasYear &&
            article.Year > DateTime.UtcNow.Year + 1)
        {
            Add(
                diagnostics,
                ImportSeverity.Warning,
                "DJ1104",
                "The bibliographic publication year appears " +
                "to be in the future.",
                GenericSourcePath + "/Article/Year");
        }

        if (article.Authors.Any(
                author =>
                    author is null))
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2102",
                "The bibliographic author collection " +
                "contains a null entry.",
                GenericSourcePath +
                "/Article/Authors");
        }

        for (int index = 0;
             index < article.Authors.Count;
             index++)
        {
            DellaertJeunetXmlAuthor? author =
                article.Authors[index];

            if (author is null)
            {
                continue;
            }

            if (!author.IsValid)
            {
                Add(
                    diagnostics,
                    options.RequireBibliographicMetadata
                        ? ImportSeverity.Error
                        : ImportSeverity.Warning,
                    "DJ1105",
                    "The bibliographic author does not " +
                    "contain a first name or last name.",
                    BuildAuthorPath(index));
            }
        }

        if (options.RequireBibliographicMetadata &&
            article.ValidAuthors.Count == 0)
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2103",
                "At least one valid bibliographic author is " +
                "required.",
                GenericSourcePath +
                "/Article/Authors");
        }
    }

    private static void ValidateItems(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        var identifierOccurrences =
            new Dictionary<int, int>();

        for (int index = 0;
             index < source.Items.Count;
             index++)
        {
            DellaertJeunetXmlItem? item =
                source.Items[index];

            if (item is null)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Error,
                    "DJ2201",
                    "The source item collection contains a " +
                    "null item.",
                    BuildItemPath(index));

                if (ShouldStop(
                        diagnostics,
                        options))
                {
                    return;
                }

                continue;
            }

            ValidateItem(
                item,
                index,
                source.NumberOfPeriods,
                options,
                diagnostics);

            if (identifierOccurrences.TryGetValue(
                    item.Id,
                    out int count))
            {
                identifierOccurrences[item.Id] =
                    count + 1;
            }
            else
            {
                identifierOccurrences[item.Id] =
                    1;
            }

            if (ShouldStop(
                    diagnostics,
                    options))
            {
                return;
            }
        }

        foreach (
            KeyValuePair<int, int> occurrence
            in identifierOccurrences
                .Where(
                    pair =>
                        pair.Value > 1)
                .OrderBy(
                    pair =>
                        pair.Key))
        {
            ImportSeverity severity =
                options.RejectDuplicateIdentifiers
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning;

            Add(
                diagnostics,
                severity,
                "DJ2202",
                $"Item identifier {occurrence.Key} occurs " +
                $"{occurrence.Value} times.",
                GenericSourcePath + "/Items",
                occurrence.Key.ToString(
                    CultureInfo.InvariantCulture));
        }
    }

    private static void ValidateItem(
        DellaertJeunetXmlItem item,
        int index,
        int numberOfPeriods,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        string itemPath =
            BuildItemPath(index);

        string entityKey =
            item.Id.ToString(
                CultureInfo.InvariantCulture);

        if (!item.HasValidId)
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2203",
                "The item identifier must be strictly " +
                "positive.",
                itemPath + "/Id",
                entityKey);
        }

        if (!item.HasName)
        {
            ImportSeverity severity =
                options.RequireNonEmptyItemNames
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning;

            Add(
                diagnostics,
                severity,
                "DJ1201",
                "The item name is empty.",
                itemPath + "/Name",
                entityKey);
        }

        if (!item.HasValidDeclaredDepth)
        {
            Add(
                diagnostics,
                options.VerifyDeclaredDepth
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning,
                "DJ1202",
                "The declared bill-of-material depth must be " +
                "strictly positive.",
                itemPath + "/DepthInBOM",
                entityKey);
        }

        ValidateDemand(
            item,
            itemPath,
            entityKey,
            numberOfPeriods,
            options,
            diagnostics);

        ValidateCosts(
            item,
            itemPath,
            entityKey,
            options,
            diagnostics);

        ValidateComponents(
            item,
            itemPath,
            entityKey,
            options,
            diagnostics);
    }

    private static void ValidateDemand(
        DellaertJeunetXmlItem item,
        string itemPath,
        string entityKey,
        int numberOfPeriods,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        if (item.HasNegativeDemand &&
            options.RejectNegativeValues)
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2301",
                "The external-demand series contains at " +
                "least one negative value.",
                itemPath + "/Demand",
                entityKey);
        }

        if (item.HasEmptyDemand)
        {
            if (options.ConvertEmptyDemandToZeroSeries)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Information,
                    "DJ0301",
                    "The empty external-demand series will be " +
                    "converted into a zero-filled series.",
                    itemPath + "/Demand",
                    entityKey);
            }
            else if (options.RejectInvalidTimeSeriesLength)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Error,
                    "DJ2302",
                    "The item contains an empty external-demand " +
                    "series and automatic zero filling is " +
                    "disabled.",
                    itemPath + "/Demand",
                    entityKey);
            }

            return;
        }

        if (numberOfPeriods <= 0)
        {
            return;
        }

        if (item.DemandValueCount != numberOfPeriods)
        {
            ImportSeverity severity =
                options.RejectInvalidTimeSeriesLength
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning;

            Add(
                diagnostics,
                severity,
                "DJ2303",
                $"The demand series contains " +
                $"{item.DemandValueCount} value(s), while " +
                $"{numberOfPeriods} were expected.",
                itemPath + "/Demand",
                entityKey);
        }
    }

    private static void ValidateCosts(
        DellaertJeunetXmlItem item,
        string itemPath,
        string entityKey,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        ImportSeverity negativeValueSeverity =
            options.RejectNegativeValues
                ? ImportSeverity.Error
                : ImportSeverity.Warning;

        if (item.HasNegativeSetupCost)
        {
            Add(
                diagnostics,
                negativeValueSeverity,
                "DJ2401",
                "The setup cost is negative.",
                itemPath + "/SetupCost",
                entityKey);
        }

        if (item.HasNegativeHoldingCost)
        {
            Add(
                diagnostics,
                negativeValueSeverity,
                "DJ2402",
                "The holding cost is negative.",
                itemPath + "/HoldingCost",
                entityKey);
        }

        if (item.HasNegativeProductionCost)
        {
            Add(
                diagnostics,
                negativeValueSeverity,
                "DJ2403",
                "The production cost is negative.",
                itemPath + "/ProductionCost",
                entityKey);
        }
    }

    private static void ValidateComponents(
        DellaertJeunetXmlItem item,
        string itemPath,
        string entityKey,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        var componentOccurrences =
            new Dictionary<int, int>();

        for (int componentIndex = 0;
             componentIndex < item.Components.Count;
             componentIndex++)
        {
            DellaertJeunetXmlComponent? component =
                item.Components[componentIndex];

            string componentPath =
                itemPath +
                "/ListOfComponents/Component[" +
                (componentIndex + 1)
                    .ToString(
                        CultureInfo.InvariantCulture) +
                "]";

            if (component is null)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Error,
                    "DJ2501",
                    "The component collection contains a null " +
                    "relationship.",
                    componentPath,
                    entityKey);

                continue;
            }

            if (!component.HasItemId)
            {
                Add(
                    diagnostics,
                    ImportSeverity.Error,
                    "DJ2502",
                    "The component item identifier must be " +
                    "strictly positive.",
                    componentPath + "/ID",
                    entityKey);
            }

            if (!component.HasPositiveQuantity)
            {
                ImportSeverity severity =
                    options.RequirePositiveComponentQuantities
                        ? ImportSeverity.Error
                        : ImportSeverity.Warning;

                Add(
                    diagnostics,
                    severity,
                    "DJ2503",
                    "The component quantity must be strictly " +
                    "positive.",
                    componentPath + "/Quantity",
                    entityKey);
            }

            if (component.IsSelfReference(
                    item.Id))
            {
                ImportSeverity severity =
                    options.RejectSelfReferences
                        ? ImportSeverity.Error
                        : ImportSeverity.Warning;

                Add(
                    diagnostics,
                    severity,
                    "DJ2504",
                    "The item references itself as a " +
                    "component.",
                    componentPath,
                    entityKey);
            }

            if (componentOccurrences.TryGetValue(
                    component.ItemId,
                    out int count))
            {
                componentOccurrences[component.ItemId] =
                    count + 1;
            }
            else
            {
                componentOccurrences[component.ItemId] =
                    1;
            }
        }

        foreach (
            KeyValuePair<int, int> occurrence
            in componentOccurrences
                .Where(
                    pair =>
                        pair.Value > 1)
                .OrderBy(
                    pair =>
                        pair.Key))
        {
            ImportSeverity severity =
                options
                    .RejectDuplicateComponentRelationships
                    ? ImportSeverity.Error
                    : ImportSeverity.Warning;

            Add(
                diagnostics,
                severity,
                "DJ2505",
                $"Component item {occurrence.Key} is declared " +
                $"{occurrence.Value} times for parent item " +
                $"{item.Id}.",
                itemPath + "/ListOfComponents",
                entityKey);
        }

        if (item.IsLeaf &&
            !options.TreatEmptyComponentListAsLeaf)
        {
            Add(
                diagnostics,
                ImportSeverity.Warning,
                "DJ1501",
                "The item has an empty component list, but " +
                "empty lists are not configured to identify " +
                "leaf items.",
                itemPath + "/ListOfComponents",
                entityKey);
        }
    }

    private static void ValidateReferences(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        HashSet<int> identifiers =
            source.Items
                .Where(
                    item =>
                        item is not null)
                .Select(
                    item =>
                        item.Id)
                .ToHashSet();

        for (int itemIndex = 0;
             itemIndex < source.Items.Count;
             itemIndex++)
        {
            DellaertJeunetXmlItem? item =
                source.Items[itemIndex];

            if (item is null)
            {
                continue;
            }

            for (int componentIndex = 0;
                 componentIndex <
                 item.Components.Count;
                 componentIndex++)
            {
                DellaertJeunetXmlComponent? component =
                    item.Components[componentIndex];

                if (component is null ||
                    identifiers.Contains(
                        component.ItemId))
                {
                    continue;
                }

                ImportSeverity severity =
                    options.RejectMissingReferences
                        ? ImportSeverity.Error
                        : ImportSeverity.Warning;

                Add(
                    diagnostics,
                    severity,
                    "DJ2601",
                    $"Parent item {item.Id} references missing " +
                    $"component item {component.ItemId}.",
                    BuildComponentPath(
                        itemIndex,
                        componentIndex),
                    item.Id.ToString(
                        CultureInfo.InvariantCulture));

                if (ShouldStop(
                        diagnostics,
                        options))
                {
                    return;
                }
            }
        }
    }

    private static void ValidateIdentifierSequence(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        if (options.RequireContiguousItemIdentifiers &&
            !source.HasContiguousItemIdentifiers)
        {
            Add(
                diagnostics,
                ImportSeverity.Error,
                "DJ2701",
                "Item identifiers do not form a contiguous " +
                "positive integer sequence.",
                GenericSourcePath + "/Items");
        }

        if (options.VerifyItemIdentifierOrder &&
            !source.AreItemsOrderedByIdentifier)
        {
            Add(
                diagnostics,
                ImportSeverity.Warning,
                "DJ1701",
                "Items are not physically ordered by ascending " +
                "identifier.",
                GenericSourcePath + "/Items");
        }
    }

    private static void ValidateGraph(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        Dictionary<int, DellaertJeunetXmlItem> itemsById =
            source.Items
                .Where(
                    item =>
                        item is not null)
                .GroupBy(
                    item =>
                        item.Id)
                .ToDictionary(
                    group =>
                        group.Key,
                    group =>
                        group.First());

        if (itemsById.Count == 0)
        {
            return;
        }

        GraphAnalysisResult graphAnalysis =
            AnalyzeGraph(
                itemsById);

        if (options.VerifyAcyclicProductStructure &&
            graphAnalysis.HasCycle)
        {
            Add(
                diagnostics,
                ImportSeverity.Fatal,
                "DJ2801",
                "The product structure contains at least one " +
                "directed cycle.",
                GenericSourcePath + "/Items",
                graphAnalysis.CycleDescription);
        }

        if (!options.VerifyDeclaredDepth ||
            graphAnalysis.HasCycle)
        {
            return;
        }

        foreach (
            KeyValuePair<int, int> computedDepth
            in graphAnalysis.ComputedDepths
                .OrderBy(
                    pair =>
                        pair.Key))
        {
            if (!itemsById.TryGetValue(
                    computedDepth.Key,
                    out DellaertJeunetXmlItem? item))
            {
                continue;
            }

            if (item.DepthInBom ==
                computedDepth.Value)
            {
                continue;
            }

            Add(
                diagnostics,
                ImportSeverity.Warning,
                "DJ1801",
                $"Item {item.Id} declares depth " +
                $"{item.DepthInBom}, while graph analysis " +
                $"computes depth {computedDepth.Value}.",
                GenericSourcePath +
                "/Items/Item[Id=" +
                item.Id.ToString(
                    CultureInfo.InvariantCulture) +
                "]/DepthInBOM",
                item.Id.ToString(
                    CultureInfo.InvariantCulture));
        }
    }

    private static GraphAnalysisResult AnalyzeGraph(
        IReadOnlyDictionary<
            int,
            DellaertJeunetXmlItem> itemsById)
    {
        var stateByItemId =
            itemsById.Keys.ToDictionary(
                itemId =>
                    itemId,
                _ =>
                    VisitState.NotVisited);

        var recursionStack =
            new List<int>();

        var cycle =
            new List<int>();

        foreach (int itemId in itemsById.Keys)
        {
            if (stateByItemId[itemId] !=
                VisitState.NotVisited)
            {
                continue;
            }

            if (VisitForCycle(
                    itemId,
                    itemsById,
                    stateByItemId,
                    recursionStack,
                    cycle))
            {
                return new GraphAnalysisResult(
                    hasCycle:
                        true,
                    cycleDescription:
                        string.Join(
                            " -> ",
                            cycle.Select(
                                value =>
                                    value.ToString(
                                        CultureInfo
                                            .InvariantCulture))),
                    computedDepths:
                        new Dictionary<int, int>());
            }
        }

        Dictionary<int, int> parentCounts =
            itemsById.Keys.ToDictionary(
                itemId =>
                    itemId,
                _ =>
                    0);

        foreach (DellaertJeunetXmlItem item
                 in itemsById.Values)
        {
            foreach (
                DellaertJeunetXmlComponent component
                in item.Components.Where(
                    component =>
                        component is not null))
            {
                if (parentCounts.ContainsKey(
                        component.ItemId))
                {
                    parentCounts[component.ItemId]++;
                }
            }
        }

        int[] rootItemIds =
            parentCounts
                .Where(
                    pair =>
                        pair.Value == 0)
                .Select(
                    pair =>
                        pair.Key)
                .ToArray();

        var computedDepths =
            itemsById.Keys.ToDictionary(
                itemId =>
                    itemId,
                _ =>
                    0);

        var queue =
            new Queue<int>();

        foreach (int rootItemId
                 in rootItemIds)
        {
            computedDepths[rootItemId] =
                1;

            queue.Enqueue(
                rootItemId);
        }

        while (queue.Count > 0)
        {
            int parentItemId =
                queue.Dequeue();

            int parentDepth =
                computedDepths[parentItemId];

            foreach (
                DellaertJeunetXmlComponent component
                in itemsById[parentItemId]
                    .Components
                    .Where(
                        component =>
                            component is not null))
            {
                if (!computedDepths.ContainsKey(
                        component.ItemId))
                {
                    continue;
                }

                int candidateDepth =
                    parentDepth + 1;

                if (candidateDepth <=
                    computedDepths[component.ItemId])
                {
                    continue;
                }

                computedDepths[component.ItemId] =
                    candidateDepth;

                queue.Enqueue(
                    component.ItemId);
            }
        }

        foreach (int itemId in itemsById.Keys)
        {
            if (computedDepths[itemId] == 0)
            {
                computedDepths[itemId] =
                    1;
            }
        }

        return new GraphAnalysisResult(
            hasCycle:
                false,
            cycleDescription:
                string.Empty,
            computedDepths:
                computedDepths);
    }

    private static bool VisitForCycle(
        int itemId,
        IReadOnlyDictionary<
            int,
            DellaertJeunetXmlItem> itemsById,
        IDictionary<int, VisitState> stateByItemId,
        IList<int> recursionStack,
        IList<int> cycle)
    {
        stateByItemId[itemId] =
            VisitState.Visiting;

        recursionStack.Add(
            itemId);

        foreach (
            DellaertJeunetXmlComponent component
            in itemsById[itemId]
                .Components
                .Where(
                    component =>
                        component is not null))
        {
            if (!itemsById.ContainsKey(
                    component.ItemId))
            {
                continue;
            }

            VisitState componentState =
                stateByItemId[component.ItemId];

            if (componentState ==
                VisitState.NotVisited)
            {
                if (VisitForCycle(
                        component.ItemId,
                        itemsById,
                        stateByItemId,
                        recursionStack,
                        cycle))
                {
                    return true;
                }
            }
            else if (componentState ==
                     VisitState.Visiting)
            {
                int cycleStartIndex =
                    recursionStack.IndexOf(
                        component.ItemId);

                if (cycleStartIndex >= 0)
                {
                    for (int index = cycleStartIndex;
                         index < recursionStack.Count;
                         index++)
                    {
                        cycle.Add(
                            recursionStack[index]);
                    }
                }

                cycle.Add(
                    component.ItemId);

                return true;
            }
        }

        recursionStack.RemoveAt(
            recursionStack.Count - 1);

        stateByItemId[itemId] =
            VisitState.Visited;

        return false;
    }

    private static void AddSummaryDiagnostics(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        ICollection<ImportDiagnostic> diagnostics)
    {
        if (!options.IncludeInformationDiagnostics)
        {
            return;
        }

        Add(
            diagnostics,
            ImportSeverity.Information,
            "DJ0001",
            "Dellaert–Jeunet source validation completed.",
            GenericSourcePath);

        Add(
            diagnostics,
            ImportSeverity.Information,
            "DJ0002",
            $"{source.ItemCount} item(s) were read from the " +
            "source document.",
            GenericSourcePath + "/Items");

        Add(
            diagnostics,
            ImportSeverity.Information,
            "DJ0003",
            $"{source.ComponentRelationshipCount} component " +
            "relationship(s) were read from the source " +
            "document.",
            GenericSourcePath + "/Items");

        Add(
            diagnostics,
            ImportSeverity.Information,
            "DJ0004",
            $"The source planning horizon contains " +
            $"{source.NumberOfPeriods} period(s).",
            GenericSourcePath + "/NBPeriods");

        Add(
            diagnostics,
            ImportSeverity.Information,
            "DJ0005",
            $"{source.ExternallyDemandedItemCount} item(s) " +
            "contain positive external demand.",
            GenericSourcePath + "/Items");
    }

    private static IReadOnlyList<ImportDiagnostic>
        FinalizeDiagnostics(
            IEnumerable<ImportDiagnostic> diagnostics,
            DellaertJeunetImportOptions options)
    {
        IEnumerable<ImportDiagnostic> filteredDiagnostics =
            diagnostics;

        if (!options.IncludeInformationDiagnostics)
        {
            filteredDiagnostics =
                filteredDiagnostics.Where(
                    diagnostic =>
                        !diagnostic.IsInformation);
        }

        return filteredDiagnostics
            .OrderBy(
                diagnostic =>
                    GetSeverityOrder(
                        diagnostic.Severity))
            .ThenBy(
                diagnostic =>
                    diagnostic.Code,
                StringComparer.Ordinal)
            .ThenBy(
                diagnostic =>
                    diagnostic.SourcePath,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static int GetSeverityOrder(
        ImportSeverity severity)
    {
        return severity switch
        {
            ImportSeverity.Fatal =>
                0,

            ImportSeverity.Error =>
                1,

            ImportSeverity.Warning =>
                2,

            ImportSeverity.Information =>
                3,

            _ =>
                4
        };
    }

    private static bool ShouldStop(
        IEnumerable<ImportDiagnostic> diagnostics,
        DellaertJeunetImportOptions options)
    {
        if (!options.StopOnFirstError)
        {
            return false;
        }

        return diagnostics.Any(
            diagnostic =>
                diagnostic.IsBlocking);
    }

    private static void Add(
        ICollection<ImportDiagnostic> diagnostics,
        ImportSeverity severity,
        string code,
        string message,
        string sourcePath,
        string entityKey = "")
    {
        var diagnostic =
            new ImportDiagnostic(
                severity,
                code,
                message,
                sourcePath)
            {
                EntityKey =
                    entityKey
            };

        diagnostics.Add(
            diagnostic);
    }

    private static string BuildItemPath(
        int itemIndex)
    {
        return
            GenericSourcePath +
            "/Items/Item[" +
            (itemIndex + 1)
                .ToString(
                    CultureInfo.InvariantCulture) +
            "]";
    }

    private static string BuildComponentPath(
        int itemIndex,
        int componentIndex)
    {
        return
            BuildItemPath(itemIndex) +
            "/ListOfComponents/Component[" +
            (componentIndex + 1)
                .ToString(
                    CultureInfo.InvariantCulture) +
            "]";
    }

    private static string BuildAuthorPath(
        int authorIndex)
    {
        return
            GenericSourcePath +
            "/Article/Authors/Author[" +
            (authorIndex + 1)
                .ToString(
                    CultureInfo.InvariantCulture) +
            "]";
    }

    private enum VisitState
    {
        NotVisited = 0,

        Visiting = 1,

        Visited = 2
    }

    private sealed class GraphAnalysisResult
    {
        public GraphAnalysisResult(
            bool hasCycle,
            string cycleDescription,
            IReadOnlyDictionary<int, int> computedDepths)
        {
            HasCycle =
                hasCycle;

            CycleDescription =
                cycleDescription;

            ComputedDepths =
                computedDepths;
        }

        public bool HasCycle { get; }

        public string CycleDescription { get; }

        public IReadOnlyDictionary<int, int>
            ComputedDepths
        { get; }
    }
}