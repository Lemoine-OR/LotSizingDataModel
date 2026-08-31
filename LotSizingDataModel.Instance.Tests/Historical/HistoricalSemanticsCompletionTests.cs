using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Classification.Historical;
using LotSizingDataModel.Instance.Historical;

namespace LotSizingDataModel.Instance.Tests.Historical;

public sealed class HistoricalSemanticsCompletionTests
{
    [Fact]
    public void DlsiAndDls_AreDistinguishedByRealInitialStockSemantics()
    {
        var dlsi =
            CreateSmallBucketInstance(
                InitialInventoryDecisionMode.VariableDecision,
                maximumSetupCount: 1);

        var dls =
            CreateSmallBucketInstance(
                InitialInventoryDecisionMode.AbsentFixedZero,
                maximumSetupCount: 1);

        Assert.Equal(
            WolseyDetectedProblemVariant.DLSI,
            WolseyHistoricalSemanticsAnalyzer
                .Analyze(dlsi)
                .ProblemVariant);

        Assert.Equal(
            WolseyDetectedProblemVariant.DLS,
            WolseyHistoricalSemanticsAnalyzer
                .Analyze(dls)
                .ProblemVariant);
    }

    [Fact]
    public void SalesOption_IsDistinctAndFollowsPlanningHorizon()
    {
        var chain = new SupplyChain(2);
        chain.Items.Add(new Item(1, "I1", 0));

        var center =
            new DistributionCenter(
                1,
                "DC1");

        chain.DistributionCenters.Add(center);

        var option =
            new SalesOption(
                1,
                1,
                2);

        option.SetMaximumAdditionalSales(1, 5.0);
        option.SetUnitPrice(1, 7.0);

        chain.AddSalesOption(option);

        Assert.Equal(
            5.0,
            option.GetMaximumAdditionalSales(1));

        Assert.Equal(
            7.0,
            option.GetUnitPrice(1));

        chain.ResizeTimeSeries(3);

        Assert.Equal(
            3,
            option.PlanningHorizon);
    }

    [Fact]
    public void ExactCounters_Sb1_And_Set_AreProjectedFromGenericSemantics()
    {
        var instance =
            CreateSmallBucketInstance(
                InitialInventoryDecisionMode.VariableDecision,
                maximumSetupCount: 1);

        ProductionCharacteristic characteristic =
            Assert.Single(
                instance.SupplyChain.ProductionCharacteristics);

        characteristic.SetupTime =
            new SetupTime(
                instance.PlanningHorizon,
                0.25);

        WolseyHistoricalDescriptor descriptor =
            WolseyHistoricalSemanticsAnalyzer.Analyze(
                instance);

        Assert.Equal(1, descriptor.NumberOfMachines);
        Assert.Equal(1, descriptor.NumberOfItems);
        Assert.Equal(2, descriptor.NumberOfPeriods);
        Assert.Equal(1, descriptor.NumberOfLevels);
        Assert.Equal(
            WolseyDetectedBucketVariant.SB1,
            descriptor.BucketVariant);
        Assert.True(descriptor.HasSetupTimes);
        Assert.Contains(
            "SET",
            descriptor.ToDetectedSummary(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Sb2_IsDetectedOnlyFromExactSetupCountSemantics()
    {
        var instance =
            CreateSmallBucketInstance(
                InitialInventoryDecisionMode.VariableDecision,
                maximumSetupCount: 2);

        Assert.Equal(
            WolseyDetectedBucketVariant.SB2,
            WolseyHistoricalSemanticsAnalyzer
                .Analyze(instance)
                .BucketVariant);
    }

    [Fact]
    public void ImVm_ArePreservedAsDeclaredSourceLabels_NeverInferred()
    {
        var instance =
            CreateSmallBucketInstance(
                InitialInventoryDecisionMode.VariableDecision,
                maximumSetupCount: 1);

        instance.HistoricalSemantics =
            new HistoricalSemanticsMetadata
            {
                OriginalWolseyCode = "DLSI-CC",
                DeclaredWolseyMachineLabel =
                    WolseyDeclaredMachineLabel.IM
            };

        WolseyHistoricalDescriptor descriptor =
            WolseyHistoricalSemanticsAnalyzer.Analyze(
                instance);

        Assert.Equal(
            WolseyDeclaredMachineLabel.IM,
            descriptor.DeclaredMachineLabel);

        Assert.False(
            descriptor.MachineLabelWasInferred);
    }

    [Fact]
    public void NonZeroFixedInitialInventory_IsRejectedForDlsOrDlsiSemantics()
    {
        var instance =
            CreateSmallBucketInstance(
                InitialInventoryDecisionMode.AbsentFixedZero,
                maximumSetupCount: 1);

        Inventory inventory =
            Assert.Single(
                instance.SupplyChain.Inventories);

        inventory.InitialInventory = 2.0;

        Assert.NotEmpty(
            WolseyHistoricalSemanticsAnalyzer
                .ValidateHistoricalSemantics(instance));
    }

    private static LotSizingInstance CreateSmallBucketInstance(
        InitialInventoryDecisionMode initialMode,
        int maximumSetupCount)
    {
        const int horizon = 2;

        var chain = new SupplyChain(horizon);
        chain.Items.Add(new Item(1, "I1", 0));

        var profile =
            new ProductionSchedulingProfile
            {
                BucketMode =
                    SchedulingBucketMode.SmallBucket,
                SmallBucketProductionMode =
                    SmallBucketProductionMode.AllOrNothing,
                SetupCarryOverPolicy =
                    SetupCarryOverPolicy.Allowed,
                MaximumProducedItemCount =
                    new MaximumProducedItemCount(
                        horizon,
                        1),
                MaximumSetupCount =
                    new MaximumSetupCount(
                        horizon,
                        maximumSetupCount)
            };

        var workCenter =
            new WorkCenter(
                1,
                "M1")
            {
                CapacityConstraint =
                    new CapacityConstraint(
                        horizon,
                        10.0),
                SchedulingProfile =
                    profile
            };

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse(
                    "P1-Warehouse"));

        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        var routing =
            new ProductionRouting(
                1,
                1,
                1,
                0);

        routing.AddWorkCenter(1);
        chain.ProductionRoutings.Add(routing);

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(
                1,
                1,
                1)
            {
                UnitCapacityConsumption =
                    new UnitCapacityConsumption(
                        horizon,
                        1.0)
            });

        var inventory =
            Inventory.ForPlantWarehouse(
                1,
                1,
                0.0);

        inventory.InitialInventoryDecisionMode =
            initialMode;

        chain.Inventories.Add(inventory);

        return new LotSizingInstance(
            chain,
            "historical-semantics");
    }
}
