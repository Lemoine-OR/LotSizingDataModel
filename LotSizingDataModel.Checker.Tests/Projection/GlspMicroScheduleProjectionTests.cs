using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Projection;

public sealed class GlspMicroScheduleProjectionTests
{
    [Fact]
    public void Projector_ReconstructsMicroProductionStateAndChangeover()
    {
        var solution = new LotSizingSolution(1);
        var schedule = new WorkCenterSchedulingDecision(new WorkCenterReference(1, 1), 1);
        schedule.AddMicroPeriodDecision(new ProductionMicroPeriodDecision(
            new ProductionMicroPeriodReference(1, 1), setupItemId: 1, routingId: 1, quantity: 3.0));
        schedule.AddMicroPeriodDecision(new ProductionMicroPeriodDecision(
            new ProductionMicroPeriodReference(1, 2), setupItemId: 2));
        solution.AddWorkCenterSchedulingDecision(schedule);

        var model = new MathematicalModel { Name = "glsp-projection" };
        model.Variables.Add(Variable(1, MathematicalDecisionCategory.MicroPeriodProduction, 1, 1, 1, 1));
        model.Variables.Add(Variable(2, MathematicalDecisionCategory.MicroPeriodSetupState, 1, 1, 1, 1));
        model.Variables.Add(ChangeoverVariable(3, 1, 2, 1, 2));

        MathematicalSolutionProjectionResult result =
            new MathematicalSolutionValueProjector().Project(model, solution);

        Assert.Empty(result.Issues);
        Assert.Equal(3.0, result.Values[1]);
        Assert.Equal(1.0, result.Values[2]);
        Assert.Equal(1.0, result.Values[3]);
    }

    private static MathematicalVariable Variable(
        int id, string category, int routingId, int itemId, int period, int micro) => new()
    {
        Id = id,
        Name = $"v{id}",
        DomainKey = new MathematicalDomainKeyBuilder(category)
            .Add(MathematicalDomainKeySegment.Plant, 1)
            .Add(MathematicalDomainKeySegment.WorkCenter, 1)
            .Add(MathematicalDomainKeySegment.Period, period)
            .Add(MathematicalDomainKeySegment.MicroPeriod, micro)
            .Add(MathematicalDomainKeySegment.Routing, routingId)
            .Add(MathematicalDomainKeySegment.Item, itemId)
            .Build(),
        VariableType = category == MathematicalDecisionCategory.MicroPeriodProduction
            ? MathematicalVariableType.Continuous
            : MathematicalVariableType.Binary,
        LowerBound = 0.0,
        UpperBound = category == MathematicalDecisionCategory.MicroPeriodProduction
            ? double.PositiveInfinity
            : 1.0
    };

    private static MathematicalVariable ChangeoverVariable(
        int id, int fromItemId, int toItemId, int period, int micro) => new()
    {
        Id = id,
        Name = $"v{id}",
        DomainKey = new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover)
            .Add(MathematicalDomainKeySegment.Plant, 1)
            .Add(MathematicalDomainKeySegment.WorkCenter, 1)
            .Add(MathematicalDomainKeySegment.Period, period)
            .Add(MathematicalDomainKeySegment.MicroPeriod, micro)
            .Add(MathematicalDomainKeySegment.FromItem, fromItemId)
            .Add(MathematicalDomainKeySegment.ToItem, toItemId)
            .Build(),
        VariableType = MathematicalVariableType.Binary,
        LowerBound = 0.0,
        UpperBound = 1.0
    };
}
