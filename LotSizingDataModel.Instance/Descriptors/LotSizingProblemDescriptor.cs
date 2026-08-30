using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>
/// Typed structured description of the factual lot-sizing problem
/// characteristics currently represented by an instance.
/// </summary>
/// <remarks>
/// Alpha.4 is a lossless non-serialized bridge over the historical
/// LotSizingProblemFeatures vector. The target extraction direction is
/// Core source data -> typed Descriptor -> legacy Features.
/// </remarks>
public sealed class LotSizingProblemDescriptor
{
    public StructureDescriptor Structure { get; init; } = new();
    public TimeDescriptor Time { get; init; } = new();
    public DemandDescriptor Demand { get; init; } = new();
    public ProductionDescriptor Production { get; init; } = new();
    public CapacityDescriptor Capacity { get; init; } = new();
    public SetupDescriptor Setup { get; init; } = new();
    public InventoryServiceDescriptor InventoryService { get; init; } = new();
    public ProcurementDescriptor Procurement { get; init; } = new();
    public TransportationDistributionDescriptor TransportationDistribution { get; init; } = new();
    public ObjectiveFinanceDescriptor ObjectiveFinance { get; init; } = new();
    public SchedulingDescriptor Scheduling { get; init; } = new();

    /// <summary>Gets the physical supply-flow network descriptor.</summary>
    public SupplyNetworkDescriptor SupplyNetwork { get; private set; } = new();

    /// <summary>
    /// Gets the production-capacity regime derived from existing production
    /// and production-capacity facts.
    /// </summary>
    public ProductionCapacityRegime ProductionCapacityRegime =>
        !Production.HasProduction
            ? ProductionCapacityRegime.NotApplicable
            : !Capacity.HasProductionCapacity
                ? ProductionCapacityRegime.Uncapacitated
                : Capacity.HasTimeVaryingProductionCapacity
                    ? ProductionCapacityRegime.TimeVarying
                    : ProductionCapacityRegime.Constant;

    public static LotSizingProblemDescriptor FromLegacyFeatures(
        LotSizingProblemFeatures features)
    {
        ArgumentNullException.ThrowIfNull(features);

        return new LotSizingProblemDescriptor
        {
            Structure = new StructureDescriptor
            {
                ItemCount = features.ItemCount,
                PlantCount = features.PlantCount,
                WorkCenterCount = features.WorkCenterCount,
                WarehouseCount = features.WarehouseCount,
                SupplierCount = features.SupplierCount,
                DistributionCenterCount = features.DistributionCenterCount,
                TransportResourceCount = features.TransportResourceCount,
                ProductStructureRelationshipCount = features.ProductStructureRelationshipCount,
                MaximumProductStructureDepth = features.MaximumProductStructureDepth,
                ProductStructureType = features.ProductStructureType,
                IsMultiSite = features.IsMultiSite
            },
            Time = new TimeDescriptor
            {
                PlanningHorizon = features.PlanningHorizon
            },
            Demand = new DemandDescriptor
            {
                HasDemand = features.HasDemand,
                IsDeterministic = features.HasDeterministicDemand,
                IsTimeVarying = features.HasTimeVaryingDemand
            },
            Production = new ProductionDescriptor
            {
                HasProduction = features.HasProduction,
                HasLeadTimes = features.HasProductionLeadTimes,
                HasMinimumLotSizes = features.HasMinimumLotSizes,
                HasMaximumLotSizes = features.HasMaximumLotSizes,
                HasLotSizeMultiples = features.HasLotSizeMultiples
            },
            Capacity = new CapacityDescriptor
            {
                HasProductionCapacity = features.HasProductionCapacityConstraints,
                HasSharedProductionCapacity = features.HasSharedProductionCapacity,
                HasTimeVaryingProductionCapacity = features.HasTimeVaryingProductionCapacity,
                HasSupplierCapacity = features.HasSupplierCapacityConstraints,
                HasTransportCapacity = features.HasTransportCapacityConstraints,
                HasWarehouseCapacity = features.HasWarehouseCapacityConstraints,
                HasAdditionalProductionCapacity = features.HasAdditionalProductionCapacity,
                HasAdditionalWarehouseCapacity = features.HasAdditionalWarehouseCapacity,
                HasAdditionalTransportCapacity = features.HasAdditionalTransportCapacity
            },
            Setup = new SetupDescriptor
            {
                HasSetupCosts = features.HasSetupCosts,
                HasSetupTimes = features.HasSetupTimes,
                HasStartUpCosts = features.HasStartUpCosts,
                HasStartUpTimes = features.HasStartUpTimes
            },
            InventoryService = new InventoryServiceDescriptor
            {
                HasInitialInventory = features.HasInitialInventory,
                HasSafetyStockRequirements = features.HasSafetyStockRequirements,
                HasBacklogging = features.HasBacklogging,
                HasLostSales = features.HasLostSales
            },
            Procurement = new ProcurementDescriptor
            {
                HasPurchasing = features.HasPurchasing,
                HasSupplierLeadTimes = features.HasSupplierLeadTimes
            },
            TransportationDistribution = new TransportationDistributionDescriptor
            {
                HasTransportation = features.HasTransportation,
                HasTransportLeadTimes = features.HasTransportLeadTimes,
                HasDistribution = features.HasDistribution
            },
            ObjectiveFinance = new ObjectiveFinanceDescriptor
            {
                HasFinancialConstraints = features.HasFinancialConstraints,
                HasMultipleObjectives = features.HasMultipleObjectives,
                ObjectiveCriterionCount = features.ObjectiveCriterionCount,
                PrimaryObjectiveKind = features.PrimaryObjectiveKind,
                AggregationMode = features.ObjectiveAggregationMode
            },
            Scheduling = new SchedulingDescriptor
            {
                HasIntegratedScheduling = features.HasIntegratedScheduling,
                BucketMode = features.SchedulingBucketMode,
                HasInitialSetupState = features.HasInitialSetupState,
                HasSetupCarryOver = features.HasSetupCarryOver,
                HasSequenceDependentChangeoverTimes =
                    features.HasSequenceDependentChangeoverTimes,
                HasSequenceDependentChangeoverCosts =
                    features.HasSequenceDependentChangeoverCosts,
                HasMaximumSetupCountConstraints =
                    features.HasMaximumSetupCountConstraints
            }
        };
    }

