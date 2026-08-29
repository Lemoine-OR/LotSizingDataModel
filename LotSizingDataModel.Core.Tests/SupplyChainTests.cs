using LotSizingDataModel.Core;

namespace LotSizingDataModel.Core.Tests;

public sealed class SupplyChainTests
{
    [Fact]
    public void Constructor_SetsPlanningHorizon()
    {
        var supplyChain = new SupplyChain(planningHorizon: 12);

        Assert.Equal(12, supplyChain.PlanningHorizon);
    }

    [Fact]
    public void EmptySupplyChain_ExposesEmptyCalculatedPhysicalCollections()
    {
        var supplyChain = new SupplyChain(planningHorizon: 6);

        Assert.Empty(supplyChain.Warehouses);
        Assert.Empty(supplyChain.WorkCenters);
    }

    [Fact]
    public void EmptySupplyChain_HasConsistentPlanningHorizon()
    {
        var supplyChain = new SupplyChain(planningHorizon: 6);

        Assert.True(supplyChain.HasConsistentPlanningHorizon);
    }
}
