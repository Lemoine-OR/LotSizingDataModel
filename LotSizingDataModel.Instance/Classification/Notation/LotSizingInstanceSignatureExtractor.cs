using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Planning;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance.Analysis;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Extracts an LSI/1 semantic signature from a SupplyChain.
/// </summary>
public static class LotSizingInstanceSignatureExtractor
{
    public static LotSizingInstanceSignature Extract(
        SupplyChain supplyChain,
        double numericalTolerance =
            LotSizingProblemFeatureExtractor.DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        ProductStructureAnalysis productStructure =
            ProductStructureAnalyzer.Analyze(supplyChain);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(
                supplyChain,
                productStructure,
                numericalTolerance);

        return Extract(
            supplyChain,
            features,
            productStructure,
            numericalTolerance);
    }

    public static LotSizingInstanceSignature Extract(
        SupplyChain supplyChain,
        LotSizingProblemFeatures features,
        ProductStructureAnalysis productStructure,
        double numericalTolerance =
            LotSizingProblemFeatureExtractor.DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(features);
        ArgumentNullException.ThrowIfNull(productStructure);

        WorkCenter[] workCenters =
            supplyChain.Plants
                .SelectMany(plant => plant.WorkCenters)
                .ToArray();

        return new LotSizingInstanceSignature
        {
            Planning = CreatePlanning(supplyChain, features),
            System = CreateSystem(features),
            Features = CreateFeatures(
                supplyChain,
                features,
                workCenters,
                numericalTolerance),
            Objective = CreateObjective(supplyChain),
            Size = CreateSize(features)
        };
    }

    private static PlanningSignature CreatePlanning(
        SupplyChain supplyChain,
        LotSizingProblemFeatures features)
    {
        return new PlanningSignature
        {
            Horizon =
                features.PlanningHorizon > 0
                    ? PlanningHorizonKind.Finite
                    : PlanningHorizonKind.Unknown,

            TimeModel =
                features.PlanningHorizon > 0
                    ? TimeModelKind.Discrete
                    : TimeModelKind.Unknown,

            BucketStructure =
                MapBucketMode(
                    supplyChain.PlanningContext?.BucketMode ??
                    PlanningBucketMode.Unspecified),

            Information =
                features.HasDeterministicDemand
                    ? InformationStructureKind.Deterministic
                    : InformationStructureKind.Unknown,

            DemandPattern =
                !features.HasDemand
                    ? DemandPatternKind.Unknown
                    : features.HasTimeVaryingDemand
                        ? DemandPatternKind.Dynamic
                        : DemandPatternKind.Stationary,

            DemandSource =
                features.HasDemand
                    ? DemandSourceKind.Exogenous
                    : DemandSourceKind.Unknown
        };
    }

    private static BucketStructureKind MapBucketMode(
        PlanningBucketMode mode)
    {
        return mode switch
        {
            PlanningBucketMode.BigBucket =>
                BucketStructureKind.BigBucket,

            PlanningBucketMode.SmallBucket =>
                BucketStructureKind.SmallBucket,

            PlanningBucketMode.MacroMicro =>
                BucketStructureKind.MacroMicro,

            PlanningBucketMode.Hybrid =>
                BucketStructureKind.Hybrid,

            _ => BucketStructureKind.Unknown
        };
    }

    private static SystemSignature CreateSystem(
        LotSizingProblemFeatures features)
    {
        NetworkStructureKind network;

        if (features.IsMultiSite)
        {
            network = NetworkStructureKind.MultiSite;
        }
        else if (features.SupplierCount > 0 ||
                 features.DistributionCenterCount > 0 ||
                 features.TransportResourceCount > 0)
        {
            network = NetworkStructureKind.SupplyChain;
        }
        else if (features.PlantCount +
                 features.WarehouseCount > 0)
        {
            network = NetworkStructureKind.SingleSite;
        }
        else
        {
            network = NetworkStructureKind.Unknown;
        }

        return new SystemSignature
        {
            Items = Cardinality(features.ItemCount),

            Levels =
                features.ProductStructureRelationshipCount > 0
                    ? CardinalityKind.Multiple
                    : features.ItemCount > 0
                        ? CardinalityKind.Single
                        : CardinalityKind.Unknown,

            ProductStructure = features.ProductStructureType,
            Network = network,
            Routing = RoutingStructureKind.Unknown,

            ResourceEnvironment =
                features.WorkCenterCount == 1
                    ? ResourceEnvironmentKind.SingleResource
                    : features.WorkCenterCount > 1
                        ? ResourceEnvironmentKind.General
                        : ResourceEnvironmentKind.Unknown
        };
    }

