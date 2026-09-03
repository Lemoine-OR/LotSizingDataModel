using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance.Analysis;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Extracts factual lot-sizing problem features from a
/// supply-chain instance.
/// </summary>
/// <remarks>
/// This extractor does not assign a known problem-family code.
///
/// It converts the data and optional model components contained
/// in a <see cref="SupplyChain"/> into a feature profile that
/// can subsequently be used by a problem classifier.
/// </remarks>
public static class LotSizingProblemFeatureExtractor
{
    /// <summary>
    /// Default tolerance used when comparing numerical
    /// time-series values.
    /// </summary>
    public const double DefaultNumericalTolerance =
        1e-9;

    /// <summary>
    /// Extracts lot-sizing problem features and automatically
    /// analyzes the product structure.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance to analyze.
    /// </param>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used for numerical
    /// comparisons.
    /// </param>
    /// <returns>
    /// Extracted lot-sizing problem-feature profile.
    /// </returns>
    public static LotSizingProblemFeatures Extract(
        SupplyChain supplyChain,
        double numericalTolerance =
            DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        ValidateTolerance(
            numericalTolerance);

        ProductStructureAnalysis
            productStructureAnalysis =
                ProductStructureAnalyzer.Analyze(
                    supplyChain);

        return Extract(
            supplyChain,
            productStructureAnalysis,
            numericalTolerance);
    }

