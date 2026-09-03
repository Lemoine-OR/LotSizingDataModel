using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack09SchedulingExecutionTests
{
    [Fact]
    public async Task SchedulingBuilders_CreateStateAndChangeoverVariables()
    {
        LotSizingInstance instance = CreateInstance();

        var queries =
            new SupplyChainQueries(
                instance.SupplyChain);

        Assert.NotNull(
            queries.GetRequiredProductionCharacteristic(
                1,
                new WorkCenterReference(1, 1)));

        Assert.NotNull(
            queries.GetRequiredProductionCharacteristic(
                2,
                new WorkCenterReference(1, 1)));

        StandardLotSizingFormulation formulation =
            StandardLotSizingFormulationFactory.CreateDefault();

        Assert.True(formulation.CanBuild(instance));

        var model =
            await formulation.BuildAsync(instance);

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    "microPeriodSetupState|",
                    StringComparison.Ordinal));

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    "auxiliaryMicroPeriodChangeover|",
                    StringComparison.Ordinal));
    }

    private static LotSizingInstance CreateInstance()
    {
        var chain =
            new SupplyChain
            {
                PlanningHorizon = 2
            };

        chain.Items.Add(new Item(1, "A", 0));
        chain.Items.Add(new Item(2, "B", 0));

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-WH"));

        var workCenter =
            new WorkCenter(1, "WC1")
            {
                SchedulingProfile =
                    new ProductionSchedulingProfile
                    {
                        BucketMode =
                            SchedulingBucketMode.MacroMicro,
                        MicroPeriodsPerPeriod = 2
                    },
                SetupTransitionProfile =
                    new ProductionSetupTransitionProfile
                    {
                        CarryOverPolicy =
                            SetupCarryOverPolicy.Allowed
                    }
            };

        workCenter.SetupTransitionProfile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId = 1,
                ToItemId = 2,
                ChangeoverTime =
                    new ProductionChangeoverTime(2, 1.0),
                ChangeoverCost =
                    new ProductionChangeoverCost(2, 2.0)
            });

        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        var r1 =
            new ProductionRouting(1, 1, 1, 0);
        r1.AddWorkCenter(1);

        var r2 =
            new ProductionRouting(2, 2, 1, 0);
        r2.AddWorkCenter(1);

        chain.ProductionRoutings.Add(r1);
        chain.ProductionRoutings.Add(r2);

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(
                itemId: 1,
                plantId: 1,
                workCenterId: 1));

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(
                itemId: 2,
                plantId: 1,
                workCenterId: 1));

        return new LotSizingInstance
        {
            SupplyChain = chain
        };
    }
}
