using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.Building;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.DellaertJeunet.XmlModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Creation;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Converts a deserialized Dellaert–Jeunet XML source object
/// into a complete <see cref="LotSizingInstance"/>.
/// </summary>
/// <remarks>
/// The Dellaert–Jeunet benchmark format describes an abstract
/// single-site multilevel lot-sizing problem. It does not
/// explicitly identify plants, work centers, warehouses or
/// distribution centers.
///
/// The converter therefore creates one synthetic physical
/// environment shared by all imported items:
/// <list type="bullet">
/// <item>
/// <description>one plant;</description>
/// </item>
/// <item>
/// <description>one non-capacitated work center;</description>
/// </item>
/// <item>
/// <description>
/// the warehouse attached to the synthetic plant;
/// </description>
/// </item>
/// <item>
/// <description>one distribution center.</description>
/// </item>
/// </list>
///
/// Source item identifiers remain unchanged when
/// <see cref="InstanceImportOptions.PreserveSourceIdentifiers"/>
/// is enabled.
/// </remarks>
public sealed class DellaertJeunetInstanceConverter
{
    private const int SyntheticPlantId =
        1;

    private const int SyntheticWorkCenterId =
        1;

    private const int SyntheticDistributionCenterId =
        1;

    private const string DefaultCreatedBy =
        "LotSizingDataModel Dellaert–Jeunet importer";

    /// <summary>
    /// Initializes a new converter.
    /// </summary>
    public DellaertJeunetInstanceConverter()
    {
    }