    /// <summary>
    /// Creates the typed descriptor and enriches it with physical-network
    /// analysis derived directly from Core data.
    /// </summary>
    public static LotSizingProblemDescriptor FromLegacyFeatures(
        LotSizingProblemFeatures features,
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        LotSizingProblemDescriptor descriptor =
            FromLegacyFeatures(features);

        descriptor.SupplyNetwork =
            new SupplyNetworkAnalyzer().Analyze(supplyChain);

        return descriptor;
    }

    public LotSizingProblemFeatures ToLegacyFeatures()
    {
        return new LotSizingProblemFeatures
        {
            ItemCount = Structure.ItemCount,
            PlanningHorizon = Time.PlanningHorizon,
            PlantCount = Structure.PlantCount,
            WorkCenterCount = Structure.WorkCenterCount,
            WarehouseCount = Structure.WarehouseCount,
            SupplierCount = Structure.SupplierCount,
            DistributionCenterCount = Structure.DistributionCenterCount,
            TransportResourceCount = Structure.TransportResourceCount,
            ProductStructureRelationshipCount = Structure.ProductStructureRelationshipCount,
            MaximumProductStructureDepth = Structure.MaximumProductStructureDepth,
            ProductStructureType = Structure.ProductStructureType,

            HasDemand = Demand.HasDemand,
            HasDeterministicDemand = Demand.IsDeterministic,
            HasTimeVaryingDemand = Demand.IsTimeVarying,

            HasInitialInventory = InventoryService.HasInitialInventory,
            HasSafetyStockRequirements = InventoryService.HasSafetyStockRequirements,
            HasBacklogging = InventoryService.HasBacklogging,
            HasLostSales = InventoryService.HasLostSales,

            HasProduction = Production.HasProduction,
            HasProductionCapacityConstraints = Capacity.HasProductionCapacity,
            HasSharedProductionCapacity = Capacity.HasSharedProductionCapacity,
            HasTimeVaryingProductionCapacity = Capacity.HasTimeVaryingProductionCapacity,

            HasSetupCosts = Setup.HasSetupCosts,
            HasSetupTimes = Setup.HasSetupTimes,
            HasStartUpCosts = Setup.HasStartUpCosts,
            HasStartUpTimes = Setup.HasStartUpTimes,

            HasProductionLeadTimes = Production.HasLeadTimes,
            HasMinimumLotSizes = Production.HasMinimumLotSizes,
            HasMaximumLotSizes = Production.HasMaximumLotSizes,
            HasLotSizeMultiples = Production.HasLotSizeMultiples,

            HasAdditionalProductionCapacity = Capacity.HasAdditionalProductionCapacity,
            HasAdditionalWarehouseCapacity = Capacity.HasAdditionalWarehouseCapacity,
            HasAdditionalTransportCapacity = Capacity.HasAdditionalTransportCapacity,

            HasPurchasing = Procurement.HasPurchasing,
            HasSupplierCapacityConstraints = Capacity.HasSupplierCapacity,
            HasSupplierLeadTimes = Procurement.HasSupplierLeadTimes,

            HasTransportation = TransportationDistribution.HasTransportation,
            HasTransportCapacityConstraints = Capacity.HasTransportCapacity,
            HasTransportLeadTimes = TransportationDistribution.HasTransportLeadTimes,

            HasDistribution = TransportationDistribution.HasDistribution,
            HasWarehouseCapacityConstraints = Capacity.HasWarehouseCapacity,

            IsMultiSite = Structure.IsMultiSite,
            HasFinancialConstraints = ObjectiveFinance.HasFinancialConstraints,
            HasMultipleObjectives = ObjectiveFinance.HasMultipleObjectives,
            ObjectiveCriterionCount = ObjectiveFinance.ObjectiveCriterionCount,
            PrimaryObjectiveKind = ObjectiveFinance.PrimaryObjectiveKind,
            ObjectiveAggregationMode = ObjectiveFinance.AggregationMode,

            HasIntegratedScheduling = Scheduling.HasIntegratedScheduling,
            SchedulingBucketMode = Scheduling.BucketMode,
            HasInitialSetupState = Scheduling.HasInitialSetupState,
            HasSetupCarryOver = Scheduling.HasSetupCarryOver,
            HasSequenceDependentChangeoverTimes =
                Scheduling.HasSequenceDependentChangeoverTimes,
            HasSequenceDependentChangeoverCosts =
                Scheduling.HasSequenceDependentChangeoverCosts,
            HasMaximumSetupCountConstraints =
                Scheduling.HasMaximumSetupCountConstraints
        };
    }
}