    /// <summary>
    /// Extracts lot-sizing problem features using an existing
    /// product-structure analysis.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance to analyze.
    /// </param>
    /// <param name="productStructureAnalysis">
    /// Previously calculated product-structure analysis.
    /// </param>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used for numerical
    /// comparisons.
    /// </param>
    /// <returns>
    /// Extracted lot-sizing problem-feature profile.
    /// </returns>
    public static LotSizingProblemFeatures Extract(
        SupplyChain supplyChain,
        ProductStructureAnalysis
            productStructureAnalysis,
        double numericalTolerance =
            DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        ArgumentNullException.ThrowIfNull(
            productStructureAnalysis);

        ValidateTolerance(
            numericalTolerance);

        WorkCenter[] workCenters =
            EnumerateWorkCenters(
                supplyChain)
            .ToArray();

        Warehouse[] warehouses =
            EnumerateWarehouses(
                supplyChain)
            .ToArray();

        bool hasTransportLanes =
            supplyChain.TransportResources.Any(
                resource =>
                    resource.Lanes.Count > 0);

        var features =
            new LotSizingProblemFeatures
            {
                ItemCount =
                    supplyChain.Items.Count,

                PlanningHorizon =
                    supplyChain.PlanningHorizon,

                PlantCount =
                    supplyChain.Plants.Count,

                WorkCenterCount =
                    workCenters.Length,

                WarehouseCount =
                    warehouses.Length,

                SupplierCount =
                    supplyChain.Suppliers.Count,

                DistributionCenterCount =
                    supplyChain
                        .DistributionCenters
                        .Count,

                TransportResourceCount =
                    supplyChain
                        .TransportResources
                        .Count,

                ProductStructureRelationshipCount =
                    productStructureAnalysis
                        .RelationshipCount,

                MaximumProductStructureDepth =
                    productStructureAnalysis
                        .MaximumDepth,

                ProductStructureType =
                    productStructureAnalysis
                        .DetectedType,

                HasDemand =
                    supplyChain.Demands.Count > 0,

                /*
                 * The current Core model stores explicit
                 * deterministic demand quantities.
                 *
                 * It does not currently contain stochastic
                 * demand distributions or scenarios.
                 */
                HasDeterministicDemand =
                    supplyChain.Demands.Count > 0,

                HasTimeVaryingDemand =
                    supplyChain.Demands.Any(
                        demand =>
                            HasTimeVariation(
                                demand.Quantities,
                                numericalTolerance)),

                HasInitialInventory =
                    supplyChain.Inventories.Any(
                        inventory =>
                            inventory.InitialInventory >
                            numericalTolerance),

                HasSafetyStockRequirements =
                    supplyChain.Inventories.Any(
                        inventory =>
                            inventory.SafetyStock is not null),

                HasBacklogging =
                    supplyChain
                        .DistributionCenterSourcings
                        .Any(
                            sourcing =>
                                sourcing.BacklogConstraint
                                    is not null),

                HasLostSales =
                    supplyChain
                        .DistributionCenterSourcings
                        .Any(
                            sourcing =>
                                sourcing.ShortageConstraint
                                    is not null),

                HasProduction =
                    supplyChain
                        .ProductionRoutings
                        .Count > 0 ||
                    supplyChain
                        .ProductionCharacteristics
                        .Count > 0,

                HasProductionCapacityConstraints =
                    workCenters.Any(
                        workCenter =>
                            workCenter.CapacityConstraint
                                is not null),

                HasSharedProductionCapacity =
                    DetectSharedProductionCapacity(
                        supplyChain),

                HasTimeVaryingProductionCapacity =
                    workCenters.Any(
                        workCenter =>
                            workCenter.CapacityConstraint
                                is not null &&
                            HasTimeVariation(
                                workCenter
                                    .CapacityConstraint
                                    .Values,
                                numericalTolerance)),

                HasSetupCosts =
                    supplyChain
                        .ProductionCharacteristics
                        .Any(
                            characteristic =>
                                characteristic.FixedSetupCost
                                    is not null),

                HasSetupTimes =
                    supplyChain
                        .ProductionCharacteristics
                        .Any(
                            characteristic =>
                                characteristic.SetupTime
                                    is not null),

                /*
                 * Start-up costs are not represented by a
                 * dedicated type in the current Core model.
                 */
                HasStartUpCosts =
                    false,

                HasProductionLeadTimes =
                    supplyChain
                        .ProductionRoutings
                        .Any(
                            routing =>
                                routing.LeadTime > 0),

                HasMinimumLotSizes =
                    supplyChain
                        .ProductionRoutings
                        .Any(
                            routing =>
                                routing.MinimumLotSize
                                    is not null),

                /*
                 * The current Core model does not yet contain
                 * a MaximumLotSize parameter.
                 */
                HasMaximumLotSizes =
                    supplyChain.ProductionRoutings.Any(
                        routing => routing.MaximumLotSize is not null),

                HasLotSizeMultiples =
                    supplyChain
                        .ProductionRoutings
                        .Any(
                            routing =>
                                routing.LotSizeMultiple
                                    is not null),

                HasGroupingConstraints =
                    supplyChain.ProductionRoutings.Any(
                        routing => routing.GroupingConstraint is not null),
                HasAdditionalProductionCapacity =
                    workCenters.Any(
                        workCenter =>
                            workCenter.AdditionalCapacity
                                is not null),

                HasPurchasing =
                    supplyChain
                        .SupplierDeliveries
                        .Count > 0,

                /*
                 * Supplier capacity is not represented by
                 * Supplier or SupplierDelivery in the current
                 * Core model.
                 */
                HasSupplierCapacityConstraints =
                    supplyChain.SupplierDeliveries.Any(
                        delivery => delivery.CapacityConstraint is not null),

                HasSupplierLeadTimes =
                    supplyChain
                        .SupplierDeliveries
                        .Any(
                            delivery =>
                                delivery.LeadTime > 0),

                HasTransportation =
                    hasTransportLanes ||
                    supplyChain
                        .TransportCharacteristics
                        .Count > 0,

                HasTransportCapacityConstraints =
                    DetectTransportCapacityConstraints(
                        supplyChain),

                HasTransportLeadTimes =
                    supplyChain
                        .TransportResources
                        .SelectMany(
                            resource =>
                                resource.Lanes)
                        .Any(
                            lane =>
                                lane.LeadTime > 0),

                HasAdditionalTransportCapacity =
                    DetectAdditionalTransportCapacity(
                        supplyChain),

                HasDistribution =
                    supplyChain
                        .DistributionCenterSourcings
                        .Count > 0 ||
                    supplyChain
                        .Demands
                        .Count > 0,

                HasWarehouseCapacityConstraints =
                    DetectWarehouseCapacityConstraints(
                        supplyChain,
                        warehouses),

                HasAdditionalWarehouseCapacity =
                    DetectAdditionalWarehouseCapacity(
                        supplyChain,
                        warehouses),

                /*
                 * Production and storage locations are used
                 * here to characterize the site structure.
                 *
                 * Suppliers and distribution centers are
                 * represented separately by their own flags.
                 */
                IsMultiSite =
                    supplyChain.Plants.Count +
                    supplyChain
                        .StandaloneWarehouses
                        .Count >
                    1,

                /*
                 * These concepts are not represented by
                 * dedicated objects in the current Core model.
                 */
                HasFinancialConstraints =
                    false,

                HasMultipleObjectives =
                    false
            };

        return features;
    }

