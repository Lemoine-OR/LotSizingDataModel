using System.IO;
using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Checker.Tests.Solution;

public sealed class MacroMicroSchedulingDecisionTests
{
    [Fact]
    public void Solution_StoresAndSerializesWorkCenterMicroSchedule()
    {
        var solution = new LotSizingSolution(2);

        var schedule =
            new WorkCenterSchedulingDecision(
                new WorkCenterReference(1, 1),
                2);

        schedule.AddMicroPeriodDecision(
            new ProductionMicroPeriodDecision(
                new ProductionMicroPeriodReference(1, 1),
                setupItemId: 1,
                routingId: 1,
                quantity: 4.0));

        schedule.AddMicroPeriodDecision(
            new ProductionMicroPeriodDecision(
                new ProductionMicroPeriodReference(1, 2),
                setupItemId: 2));

        solution.AddWorkCenterSchedulingDecision(schedule);

        Assert.Equal(1, solution.DecisionCount);
        Assert.True(solution.IsInternallyValid);

        var serializer =
            new XmlSerializer(typeof(LotSizingSolution));

        string xml;
        using (var writer = new StringWriter())
        {
            serializer.Serialize(writer, solution);
            xml = writer.ToString();
        }

        LotSizingSolution clone;
        using (var reader = new StringReader(xml))
        {
            clone =
                (LotSizingSolution)serializer.Deserialize(reader)!;
        }

        WorkCenterSchedulingDecision cloned =
            Assert.Single(clone.WorkCenterSchedulingDecisions);

        Assert.Equal(2, cloned.MicroPeriodDecisionCount);
        Assert.Equal(4.0, cloned.GetMicroPeriods(1)[0].Quantity);
    }

    [Fact]
    public void Resize_RemovesMicroPeriodsOutsideNewMacroHorizon()
    {
        var schedule =
            new WorkCenterSchedulingDecision(
                new WorkCenterReference(1, 1),
                3);

        schedule.AddMicroPeriodDecision(
            new ProductionMicroPeriodDecision(
                new ProductionMicroPeriodReference(3, 1),
                setupItemId: 1));

        schedule.ResizeTimeSeries(2);

        Assert.Empty(schedule.MicroPeriods);
        Assert.Equal(2, schedule.PlanningHorizon);
    }
}
