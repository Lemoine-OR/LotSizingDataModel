using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class LotSizingProblemFeaturesTests
{
    [Fact]
    public void Counts_RejectNegativeValues()
    {
        var features = new LotSizingProblemFeatures();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => features.ItemCount = -1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => features.PlanningHorizon = -1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => features.ProductStructureRelationshipCount = -1);
    }

    [Fact]
    public void ItemCardinality_DerivesSingleAndMultiItemFlags()
    {
        var features = new LotSizingProblemFeatures
        {
            ItemCount = 1
        };

        Assert.True(features.IsSingleItem);
        Assert.False(features.IsMultiItem);

        features.ItemCount = 2;

        Assert.False(features.IsSingleItem);
        Assert.True(features.IsMultiItem);
    }

    [Fact]
    public void ProductStructure_DerivesSingleAndMultiLevelFlags()
    {
        var features = new LotSizingProblemFeatures
        {
            ProductStructureRelationshipCount = 0
        };

        Assert.False(features.HasProductStructure);
        Assert.True(features.IsSingleLevel);
        Assert.False(features.IsMultiLevel);

        features.ProductStructureRelationshipCount = 1;

        Assert.True(features.HasProductStructure);
        Assert.False(features.IsSingleLevel);
        Assert.True(features.IsMultiLevel);
    }

    [Fact]
    public void AnyCapacityFamily_MakesProblemCapacitated()
    {
        var features = new LotSizingProblemFeatures();

        Assert.False(features.IsCapacitated);

        features.HasSupplierCapacityConstraints = true;

        Assert.True(features.IsCapacitated);
    }

    [Fact]
    public void LotSizeRestrictionFlag_IsDerivedFromAllSupportedRestrictionFamilies()
    {
        var features = new LotSizingProblemFeatures();

        Assert.False(features.HasLotSizeRestrictions);

        features.HasLotSizeMultiples = true;

        Assert.True(features.HasLotSizeRestrictions);
    }

    [Fact]
    public void SupplyChainNetworkDecisionFlag_IsDerivedFromPurchasingTransportOrDistribution()
    {
        var features = new LotSizingProblemFeatures();

        Assert.False(features.HasSupplyChainNetworkDecisions);

        features.HasTransportation = true;

        Assert.True(features.HasSupplyChainNetworkDecisions);
    }

    [Fact]
    public void StationaryDemand_RequiresDemandAndNoTimeVariation()
    {
        var features = new LotSizingProblemFeatures
        {
            HasDemand = true,
            HasTimeVaryingDemand = false
        };

        Assert.True(features.HasStationaryDemand);

        features.HasTimeVaryingDemand = true;

        Assert.False(features.HasStationaryDemand);
    }

    [Fact]
    public void StructurallyUsable_RequiresItemsHorizonAndKnownProductStructure()
    {
        var features = new LotSizingProblemFeatures
        {
            ItemCount = 1,
            PlanningHorizon = 12,
            ProductStructureType = ProductStructureType.IndependentItems
        };

        Assert.True(features.IsStructurallyUsable);

        features.ProductStructureType = ProductStructureType.Unknown;

        Assert.False(features.IsStructurallyUsable);
    }

    [Fact]
    public void ConstantProductionCapacity_RequiresCapacityWithoutTimeVariation()
    {
        var features = new LotSizingProblemFeatures
        {
            HasProductionCapacityConstraints = true,
            HasTimeVaryingProductionCapacity = false
        };

        Assert.True(features.HasConstantProductionCapacity);

        features.HasTimeVaryingProductionCapacity = true;

        Assert.False(features.HasConstantProductionCapacity);
    }
}
