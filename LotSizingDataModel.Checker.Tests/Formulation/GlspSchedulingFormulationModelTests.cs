using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class GlspSchedulingFormulationModelTests
{
    [Fact]
    public async Task Glsp_ModelContainsMicroStateProductionChangeoverAndMacroCapacity()
    {
        LotSizingInstance instance = CreateInstance();
        MathematicalModel model = await GlspSchedulingFormulationFactory.CreateDefault().BuildAsync(instance);

        Assert.Contains(model.Variables, variable =>
            variable.DomainKey.StartsWith(MathematicalDecisionCategory.MicroPeriodProduction + "|", StringComparison.Ordinal));
        Assert.Contains(model.Variables, variable =>
            variable.DomainKey.StartsWith(MathematicalDecisionCategory.MicroPeriodSetupState + "|", StringComparison.Ordinal));
        Assert.Contains(model.Variables, variable =>
            variable.DomainKey.StartsWith(MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover + "|", StringComparison.Ordinal));
        Assert.Contains(model.Constraints, constraint => constraint.Name == "glspAggregateProduction_r1_t1");
        Assert.Contains(model.Constraints, constraint => constraint.Name == "glspSingleSetup_t1_s1");
        Assert.Contains(model.Constraints, constraint => constraint.Name == "glspMacroCapacity_p1_w1_t1");
        Assert.Contains(model.Constraints, constraint =>
            constraint.Name.StartsWith("glspChangeoverLower", StringComparison.Ordinal));
    }

    private static LotSizingInstance CreateInstance()
    {
        const int horizon = 2;
        var chain = new SupplyChain(horizon);
        chain.Items.Add(new Item(1, "I1", 0));
        chain.Items.Add(new Item(2, "I2", 0));

        var profile = new ProductionSchedulingProfile
        {
            BucketMode = SchedulingBucketMode.MacroMicro,
            MicroPeriodLengthMode = MicroPeriodLengthMode.Variable,
            MicroPeriodAssignmentMode = MicroPeriodAssignmentMode.SingleItem,
            SetupCarryOverPolicy = SetupCarryOverPolicy.Allowed,
            MicroPeriodCount = new MicroPeriodCount(horizon, 2)
        };

        profile.Changeovers.Add(new ProductionChangeover
        {
            FromItemId = 1,
            ToItemId = 2,
            ChangeoverTime = new SequenceDependentChangeoverTime(horizon, 1.0),
            ChangeoverCost = new SequenceDependentChangeoverCost(horizon, 3.0)
        });
        profile.Changeovers.Add(new ProductionChangeover
        {
            FromItemId = 2,
            ToItemId = 1,
            ChangeoverTime = new SequenceDependentChangeoverTime(horizon, 2.0),
            ChangeoverCost = new SequenceDependentChangeoverCost(horizon, 4.0)
        });

        var workCenter = new WorkCenter(1, "M1")
        {
            CapacityConstraint = new CapacityConstraint(horizon, 10.0),
            SchedulingProfile = profile
        };
        var plant = new Plant(1, "P1", new PlantWarehouse("P1-Warehouse"));
        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        AddRouting(chain, 1, 1, horizon);
        AddRouting(chain, 2, 2, horizon);
        return new LotSizingInstance(chain, "glsp-formulation-test");
    }

    private static void AddRouting(SupplyChain chain, int routingId, int itemId, int horizon)
    {
        var routing = new ProductionRouting(routingId, itemId, 1, 0);
        routing.AddWorkCenter(1);
        chain.ProductionRoutings.Add(routing);
        chain.ProductionCharacteristics.Add(new ProductionCharacteristic(itemId, 1, 1)
        {
            UnitCapacityConsumption = new UnitCapacityConsumption(horizon, 1.0)
        });
    }
}
