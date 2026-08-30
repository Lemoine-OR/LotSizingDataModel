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
    public async Task Dlsp_ModelContainsStateStartActivationAndProducedItemCount()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketSchedulingFormulationKind.Dlsp);

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

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name ==
                    "smallBucketProducedItemCount_t1");
    }

    [Fact]
    public async Task Cslp_ModelUsesProductionActivationForRealPeriodCountLimit()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketSchedulingFormulationKind.Cslp);

        MathematicalModel model =
            await SmallBucketSchedulingFormulationFactory
                .CreateCslp()
                .BuildAsync(instance);

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
                    "cslpProductionState_",
                    StringComparison.Ordinal));

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name ==
                    "smallBucketProducedItemCount_t1");
    }

    [Fact]
    public async Task Plsp_ModelUsesIncomingAndOutgoingStates()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketSchedulingFormulationKind.Plsp);

        MathematicalModel model =
            await SmallBucketSchedulingFormulationFactory
                .CreatePlsp()
                .BuildAsync(instance);

        LinearConstraint state =
            Assert.Single(
                model.Constraints,
                constraint =>
                    constraint.Name ==
                        "plspSingleSetupState_t1");

        Assert.Equal(
            MathematicalConstraintSense.Equal,
            state.Sense);

        Assert.Equal(
            1.0,
            state.RightHandSide);

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name ==
                    "plspProductionState_r1_t2");

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name ==
                    "smallBucketProducedItemCount_t2");
    }

    [Fact]
    public async Task Plsp_ZeroTransitionPeriodPreservesSetupState()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketSchedulingFormulationKind.Plsp);

        ProductionSchedulingProfile profile =
            instance.SupplyChain.WorkCenters
                .Single()
                .SchedulingProfile!;

        profile.MaximumSetupCount!
            .SetCount(
                period: 2,
                count: 0);

        MathematicalModel model =
            await SmallBucketSchedulingFormulationFactory
                .CreatePlsp()
                .BuildAsync(instance);

        Assert.Equal(
            2,
            model.Constraints.Count(
                constraint =>
                    constraint.Name.StartsWith(
                        "plspNoSetupTransition_",
                        StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Cslp_ZeroProducedItemLimitIsRepresented()
    {
        LotSizingInstance instance =
            CreateInstance(
                SmallBucketSchedulingFormulationKind.Cslp);

        ProductionSchedulingProfile profile =
            instance.SupplyChain.WorkCenters
                .Single()
                .SchedulingProfile!;

        profile.MaximumProducedItemCount!
            .SetCount(
                period: 2,
                count: 0);

        MathematicalModel model =
            await SmallBucketSchedulingFormulationFactory
                .CreateCslp()
                .BuildAsync(instance);

        LinearConstraint limit =
            Assert.Single(
                model.Constraints,
                constraint =>
                    constraint.Name ==
                        "smallBucketProducedItemCount_t2");

        Assert.Equal(
            0.0,
            limit.RightHandSide);
    }

    private static LotSizingInstance CreateInstance(
        SmallBucketSchedulingFormulationKind kind)
    {
        const int horizon = 2;

        var chain =
            new SupplyChain(horizon);

        chain.Items.Add(
            new Item(1, "I1", 0));

        chain.Items.Add(
            new Item(2, "I2", 0));

        bool isDlsp =
            kind ==
            SmallBucketSchedulingFormulationKind.Dlsp;

        bool isPlsp =
            kind ==
            SmallBucketSchedulingFormulationKind.Plsp;

        var profile =
            new ProductionSchedulingProfile
            {
                BucketMode =
                    SchedulingBucketMode.SmallBucket,
                SmallBucketProductionMode =
                    isDlsp
                        ? SmallBucketProductionMode.AllOrNothing
                        : SmallBucketProductionMode.Continuous,
                MaximumProducedItemCount =
                    new MaximumProducedItemCount(
                        horizon,
                        isPlsp ? 2 : 1)
            };

        if (isPlsp)
        {
            profile.MaximumSetupCount =
                new MaximumSetupCount(
                    horizon,
                    1);
        }

        var workCenter =
            new WorkCenter(1, "M1")
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
