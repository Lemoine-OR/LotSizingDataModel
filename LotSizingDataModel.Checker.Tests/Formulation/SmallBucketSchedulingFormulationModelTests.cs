using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class SmallBucketSchedulingFormulationModelTests
{
    [Fact]
    public async Task Dlsp_ModelContainsStateStartAndFullBucketActivation()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketProductionMode.AllOrNothing);

        MathematicalModel model =
            await SmallBucketSchedulingFormulationFactory
                .CreateDlsp()
                .BuildAsync(instance);

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory.Setup + "|",
                    StringComparison.Ordinal));

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory
                        .AuxiliarySchedulingSetupStart + "|",
                    StringComparison.Ordinal));

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory
                        .AuxiliarySmallBucketProductionActivation + "|",
                    StringComparison.Ordinal));

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name.StartsWith(
                    "dlspProductionFullBucket_",
                    StringComparison.Ordinal));
    }

    [Fact]
    public async Task Cslp_ModelContainsStateAndStartWithoutDlspActivation()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketProductionMode.Continuous);

        MathematicalModel model =
            await SmallBucketSchedulingFormulationFactory
                .CreateCslp()
                .BuildAsync(instance);

        Assert.DoesNotContain(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory
                        .AuxiliarySmallBucketProductionActivation + "|",
                    StringComparison.Ordinal));

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name.StartsWith(
                    "cslpProductionState_",
                    StringComparison.Ordinal));
    }

    private static LotSizingInstance CreateInstance(
        SmallBucketProductionMode mode)
    {
        const int horizon = 2;

        var chain =
            new SupplyChain(horizon);

        chain.Items.Add(
            new Item(1, "I1", 0));

        chain.Items.Add(
            new Item(2, "I2", 0));

        var workCenter =
            new WorkCenter(1, "M1")
            {
                CapacityConstraint =
                    new CapacityConstraint(
                        horizon,
                        10.0),
                SchedulingProfile =
                    new ProductionSchedulingProfile
                    {
                        BucketMode =
                            SchedulingBucketMode.SmallBucket,
                        SmallBucketProductionMode =
                            mode,
                        MaximumProducedItemCount =
                            new MaximumProducedItemCount(
                                horizon,
                                1)
                    }
            };

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(
            workCenter);

        chain.Plants.Add(
            plant);

        AddRouting(
            chain,
            routingId: 1,
            itemId: 1,
            horizon);

        AddRouting(
            chain,
            routingId: 2,
            itemId: 2,
            horizon);

        return new LotSizingInstance(
            chain,
            "small-bucket-test");
    }

    private static void AddRouting(
        SupplyChain chain,
        int routingId,
        int itemId,
        int horizon)
    {
        var routing =
            new ProductionRouting(
                routingId,
                itemId,
                plantId: 1,
                leadTime: 0);

        routing.AddWorkCenter(
            1);

        chain.ProductionRoutings.Add(
            routing);

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(
                itemId,
                plantId: 1,
                workCenterId: 1)
            {
                UnitCapacityConsumption =
                    new UnitCapacityConsumption(
                        horizon,
                        1.0),
                FixedSetupCost =
                    new FixedSetupCost(
                        horizon,
                        5.0)
            });
    }
}
