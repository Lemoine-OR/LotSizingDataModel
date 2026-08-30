using LotSizingDataModel.Checker.Structural;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Checker.Tests.Structural;

public sealed class MacroMicroSchedulingStructuralValidatorTests
{
    [Fact]
    public void Validator_RejectsMicroPeriodOutsideInstanceGrid()
    {
        LotSizingInstance instance = CreateInstance();
        var solution = new LotSizingSolution(2);

        var schedule =
            new WorkCenterSchedulingDecision(
                new WorkCenterReference(1, 1),
                2);

        schedule.AddMicroPeriodDecision(
            new ProductionMicroPeriodDecision(
                new ProductionMicroPeriodReference(1, 3),
                setupItemId: 1));

        solution.AddWorkCenterSchedulingDecision(schedule);

        var issues =
            new MacroMicroSchedulingStructuralValidator()
                .Validate(instance, solution);

        Assert.Contains(
            issues,
            issue =>
                issue.Message.Contains(
                    "DECSCHED002",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void Validator_RejectsMissingExecutableMicroSchedule()
    {
        LotSizingInstance instance = CreateInstance();
        var solution = new LotSizingSolution(2);

        var issues = new MacroMicroSchedulingStructuralValidator().Validate(instance, solution);

        Assert.Contains(
            issues,
            issue => issue.Message.Contains("DECSCHED005", StringComparison.Ordinal));
    }

    private static LotSizingInstance CreateInstance()
    {
        var chain = new SupplyChain(2);
        chain.Items.Add(new Item(1, "I1", 0));

        var workCenter =
            new WorkCenter(1, "M1")
            {
                SchedulingProfile =
                    new ProductionSchedulingProfile
                    {
                        BucketMode = SchedulingBucketMode.MacroMicro,
                        MicroPeriodLengthMode = MicroPeriodLengthMode.Variable,
                        MicroPeriodAssignmentMode =
                            MicroPeriodAssignmentMode.SingleItem,
                        MicroPeriodCount = new MicroPeriodCount(2, 2)
                    }
            };

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        var routing =
            new ProductionRouting(1, 1, 1, 0);

        routing.AddWorkCenter(1);
        chain.ProductionRoutings.Add(routing);

        return new LotSizingInstance(
            chain,
            "macro-micro-structural-test");
    }
}