    private static bool
        DetectSharedProductionCapacity(
            SupplyChain supplyChain)
    {
        return supplyChain
            .ProductionCharacteristics
            .Where(
                characteristic =>
                    characteristic.WorkCenter.PlantId > 0 &&
                    characteristic.WorkCenter.WorkCenterId > 0)
            .GroupBy(
                characteristic =>
                    (
                        characteristic.WorkCenter.PlantId,
                        characteristic.WorkCenter
                            .WorkCenterId
                    ))
            .Any(
                group =>
                    group
                        .Select(
                            characteristic =>
                                characteristic.ItemId)
                        .Distinct()
                        .Count() > 1);
    }

    private static bool
        DetectTransportCapacityConstraints(
            SupplyChain supplyChain)
    {
        bool hasGlobalCapacity =
            supplyChain
                .TransportResources
                .Any(
                    resource =>
                        resource.CapacityConstraint
                            is not null);

        bool hasItemSpecificCapacity =
            supplyChain
                .TransportCharacteristics
                .Any(
                    characteristic =>
                        characteristic.CapacityConstraint
                            is not null);

        return
            hasGlobalCapacity ||
            hasItemSpecificCapacity;
    }

    private static bool
        DetectAdditionalTransportCapacity(
            SupplyChain supplyChain)
    {
        bool hasGlobalAdditionalCapacity =
            supplyChain
                .TransportResources
                .Any(
                    resource =>
                        resource.AdditionalCapacity
                            is not null);

        bool hasItemSpecificAdditionalCapacity =
            supplyChain
                .TransportCharacteristics
                .Any(
                    characteristic =>
                        characteristic.AdditionalCapacity
                            is not null);

        return
            hasGlobalAdditionalCapacity ||
            hasItemSpecificAdditionalCapacity;
    }

    private static bool
        DetectWarehouseCapacityConstraints(
            SupplyChain supplyChain,
            IEnumerable<Warehouse> warehouses)
    {
        bool hasGlobalWarehouseCapacity =
            warehouses.Any(
                warehouse =>
                    warehouse.CapacityConstraint
                        is not null);

        bool hasItemSpecificWarehouseCapacity =
            supplyChain.Inventories.Any(
                inventory =>
                    inventory.CapacityConstraint
                        is not null);

        return
            hasGlobalWarehouseCapacity ||
            hasItemSpecificWarehouseCapacity;
    }

    private static bool
        DetectAdditionalWarehouseCapacity(
            SupplyChain supplyChain,
            IEnumerable<Warehouse> warehouses)
    {
        bool hasGlobalAdditionalCapacity =
            warehouses.Any(
                warehouse =>
                    warehouse.AdditionalCapacity
                        is not null);

        bool hasItemSpecificAdditionalCapacity =
            supplyChain.Inventories.Any(
                inventory =>
                    inventory.AdditionalCapacity
                        is not null);

        return
            hasGlobalAdditionalCapacity ||
            hasItemSpecificAdditionalCapacity;
    }

    private static IEnumerable<WorkCenter>
        EnumerateWorkCenters(
            SupplyChain supplyChain)
    {
        foreach (Plant plant
                 in supplyChain.Plants)
        {
            foreach (WorkCenter workCenter
                     in plant.WorkCenters)
            {
                yield return workCenter;
            }
        }
    }

    private static IEnumerable<Warehouse>
        EnumerateWarehouses(
            SupplyChain supplyChain)
    {
        foreach (Plant plant
                 in supplyChain.Plants)
        {
            yield return plant.Warehouse;
        }

        foreach (StandaloneWarehouse warehouse
                 in supplyChain.StandaloneWarehouses)
        {
            yield return warehouse;
        }
    }

    private static bool HasTimeVariation(
        IEnumerable<double> values,
        double numericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(values);

        using IEnumerator<double> enumerator =
            values.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return false;
        }

        double referenceValue =
            enumerator.Current;

        while (enumerator.MoveNext())
        {
            double currentValue =
                enumerator.Current;

            if (!double.IsFinite(referenceValue) ||
                !double.IsFinite(currentValue))
            {
                /*
                 * Invalid numerical data must not be treated
                 * as a stationary valid series.
                 */
                return true;
            }

            if (Math.Abs(
                    currentValue -
                    referenceValue) >
                numericalTolerance)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateTolerance(
        double numericalTolerance)
    {
        if (!double.IsFinite(
                numericalTolerance) ||
            numericalTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numericalTolerance),
                numericalTolerance,
                "The numerical tolerance must be finite " +
                "and non-negative.");
        }
    }
}