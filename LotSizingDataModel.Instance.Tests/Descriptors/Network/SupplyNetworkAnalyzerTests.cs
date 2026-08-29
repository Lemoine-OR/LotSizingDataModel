using LotSizingDataModel.Core;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Tests.Descriptors.Network;

public sealed class SupplyNetworkAnalyzerTests
{
    [Fact]
    public void Analyze_NoFlowArcs_ClassifiesIndependentNetwork()
    {
        var supplyChain = new SupplyChain(planningHorizon: 3);
        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(1, "Warehouse"));

        SupplyNetworkDescriptor descriptor =
            new SupplyNetworkAnalyzer().Analyze(supplyChain);

        Assert.Equal(
            SupplyNetworkTopologyType.Independent,
            descriptor.ForwardNetwork.Topology);
        Assert.False(descriptor.ForwardNetwork.HasCycles);
        Assert.Equal(1, descriptor.ForwardNetwork.EchelonCount);
        Assert.Equal(NetworkCouplingType.ForwardOnly, descriptor.Coupling);
        Assert.False(descriptor.HasReverseNetwork);
    }

    [Fact]
    public void Analyze_SupplierWarehouseDistributionCenter_IsThreeEchelonSerial()
    {
        SupplyChain supplyChain = CreateBaseSupplyChain();

        supplyChain.AddSupplier(new Supplier(1, "Supplier"));
        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(10, "Warehouse"));
        supplyChain.AddDistributionCenter(
            new DistributionCenter(20, "DC"));

        supplyChain.AddSupplierDelivery(
            SupplierDelivery.ToStandaloneWarehouse(1, 1, 10, 0));

        supplyChain.AddDistributionCenterSourcing(
            DistributionCenterSourcing.FromStandaloneWarehouse(
                20,
                1,
                10));

        supplyChain.AddDemand(
            new Demand(1, 20, 3));

        SupplyNetworkDescriptor descriptor =
            new SupplyNetworkAnalyzer().Analyze(supplyChain);

        Assert.Equal(
            SupplyNetworkTopologyType.Serial,
            descriptor.ForwardNetwork.Topology);
        Assert.Equal(3, descriptor.ForwardNetwork.EchelonCount);
        Assert.Single(descriptor.ForwardNetwork.SourceKeys);
        Assert.Single(descriptor.ForwardNetwork.SinkKeys);
        Assert.True(descriptor.HasDistributionNetwork);
        Assert.True(
            descriptor.HasExternalDemandAtDistributionCenters);
        Assert.True(descriptor.HasMultiEchelonStructure);
        Assert.True(
            descriptor.IsAcyclicSingleSourcingDistributionNetwork);
    }

    [Fact]
    public void Analyze_DivergentWarehouseFlow_IsDetected()
    {
        SupplyChain supplyChain = CreateBaseSupplyChain();

        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(1, "Central"));
        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(2, "Regional A"));
        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(3, "Regional B"));

        var resource = new TransportResource(1, "Truck");

        resource.AddLane(
            new TransportLane(
                WarehouseReference.ForStandaloneWarehouse(1),
                WarehouseReference.ForStandaloneWarehouse(2),
                0));

        resource.AddLane(
            new TransportLane(
                WarehouseReference.ForStandaloneWarehouse(1),
                WarehouseReference.ForStandaloneWarehouse(3),
                0));

        supplyChain.AddTransportResource(resource);

        SupplyNetworkDescriptor descriptor =
            new SupplyNetworkAnalyzer().Analyze(supplyChain);

        Assert.Equal(
            SupplyNetworkTopologyType.Divergent,
            descriptor.ForwardNetwork.Topology);
        Assert.Equal(2, descriptor.ForwardNetwork.EchelonCount);
        Assert.True(descriptor.HasTransshipment);
    }

    [Fact]
    public void Analyze_TwoSuppliersSameItemWarehouse_DetectsConvergenceAndMultiSourcing()
    {
        SupplyChain supplyChain = CreateBaseSupplyChain();

        supplyChain.AddSupplier(new Supplier(1, "Supplier A"));
        supplyChain.AddSupplier(new Supplier(2, "Supplier B"));
        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(10, "Warehouse"));

        supplyChain.AddSupplierDelivery(
            SupplierDelivery.ToStandaloneWarehouse(1, 1, 10, 0));
        supplyChain.AddSupplierDelivery(
            SupplierDelivery.ToStandaloneWarehouse(2, 1, 10, 0));

        SupplyNetworkDescriptor descriptor =
            new SupplyNetworkAnalyzer().Analyze(supplyChain);

        Assert.Equal(
            SupplyNetworkTopologyType.Convergent,
            descriptor.ForwardNetwork.Topology);
        Assert.True(descriptor.HasMultiSourcing);
        Assert.False(
            descriptor.IsAcyclicSingleSourcingDistributionNetwork);
    }

    [Fact]
    public void Analyze_PhysicalCycle_IsAllowedAndReportedSeparately()
    {
        SupplyChain supplyChain = CreateBaseSupplyChain();

        supplyChain.AddStandaloneWarehouse(new StandaloneWarehouse(1, "A"));
        supplyChain.AddStandaloneWarehouse(new StandaloneWarehouse(2, "B"));

        var resource = new TransportResource(1, "Loop");

        resource.AddLane(
            new TransportLane(
                WarehouseReference.ForStandaloneWarehouse(1),
                WarehouseReference.ForStandaloneWarehouse(2),
                0));

        resource.AddLane(
            new TransportLane(
                WarehouseReference.ForStandaloneWarehouse(2),
                WarehouseReference.ForStandaloneWarehouse(1),
                0));

        supplyChain.AddTransportResource(resource);

        SupplyNetworkDescriptor descriptor =
            new SupplyNetworkAnalyzer().Analyze(supplyChain);

        Assert.True(descriptor.ForwardNetwork.HasCycles);
        Assert.Null(descriptor.ForwardNetwork.EchelonCount);
        Assert.Equal(
            SupplyNetworkTopologyType.General,
            descriptor.ForwardNetwork.Topology);
        Assert.Equal(NetworkCouplingType.ForwardOnly, descriptor.Coupling);
    }

    [Fact]
    public void TypedProblemDescriptor_CanBeEnrichedWithPhysicalNetwork()
    {
        SupplyChain supplyChain = CreateBaseSupplyChain();

        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(1, "Warehouse"));

        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 3,
                ProductStructureType =
                    ProductStructureType.IndependentItems
            };

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features,
                supplyChain);

        Assert.Equal(
            SupplyNetworkTopologyType.Independent,
            descriptor.SupplyNetwork.ForwardNetwork.Topology);

        LotSizingProblemFeatures roundTrip =
            descriptor.ToLegacyFeatures();

        Assert.Equal(features.ItemCount, roundTrip.ItemCount);
        Assert.Equal(features.PlanningHorizon, roundTrip.PlanningHorizon);
    }

    private static SupplyChain CreateBaseSupplyChain()
    {
        var supplyChain =
            new SupplyChain(planningHorizon: 3);

        supplyChain.AddItem(
            new Item(
                id: 1,
                name: "Item",
                billOfMaterialsLevel: 0));

        return supplyChain;
    }
}
