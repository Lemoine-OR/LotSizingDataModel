using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

public sealed class StartUpParameterTests
{
    [Fact]
    public void StartUpCost_IsNonNegativePeriodParameter()
    {
        var parameter =
            new StartUpCost(
                planningHorizon: 3,
                defaultStartUpCost: 12.5);

        Assert.Equal(3, parameter.PlanningHorizon);
        Assert.Equal(12.5, parameter.GetStartUpCost(1));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => parameter.SetStartUpCost(2, -1.0));
    }

    [Fact]
    public void StartUpTime_IsNonNegativePeriodParameter()
    {
        var parameter =
            new StartUpTime(
                planningHorizon: 4,
                defaultStartUpTime: 2.0);

        Assert.Equal(4, parameter.PlanningHorizon);
        Assert.Equal(2.0, parameter.GetStartUpTime(4));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => parameter.SetStartUpTime(1, -0.5));
    }

    [Fact]
    public void ProductionCharacteristic_StartUpParametersParticipateInDecisionModelLifecycle()
    {
        var characteristic =
            new ProductionCharacteristic(
                itemId: 1,
                plantId: 1,
                workCenterId: 1)
            {
                StartUpCost =
                    new StartUpCost(3, 5.0),
                StartUpTime =
                    new StartUpTime(3, 1.0)
            };

        Assert.True(characteristic.HasDecisionParameters);
        Assert.True(
            characteristic.RequiresCapacityConstrainedWorkCenter);
        Assert.True(characteristic.HasConsistentPlanningHorizon);
        Assert.Equal(3, characteristic.PlanningHorizon);

        characteristic.ResizeTimeSeries(5);

        Assert.Equal(5, characteristic.StartUpCost!.PlanningHorizon);
        Assert.Equal(5, characteristic.StartUpTime!.PlanningHorizon);

        characteristic.ClearDecisionParameters();

        Assert.Null(characteristic.StartUpCost);
        Assert.Null(characteristic.StartUpTime);
        Assert.False(characteristic.HasDecisionParameters);
    }
}
