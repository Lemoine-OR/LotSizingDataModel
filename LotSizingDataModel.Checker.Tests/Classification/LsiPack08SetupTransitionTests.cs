using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Validation;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification.Notation;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack08SetupTransitionTests
{
    [Fact]
    public void ChangeoverParameters_AreNonNegative()
    {
        var time = new ProductionChangeoverTime(2, 3.0);
        var cost = new ProductionChangeoverCost(2, 4.0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => time[1] = -1.0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => cost[1] = -1.0);
    }

    [Fact]
    public void TransitionProfile_Resizes()
    {
        var profile = new ProductionSetupTransitionProfile
        {
            CarryOverPolicy = SetupCarryOverPolicy.Allowed
        };

        profile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId = 1,
                ToItemId = 2,
                ChangeoverTime =
                    new ProductionChangeoverTime(2, 1.0)
            });

        profile.ResizeTimeSeries(4);

        Assert.Equal(4, profile.PlanningHorizon);
        Assert.True(profile.HasSequenceDependentTimes);
    }

    [Fact]
    public void StandardFormulation_RejectsUnsupportedTransitionSemantics()
    {
        SupplyChain chain = BuildChain();

        chain.Plants[0].WorkCenters[0].SetupTransitionProfile =
            new ProductionSetupTransitionProfile
            {
                CarryOverPolicy = SetupCarryOverPolicy.Allowed
            };

        var instance = new LotSizingInstance
        {
            SupplyChain = chain
        };

        StandardLotSizingFormulation formulation =
            StandardLotSizingFormulationFactory.CreateDefault();

        Assert.False(formulation.CanBuild(instance));
    }

    [Fact]
    public void Validator_AcceptsValidDirectedChangeover()
    {
        SupplyChain chain = BuildChain();

        var profile = new ProductionSetupTransitionProfile
        {
            CarryOverPolicy = SetupCarryOverPolicy.Forbidden
        };

        profile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId = 1,
                ToItemId = 2,
                ChangeoverTime =
                    new ProductionChangeoverTime(2, 1.0),
                ChangeoverCost =
                    new ProductionChangeoverCost(2, 5.0)
            });

        chain.Plants[0].WorkCenters[0].SetupTransitionProfile =
            profile;

        Assert.Contains(
            chain.ProductionRoutings,
            routing =>
                routing.ItemId == 1 &&
                routing.PlantId == 1 &&
                routing.UsesWorkCenter(1));

        Assert.Contains(
            chain.ProductionRoutings,
            routing =>
                routing.ItemId == 2 &&
                routing.PlantId == 1 &&
                routing.UsesWorkCenter(1));

        var issues =
            new List<SupplyChainValidator.ValidationIssue>();

        ProductionSetupTransitionValidator.AppendIssues(
            chain,
            issues);

        Assert.Empty(issues);
    }

    private static SupplyChain BuildChain()
    {
        var chain = new SupplyChain
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

        plant.WorkCenters.Add(
            new WorkCenter(1, "WC1"));

        chain.Plants.Add(plant);

        var routing1 =
            new LotSizingDataModel.Core.Relationships.ProductionRouting(
                id: 1,
                itemId: 1,
                plantId: 1,
                leadTime: 0);

        routing1.AddWorkCenter(1);

        var routing2 =
            new LotSizingDataModel.Core.Relationships.ProductionRouting(
                id: 2,
                itemId: 2,
                plantId: 1,
                leadTime: 0);

        routing2.AddWorkCenter(1);

        chain.ProductionRoutings.Add(routing1);
        chain.ProductionRoutings.Add(routing2);

        return chain;
    }
}