    /// <summary>
    /// Converts a source instance using the recommended
    /// default import options.
    /// </summary>
    /// <param name="source">
    /// Deserialized Dellaert–Jeunet source instance.
    /// </param>
    /// <returns>
    /// Converted lot-sizing instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingInstance Convert(
        DellaertJeunetXmlInstance source)
    {
        return Convert(
            source,
            new DellaertJeunetImportOptions());
    }

    /// <summary>
    /// Converts a source instance using the supplied import
    /// options.
    /// </summary>
    /// <param name="source">
    /// Deserialized Dellaert–Jeunet source instance.
    /// </param>
    /// <param name="options">
    /// Import options.
    /// </param>
    /// <returns>
    /// Converted lot-sizing instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the source data cannot be represented by
    /// the target domain model.
    /// </exception>
    public LotSizingInstance Convert(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            options);

        options.EnsureValid();

        EnsureConvertible(
            source,
            options);

        SupplyChain supplyChain =
            BuildSupplyChain(
                source,
                options);

        ProductStructureType declaredStructureType =
            ParseDeclaredProductStructureType(
                source.BomType,
                options.NormalizeDeclaredBomType);

        LotSizingInstance instance =
            LotSizingInstanceFactory.Create(
                instanceId:
                    BuildInstanceId(
                        source,
                        options),

                supplyChain:
                    supplyChain,

                name:
                    BuildInstanceName(
                        source,
                        options),

                declaredProductStructureType:
                    declaredStructureType,

                analyzeProductStructure:
                    options.AnalyzeProductStructure,

                classifyProblem:
                    options.ClassifyProblem,

                createdBy:
                    BuildCreatedBy(
                        options));

        ApplyMetadata(
            instance,
            source,
            options);

        return instance;
    }

    /// <summary>
    /// Attempts to convert a source instance without throwing
    /// for expected conversion problems.
    /// </summary>
    /// <param name="source">
    /// Deserialized source instance.
    /// </param>
    /// <param name="options">
    /// Import options.
    /// </param>
    /// <param name="instance">
    /// Converted instance when conversion succeeds.
    /// </param>
    /// <param name="diagnostics">
    /// Conversion diagnostics.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when conversion succeeds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryConvert(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        out LotSizingInstance? instance,
        out IReadOnlyList<ImportDiagnostic> diagnostics)
    {
        instance =
            null;

        var mutableDiagnostics =
            new List<ImportDiagnostic>();

        if (source is null)
        {
            mutableDiagnostics.Add(
                ImportDiagnostic.Fatal(
                    "DJ4001",
                    "A Dellaert–Jeunet source instance is " +
                    "required."));

            diagnostics =
                mutableDiagnostics;

            return false;
        }

        if (options is null)
        {
            mutableDiagnostics.Add(
                ImportDiagnostic.Fatal(
                    "DJ4002",
                    "Dellaert–Jeunet import options are " +
                    "required."));

            diagnostics =
                mutableDiagnostics;

            return false;
        }

        try
        {
            instance =
                Convert(
                    source,
                    options);

            if (options.IncludeInformationDiagnostics)
            {
                mutableDiagnostics.Add(
                    ImportDiagnostic.Information(
                        "DJ0007",
                        "The Dellaert–Jeunet source instance " +
                        "was converted successfully."));
            }

            diagnostics =
                mutableDiagnostics;

            return true;
        }
        catch (Exception exception)
            when (
                exception is InvalidOperationException or
                ArgumentException or
                OverflowException)
        {
            ImportDiagnostic diagnostic =
                ImportDiagnostic.FromException(
                    ImportSeverity.Error,
                    "DJ4003",
                    "The Dellaert–Jeunet source instance " +
                    "could not be converted: " +
                    GetInnermostException(exception).Message,
                    exception);

            if (options.IncludeTechnicalDetails)
            {
                diagnostic.TechnicalDetails =
                    BuildExceptionDetails(
                        exception);
            }
            else
            {
                diagnostic.TechnicalDetails =
                    string.Empty;
            }

            mutableDiagnostics.Add(
                diagnostic);

            diagnostics =
                mutableDiagnostics;

            return false;
        }
    }

    private static SupplyChain BuildSupplyChain(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        var builder =
            new SupplyChainModelBuilder(
                source.NumberOfPeriods);

        AddSyntheticPhysicalModel(
            builder);

        Dictionary<int, int> targetItemIds =
            BuildTargetItemIdentifiers(
                source,
                options);

        AddItems(
            builder,
            source,
            options,
            targetItemIds);

        AddComponentRequirements(
            builder,
            source,
            targetItemIds);

        AddProductionData(
            builder,
            source,
            targetItemIds);

        AddInventoryData(
            builder,
            source,
            targetItemIds);

        AddDemandData(
            builder,
            source,
            options,
            targetItemIds);

        return builder.Build(
            validate:
                true);
    }

    private static void AddSyntheticPhysicalModel(
        SupplyChainModelBuilder builder)
    {
        var plantWarehouse =
            new PlantWarehouse(
                "Dellaert–Jeunet plant warehouse");

        var plant =
            new Plant(
                SyntheticPlantId,
                "Dellaert–Jeunet synthetic plant",
                plantWarehouse);

        builder
            .AddPlant(
                plant)

            .AddWorkCenter(
                SyntheticPlantId,
                new WorkCenter(
                    SyntheticWorkCenterId,
                    "Dellaert–Jeunet production resource"))

            .AddDistributionCenter(
                new DistributionCenter(
                    SyntheticDistributionCenterId,
                    "Dellaert–Jeunet external demand"));
    }

    private static Dictionary<int, int>
        BuildTargetItemIdentifiers(
            DellaertJeunetXmlInstance source,
            DellaertJeunetImportOptions options)
    {
        var result =
            new Dictionary<int, int>();

        if (options.PreserveSourceIdentifiers)
        {
            foreach (
                DellaertJeunetXmlItem item
                in source.Items)
            {
                result.Add(
                    item.Id,
                    item.Id);
            }

            return result;
        }

        int nextIdentifier =
            1;

        foreach (
            DellaertJeunetXmlItem item
            in source.Items.OrderBy(
                item =>
                    item.Id))
        {
            result.Add(
                item.Id,
                nextIdentifier);

            nextIdentifier++;
        }

        return result;
    }

    private static void AddItems(
        SupplyChainModelBuilder builder,
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        IReadOnlyDictionary<int, int> targetItemIds)
    {
        foreach (
            DellaertJeunetXmlItem sourceItem
            in source.Items)
        {
            int targetItemId =
                targetItemIds[sourceItem.Id];

            string name =
                BuildItemName(
                    sourceItem,
                    options);

            int level =
                ConvertDeclaredDepthToLevel(
                    sourceItem.DepthInBom);

            builder.AddItem(
                targetItemId,
                name,
                level);
        }
    }

    private static void AddComponentRequirements(
        SupplyChainModelBuilder builder,
        DellaertJeunetXmlInstance source,
        IReadOnlyDictionary<int, int> targetItemIds)
    {
        foreach (
            DellaertJeunetXmlItem parentItem
            in source.Items)
        {
            int targetParentId =
                targetItemIds[parentItem.Id];

            foreach (
                DellaertJeunetXmlComponent component
                in parentItem.Components)
            {
                int quantity =
                    ConvertComponentQuantity(
                        parentItem.Id,
                        component);

                int targetComponentId =
                    targetItemIds[component.ItemId];

                builder.AddComponentRequirement(
                    targetParentId,
                    targetComponentId,
                    quantity);
            }
        }
    }

    private static void AddProductionData(
        SupplyChainModelBuilder builder,
        DellaertJeunetXmlInstance source,
        IReadOnlyDictionary<int, int> targetItemIds)
    {
        int routingId =
            1;

        foreach (
            DellaertJeunetXmlItem sourceItem
            in source.Items)
        {
            int targetItemId =
                targetItemIds[sourceItem.Id];

            var workCenterReference =
                new WorkCenterReference
                {
                    PlantId =
                        SyntheticPlantId,

                    WorkCenterId =
                        SyntheticWorkCenterId
                };

            var routing =
                new ProductionRouting
                {
                    Id =
                        routingId,

                    ItemId =
                        targetItemId,

                    PlantId =
                        SyntheticPlantId,

                    LeadTime =
                        0
                };

            routing.WorkCenters.Add(
                workCenterReference);

            builder.AddProductionRouting(
                routing);

            var productionCharacteristic =
                new ProductionCharacteristic
                {
                    ItemId =
                        targetItemId,

                    WorkCenter =
                        new WorkCenterReference
                        {
                            PlantId =
                                SyntheticPlantId,

                            WorkCenterId =
                                SyntheticWorkCenterId
                        },

                    FixedSetupCost =
                        new FixedSetupCost(
                            source.NumberOfPeriods,
                            System.Convert.ToDouble(
                                sourceItem.SetupCost,
                                CultureInfo.InvariantCulture)),

                    UnitUsageCost =
                        new UnitUsageCost(
                            source.NumberOfPeriods,
                            System.Convert.ToDouble(
                                sourceItem.ProductionCost,
                                CultureInfo.InvariantCulture))
                };

            builder.AddProductionCharacteristic(
                productionCharacteristic);

            routingId++;
        }
    }

    private static void AddInventoryData(
        SupplyChainModelBuilder builder,
        DellaertJeunetXmlInstance source,
        IReadOnlyDictionary<int, int> targetItemIds)
    {
        foreach (
            DellaertJeunetXmlItem sourceItem
            in source.Items)
        {
            int targetItemId =
                targetItemIds[sourceItem.Id];

            Inventory inventory =
                Inventory.ForPlantWarehouse(
                    targetItemId,
                    SyntheticPlantId,
                    0.0);

            inventory.UnitUsageCost =
                new UnitUsageCost(
                    source.NumberOfPeriods,
                    System.Convert.ToDouble(
                        sourceItem.HoldingCost,
                        CultureInfo.InvariantCulture));

            builder.AddInventory(
                inventory);
        }
    }

    private static void AddDemandData(
        SupplyChainModelBuilder builder,
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options,
        IReadOnlyDictionary<int, int> targetItemIds)
    {
        foreach (
            DellaertJeunetXmlItem sourceItem
            in source.Items)
        {
            IReadOnlyList<int> demandValues =
                sourceItem.BuildDemandSeries(
                    source.NumberOfPeriods,
                    options.ConvertEmptyDemandToZeroSeries);

            bool hasPositiveDemand =
                demandValues.Any(
                    quantity =>
                        quantity > 0);

            if (!hasPositiveDemand)
            {
                continue;
            }

            int targetItemId =
                targetItemIds[sourceItem.Id];

            var demand =
                new Demand(
                    targetItemId,
                    SyntheticDistributionCenterId,
                    planningHorizon:
                        source.NumberOfPeriods);

            for (int period = 1;
                 period <= source.NumberOfPeriods;
                 period++)
            {
                demand.SetQuantity(
                    period,
                    demandValues[period - 1]);
            }

            builder.AddDemand(
                demand);

            builder.AddDistributionCenterSourcing(
                new DistributionCenterSourcing
                {
                    DistributionCenterId =
                        SyntheticDistributionCenterId,

                    ItemId =
                        targetItemId,

                    Warehouse =
                        WarehouseReference.ForPlantWarehouse(
                            SyntheticPlantId)
                });
        }
    }

    private static void ApplyMetadata(
        LotSizingInstance instance,
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        instance.Description =
            BuildDescription(
                source);

        instance.SourceInformation =
            BuildSourceInformation(
                source,
                options);

        instance.Tags.Add(
            "Dellaert-Jeunet");

        instance.Tags.Add(
            "XML import");

        if (source.HasInstanceType)
        {
            instance.Tags.Add(
                source.InstanceType);
        }

        if (source.HasBomType)
        {
            instance.Tags.Add(
                "BOM:" +
                source.BomType);
        }

        instance.Comment =
            "The physical supply-chain resources were " +
            "generated by the importer because the source " +
            "benchmark defines an abstract single-site " +
            "lot-sizing problem. Production, inventory and " +
            "external-demand sourcing are therefore attached " +
            "to the synthetic plant warehouse.";
    }

    private static string BuildInstanceId(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        if (!string.IsNullOrWhiteSpace(
                options.InstanceIdOverride))
        {
            return options.InstanceIdOverride;
        }

        return
            "DJ-" +
            source.Id.ToString(
                CultureInfo.InvariantCulture);
    }

    private static string BuildInstanceName(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        if (!string.IsNullOrWhiteSpace(
                options.InstanceNameOverride))
        {
            return options.InstanceNameOverride;
        }

        if (source.HasName)
        {
            return source.Name;
        }

        return
            "Dellaert–Jeunet instance " +
            source.Id.ToString(
                CultureInfo.InvariantCulture);
    }

    private static string BuildCreatedBy(
        DellaertJeunetImportOptions options)
    {
        if (!string.IsNullOrWhiteSpace(
                options.CreatedByOverride))
        {
            return options.CreatedByOverride;
        }

        return DefaultCreatedBy;
    }

    private static string BuildItemName(
        DellaertJeunetXmlItem sourceItem,
        DellaertJeunetImportOptions options)
    {
        string sourceName =
            sourceItem.Name;

        if (!string.IsNullOrWhiteSpace(
                sourceName))
        {
            return options.NormalizeTextValues
                ? sourceName.Trim()
                : sourceName;
        }

        return
            "Item " +
            sourceItem.Id.ToString(
                CultureInfo.InvariantCulture);
    }

    private static int ConvertDeclaredDepthToLevel(
        int declaredDepth)
    {
        if (declaredDepth <= 0)
        {
            throw new InvalidOperationException(
                "The declared bill-of-material depth must be " +
                "strictly positive.");
        }

        return declaredDepth - 1;
    }

    private static int ConvertComponentQuantity(
        int parentItemId,
        DellaertJeunetXmlComponent component)
    {
        if (component.Quantity <= 0m)
        {
            throw new InvalidOperationException(
                $"Component quantity for relationship " +
                $"{parentItemId}->{component.ItemId} must be " +
                "strictly positive.");
        }

        if (decimal.Truncate(
                component.Quantity) !=
            component.Quantity)
        {
            throw new InvalidOperationException(
                $"Component quantity " +
                $"{component.Quantity.ToString(
                    CultureInfo.InvariantCulture)} for " +
                $"relationship {parentItemId}->" +
                $"{component.ItemId} cannot be represented " +
                "because ComponentRequirement.Quantity is an " +
                "integer.");
        }

        return checked(
            decimal.ToInt32(
                component.Quantity));
    }

    private static ProductStructureType
        ParseDeclaredProductStructureType(
            string value,
            bool normalize)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return ProductStructureType.Unknown;
        }

        string normalizedValue =
            normalize
                ? value.Trim().ToUpperInvariant()
                : value;

        return normalizedValue switch
        {
            "GENERAL" =>
                ProductStructureType.General,

            "SERIAL" =>
                ProductStructureType.Serial,

            "ASSEMBLY" =>
                ProductStructureType.Assembly,

            "ARBORESCENT" =>
                ProductStructureType.Arborescent,

            "TREE" =>
                ProductStructureType.Arborescent,

            "INDEPENDENT" =>
                ProductStructureType.IndependentItems,

            "INDEPENDENTITEMS" =>
                ProductStructureType.IndependentItems,

            _ =>
                ProductStructureType.Unknown
        };
    }

    private static string BuildDescription(
        DellaertJeunetXmlInstance source)
    {
        return
            "Imported Dellaert–Jeunet lot-sizing benchmark " +
            "instance. Source identifier: " +
            source.Id.ToString(
                CultureInfo.InvariantCulture) +
            ". Declared instance type: " +
            (
                source.HasInstanceType
                    ? source.InstanceType
                    : "not specified"
            ) +
            ". Declared BOM type: " +
            (
                source.HasBomType
                    ? source.BomType
                    : "not specified"
            ) +
            ".";
    }

    private static string BuildSourceInformation(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        if (!string.IsNullOrWhiteSpace(
                options.SourceInformationOverride))
        {
            return options.SourceInformationOverride;
        }

        var parts =
            new List<string>
            {
                "Format: Dellaert–Jeunet XML",
                "Source instance ID: " +
                source.Id.ToString(
                    CultureInfo.InvariantCulture),
                "Source instance name: " +
                source.Name
            };

        if (!string.IsNullOrWhiteSpace(
                options.SourceName))
        {
            parts.Add(
                "Source: " +
                options.SourceName);
        }

        if (options.PreserveBibliographicMetadata &&
            source.Article is not null &&
            source.Article.HasBibliographicData)
        {
            string citation =
                source.Article.BuildCitation();

            if (!string.IsNullOrWhiteSpace(
                    citation))
            {
                parts.Add(
                    "Reference: " +
                    citation);
            }
        }

        return string.Join(
            Environment.NewLine,
            parts);
    }

    private static void EnsureConvertible(
        DellaertJeunetXmlInstance source,
        DellaertJeunetImportOptions options)
    {
        if (source.NumberOfPeriods <= 0)
        {
            throw new InvalidOperationException(
                "The source planning horizon must be " +
                "strictly positive.");
        }

        if (source.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "The source instance does not contain any " +
                "item.");
        }

        if (source.Items.Any(
                item =>
                    item is null))
        {
            throw new InvalidOperationException(
                "The source item collection contains a null " +
                "entry.");
        }

        if (source.HasDuplicateItemIdentifiers)
        {
            throw new InvalidOperationException(
                "Source item identifiers must be unique.");
        }

        HashSet<int> itemIds =
            source.Items
                .Select(
                    item =>
                        item.Id)
                .ToHashSet();

        foreach (
            DellaertJeunetXmlItem item
            in source.Items)
        {
            if (item.Id <= 0)
            {
                throw new InvalidOperationException(
                    "All source item identifiers must be " +
                    "strictly positive.");
            }

            if (!item.HasCompatibleDemandLength(
                    source.NumberOfPeriods,
                    allowEmptyDemand:
                        options
                            .ConvertEmptyDemandToZeroSeries))
            {
                throw new InvalidOperationException(
                    $"Item {item.Id} has a demand-series " +
                    "length incompatible with the planning " +
                    "horizon.");
            }

            foreach (
                DellaertJeunetXmlComponent component
                in item.Components)
            {
                if (component is null)
                {
                    throw new InvalidOperationException(
                        $"Item {item.Id} contains a null " +
                        "component relationship.");
                }

                if (!itemIds.Contains(
                        component.ItemId))
                {
                    throw new InvalidOperationException(
                        $"Item {item.Id} references missing " +
                        $"component item {component.ItemId}.");
                }

                _ =
                    ConvertComponentQuantity(
                        item.Id,
                        component);
            }
        }
    }

    private static string BuildExceptionDetails(
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(
            exception);

        var lines =
            new List<string>();

        Exception? currentException =
            exception;

        int level =
            0;

        while (currentException is not null)
        {
            lines.Add(
                $"Exception level {level}: " +
                currentException.GetType().FullName);

            lines.Add(
                $"Message: {currentException.Message}");

            if (!string.IsNullOrWhiteSpace(
                    currentException.StackTrace))
            {
                lines.Add(
                    currentException.StackTrace);
            }

            currentException =
                currentException.InnerException;

            level++;
        }

        return string.Join(
            Environment.NewLine,
            lines);
    }

    private static Exception GetInnermostException(
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(
            exception);

        Exception currentException =
            exception;

        while (currentException.InnerException is not null)
        {
            currentException =
                currentException.InnerException;
        }

        return currentException;
    }

}