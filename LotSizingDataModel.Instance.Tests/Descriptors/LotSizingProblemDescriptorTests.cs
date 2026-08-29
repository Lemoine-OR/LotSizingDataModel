using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.Tests.Descriptors;

public sealed class LotSizingProblemDescriptorTests
{
    [Fact]
    public void FromLegacyFeatures_PreservesTypedAndDerivedSemantics()
    {
        var features = CreateRepresentativeFeatures();

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(features);

        Assert.Equal(4, descriptor.Structure.ItemCount);
        Assert.Equal(ProductStructureType.General, descriptor.Structure.ProductStructureType);
        Assert.True(descriptor.Structure.IsMultiLevel);
        Assert.True(descriptor.Time.IsMultiPeriod);
        Assert.True(descriptor.Demand.IsDeterministic);
        Assert.True(descriptor.Capacity.IsCapacitated);
        Assert.True(descriptor.Production.HasLotSizeRestrictions);
        Assert.True(descriptor.TransportationDistribution.HasNetworkDecisions);
    }

    [Fact]
    public void LegacyFeatureRoundTrip_IsLosslessForPrimaryProperties()
    {
        var original = CreateRepresentativeFeatures();

        LotSizingProblemFeatures roundTrip =
            LotSizingProblemDescriptor.FromLegacyFeatures(original).ToLegacyFeatures();

        AssertEquivalent(original, roundTrip);
    }

    [Fact]
    public void FromLegacyFeatures_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => LotSizingProblemDescriptor.FromLegacyFeatures(null!));
    }

    private static LotSizingProblemFeatures CreateRepresentativeFeatures()
    {
        return new LotSizingProblemFeatures
        {
            ItemCount = 4,
            PlanningHorizon = 12,
            PlantCount = 2,
            WorkCenterCount = 3,
            WarehouseCount = 4,
            SupplierCount = 2,
            DistributionCenterCount = 2,
            TransportResourceCount = 1,
            ProductStructureRelationshipCount = 5,
            MaximumProductStructureDepth = 3,
            ProductStructureType = ProductStructureType.General,
            HasDemand = true,
            HasDeterministicDemand = true,
            HasTimeVaryingDemand = true,
            HasInitialInventory = true,
            HasSafetyStockRequirements = true,
            HasBacklogging = true,
            HasLostSales = false,
            HasProduction = true,
            HasProductionCapacityConstraints = true,
            HasSharedProductionCapacity = true,
            HasTimeVaryingProductionCapacity = true,
            HasSetupCosts = true,
            HasSetupTimes = true,
            HasStartUpCosts = false,
            HasProductionLeadTimes = true,
            HasMinimumLotSizes = true,
            HasMaximumLotSizes = false,
            HasLotSizeMultiples = true,
            HasAdditionalProductionCapacity = true,
            HasAdditionalWarehouseCapacity = false,
            HasAdditionalTransportCapacity = true,
            HasPurchasing = true,
            HasSupplierCapacityConstraints = true,
            HasSupplierLeadTimes = true,
            HasTransportation = true,
            HasTransportCapacityConstraints = true,
            HasTransportLeadTimes = true,
            HasDistribution = true,
            HasWarehouseCapacityConstraints = true,
            IsMultiSite = true,
            HasFinancialConstraints = false,
            HasMultipleObjectives = false
        };
    }

    private static void AssertEquivalent(
        LotSizingProblemFeatures e,
        LotSizingProblemFeatures a)
    {
        Assert.Equal(e.ItemCount, a.ItemCount);
        Assert.Equal(e.PlanningHorizon, a.PlanningHorizon);
        Assert.Equal(e.PlantCount, a.PlantCount);
        Assert.Equal(e.WorkCenterCount, a.WorkCenterCount);
        Assert.Equal(e.WarehouseCount, a.WarehouseCount);
        Assert.Equal(e.SupplierCount, a.SupplierCount);
        Assert.Equal(e.DistributionCenterCount, a.DistributionCenterCount);
        Assert.Equal(e.TransportResourceCount, a.TransportResourceCount);
        Assert.Equal(e.ProductStructureRelationshipCount, a.ProductStructureRelationshipCount);
        Assert.Equal(e.MaximumProductStructureDepth, a.MaximumProductStructureDepth);
        Assert.Equal(e.ProductStructureType, a.ProductStructureType);
        Assert.Equal(e.HasDemand, a.HasDemand);
        Assert.Equal(e.HasDeterministicDemand, a.HasDeterministicDemand);
        Assert.Equal(e.HasTimeVaryingDemand, a.HasTimeVaryingDemand);
        Assert.Equal(e.HasInitialInventory, a.HasInitialInventory);
        Assert.Equal(e.HasSafetyStockRequirements, a.HasSafetyStockRequirements);
        Assert.Equal(e.HasBacklogging, a.HasBacklogging);
        Assert.Equal(e.HasLostSales, a.HasLostSales);
        Assert.Equal(e.HasProduction, a.HasProduction);
        Assert.Equal(e.HasProductionCapacityConstraints, a.HasProductionCapacityConstraints);
        Assert.Equal(e.HasSharedProductionCapacity, a.HasSharedProductionCapacity);
        Assert.Equal(e.HasTimeVaryingProductionCapacity, a.HasTimeVaryingProductionCapacity);
        Assert.Equal(e.HasSetupCosts, a.HasSetupCosts);
        Assert.Equal(e.HasSetupTimes, a.HasSetupTimes);
        Assert.Equal(e.HasStartUpCosts, a.HasStartUpCosts);
        Assert.Equal(e.HasProductionLeadTimes, a.HasProductionLeadTimes);
        Assert.Equal(e.HasMinimumLotSizes, a.HasMinimumLotSizes);
        Assert.Equal(e.HasMaximumLotSizes, a.HasMaximumLotSizes);
        Assert.Equal(e.HasLotSizeMultiples, a.HasLotSizeMultiples);
        Assert.Equal(e.HasAdditionalProductionCapacity, a.HasAdditionalProductionCapacity);
        Assert.Equal(e.HasAdditionalWarehouseCapacity, a.HasAdditionalWarehouseCapacity);
        Assert.Equal(e.HasAdditionalTransportCapacity, a.HasAdditionalTransportCapacity);
        Assert.Equal(e.HasPurchasing, a.HasPurchasing);
        Assert.Equal(e.HasSupplierCapacityConstraints, a.HasSupplierCapacityConstraints);
        Assert.Equal(e.HasSupplierLeadTimes, a.HasSupplierLeadTimes);
        Assert.Equal(e.HasTransportation, a.HasTransportation);
        Assert.Equal(e.HasTransportCapacityConstraints, a.HasTransportCapacityConstraints);
        Assert.Equal(e.HasTransportLeadTimes, a.HasTransportLeadTimes);
        Assert.Equal(e.HasDistribution, a.HasDistribution);
        Assert.Equal(e.HasWarehouseCapacityConstraints, a.HasWarehouseCapacityConstraints);
        Assert.Equal(e.IsMultiSite, a.IsMultiSite);
        Assert.Equal(e.HasFinancialConstraints, a.HasFinancialConstraints);
        Assert.Equal(e.HasMultipleObjectives, a.HasMultipleObjectives);
    }
}