    private static FeatureSignature CreateFeatures(
        SupplyChain supplyChain,
        LotSizingProblemFeatures features,
        WorkCenter[] workCenters,
        double numericalTolerance)
    {
        var result = new FeatureSignature();

        Set(result, LsiFeatureCodes.Demand, features.HasDemand);
        Set(result, LsiFeatureCodes.Production, features.HasProduction);

        Set(result, LsiFeatureCodes.ProductionCapacity,
            features.HasProductionCapacityConstraints,
            CombineDoubleProfiles(
                workCenters
                    .Where(workCenter =>
                        workCenter.CapacityConstraint is not null)
                    .Select(workCenter =>
                        workCenter.CapacityConstraint!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.SharedProductionCapacity,
            features.HasSharedProductionCapacity);

        Set(result, LsiFeatureCodes.WarehouseCapacity,
            features.HasWarehouseCapacityConstraints);

        Set(result, LsiFeatureCodes.SupplierCapacity,
            features.HasSupplierCapacityConstraints,
            CombineDoubleProfiles(
                supplyChain.SupplierDeliveries
                    .Where(delivery =>
                        delivery.CapacityConstraint is not null)
                    .Select(delivery =>
                        delivery.CapacityConstraint!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.TransportCapacity,
            features.HasTransportCapacityConstraints);

        Set(result, LsiFeatureCodes.AdditionalProductionCapacity,
            features.HasAdditionalProductionCapacity);

        Set(result, LsiFeatureCodes.AdditionalWarehouseCapacity,
            features.HasAdditionalWarehouseCapacity);

        Set(result, LsiFeatureCodes.AdditionalTransportCapacity,
            features.HasAdditionalTransportCapacity);

        Set(result, LsiFeatureCodes.SetupCost,
            features.HasSetupCosts,
            CombineDoubleProfiles(
                supplyChain.ProductionCharacteristics
                    .Where(characteristic =>
                        characteristic.FixedSetupCost is not null)
                    .Select(characteristic =>
                        characteristic.FixedSetupCost!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.SetupTime,
            features.HasSetupTimes,
            CombineDoubleProfiles(
                supplyChain.ProductionCharacteristics
                    .Where(characteristic =>
                        characteristic.SetupTime is not null)
                    .Select(characteristic =>
                        characteristic.SetupTime!.Values),
                numericalTolerance));

        bool hasCarryOver = workCenters.Any(workCenter =>
            workCenter.SetupTransitionProfile?.CarryOverPolicy ==
                LotSizingDataModel.Core.DecisionModel.Scheduling.SetupCarryOverPolicy.Allowed);

        Set(result, LsiFeatureCodes.SetupCarryOver, hasCarryOver);

        Set(result, LsiFeatureCodes.SequenceDependentSetupTime,
            workCenters.Any(workCenter =>
                workCenter.SetupTransitionProfile?.HasSequenceDependentTimes == true),
            CombineDoubleProfiles(
                workCenters
                    .Where(workCenter => workCenter.SetupTransitionProfile is not null)
                    .SelectMany(workCenter => workCenter.SetupTransitionProfile!.Changeovers)
                    .Where(changeover => changeover.ChangeoverTime is not null)
                    .Select(changeover => changeover.ChangeoverTime!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.SequenceDependentSetupCost,
            workCenters.Any(workCenter =>
                workCenter.SetupTransitionProfile?.HasSequenceDependentCosts == true),
            CombineDoubleProfiles(
                workCenters
                    .Where(workCenter => workCenter.SetupTransitionProfile is not null)
                    .SelectMany(workCenter => workCenter.SetupTransitionProfile!.Changeovers)
                    .Where(changeover => changeover.ChangeoverCost is not null)
                    .Select(changeover => changeover.ChangeoverCost!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.ProductionSetupFamily,
            supplyChain.ProductionSetupFamilies.Count > 0);

        Set(result, LsiFeatureCodes.ProductionSetupFamilyTime,
            supplyChain.ProductionSetupFamilies.Any(
                family => family.SetupTime is not null),
            CombineDoubleProfiles(
                supplyChain.ProductionSetupFamilies
                    .Where(family => family.SetupTime is not null)
                    .Select(family => family.SetupTime!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.StartUpCost,
            features.HasStartUpCosts);

        Set(result, LsiFeatureCodes.MinimumLotSize,
            features.HasMinimumLotSizes,
            CombineDoubleProfiles(
                supplyChain.ProductionRoutings
                    .Where(routing =>
                        routing.MinimumLotSize is not null)
                    .Select(routing =>
                        routing.MinimumLotSize!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.MaximumLotSize,
            features.HasMaximumLotSizes,
            CombineDoubleProfiles(
                supplyChain.ProductionRoutings
                    .Where(routing =>
                        routing.MaximumLotSize is not null)
                    .Select(routing =>
                        routing.MaximumLotSize!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.LotSizeMultiple,
            features.HasLotSizeMultiples,
            CombineDoubleProfiles(
                supplyChain.ProductionRoutings
                    .Where(routing =>
                        routing.LotSizeMultiple is not null)
                    .Select(routing =>
                        routing.LotSizeMultiple!.Values),
                numericalTolerance));

        Set(result, LsiFeatureCodes.GroupingConstraint,
            features.HasGroupingConstraints,
            CombineIntegerProfiles(
                supplyChain.ProductionRoutings
                    .Where(routing =>
                        routing.GroupingConstraint is not null)
                    .Select(routing =>
                        routing.GroupingConstraint!.Values)));

        Set(result, LsiFeatureCodes.ProductionLeadTime,
            features.HasProductionLeadTimes);
        Set(result, LsiFeatureCodes.SupplierLeadTime,
            features.HasSupplierLeadTimes);
        Set(result, LsiFeatureCodes.TransportLeadTime,
            features.HasTransportLeadTimes);

        Set(result, LsiFeatureCodes.InitialInventory,
            features.HasInitialInventory);
        Set(result, LsiFeatureCodes.SafetyStock,
            features.HasSafetyStockRequirements);

        Set(result, LsiFeatureCodes.Backlogging,
            features.HasBacklogging);
        Set(result, LsiFeatureCodes.LostSales,
            features.HasLostSales);

        Set(result, LsiFeatureCodes.Purchasing,
            features.HasPurchasing);
        Set(result, LsiFeatureCodes.Transportation,
            features.HasTransportation);
        Set(result, LsiFeatureCodes.Distribution,
            features.HasDistribution);
        Set(result, LsiFeatureCodes.MultiSite,
            features.IsMultiSite);
        Set(result, LsiFeatureCodes.FinancialConstraints,
            features.HasFinancialConstraints);
        Set(result, LsiFeatureCodes.MultipleObjectives,
            supplyChain.ObjectivePolicy?
                .HasMultipleEnabledCriteria ?? false);

        return result;
    }

    private static ObjectiveSignature CreateObjective(
        SupplyChain supplyChain)
    {
        OptimizationObjectivePolicy? policy =
            supplyChain.ObjectivePolicy;

        if (policy is null)
        {
            return new ObjectiveSignature
            {
                State = FeatureState.Unknown
            };
        }

        var result = new ObjectiveSignature
        {
            State = FeatureState.Present,
            Sense = ObjectiveSenseKind.Unknown,

            Aggregation =
                policy.AggregationMode switch
                {
                    ObjectiveAggregationMode.Single =>
                        ObjectiveAggregationKind.Single,

                    ObjectiveAggregationMode.WeightedSum =>
                        ObjectiveAggregationKind.WeightedSum,

                    ObjectiveAggregationMode.Lexicographic =>
                        ObjectiveAggregationKind.Lexicographic,

                    _ => ObjectiveAggregationKind.Unknown
                }
        };

        result.ReplaceComponents(
            policy.Criteria
                .Where(criterion =>
                    criterion is not null &&
                    criterion.IsEnabled)
                .Select(criterion =>
                    MapObjectiveKind(criterion.Kind)));

        return result;
    }

    private static ObjectiveComponentKind MapObjectiveKind(
        OptimizationObjectiveKind kind)
    {
        return kind switch
        {
            OptimizationObjectiveKind.Economic =>
                ObjectiveComponentKind.Economic,

            OptimizationObjectiveKind.Financial =>
                ObjectiveComponentKind.Financial,

            OptimizationObjectiveKind.Sustainability =>
                ObjectiveComponentKind.Sustainability,

            OptimizationObjectiveKind.ServiceLevel =>
                ObjectiveComponentKind.ServiceLevel,

            _ => ObjectiveComponentKind.Unknown
        };
    }

    private static InstanceSizeSignature CreateSize(
        LotSizingProblemFeatures features)
    {
        return new InstanceSizeSignature
        {
            Periods = features.PlanningHorizon,
            Items = features.ItemCount,
            Plants = features.PlantCount,
            WorkCenters = features.WorkCenterCount,
            Warehouses = features.WarehouseCount,
            Suppliers = features.SupplierCount,
            DistributionCenters =
                features.DistributionCenterCount,
            TransportResources =
                features.TransportResourceCount,
            BomRelationships =
                features.ProductStructureRelationshipCount,
            MaximumBomDepth =
                features.MaximumProductStructureDepth
        };
    }

    private static void Set(
        FeatureSignature signature,
        string code,
        bool present,
        TemporalProfile? profile = null)
    {
        signature.Set(
            code,
            present ? FeatureState.Present : FeatureState.Absent,
            present ? profile : null);
    }

    private static CardinalityKind Cardinality(int value)
    {
        if (value < 0)
        {
            return CardinalityKind.Unknown;
        }

        if (value == 0)
        {
            return CardinalityKind.None;
        }

        return value == 1
            ? CardinalityKind.Single
            : CardinalityKind.Multiple;
    }

    private static TemporalProfile? CombineDoubleProfiles(
        IEnumerable<IEnumerable<double>> series,
        double tolerance)
    {
        TemporalProfile[] profiles =
            series
                .Select(values =>
                    TemporalProfileAnalyzer.Analyze(
                        values,
                        tolerance))
                .ToArray();

        return profiles.Length == 0
            ? null
            : TemporalProfileAnalyzer.Combine(profiles);
    }

    private static TemporalProfile? CombineIntegerProfiles(
        IEnumerable<IEnumerable<int>> series)
    {
        TemporalProfile[] profiles =
            series
                .Select(TemporalProfileAnalyzer.Analyze)
                .ToArray();

        return profiles.Length == 0
            ? null
            : TemporalProfileAnalyzer.Combine(profiles);
    }
}
