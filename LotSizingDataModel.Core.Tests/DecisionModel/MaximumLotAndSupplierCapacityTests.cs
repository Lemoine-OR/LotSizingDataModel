using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

public sealed class MaximumLotAndSupplierCapacityTests
{
    [Fact]
    public void MaximumLotSize_IsNonNegativeAndZeroIsActive()
    {
        var parameter =
            new MaximumLotSize(3, 25.0);

        Assert.Equal(25.0, parameter.GetMaximumLotSize(2));

        parameter.SetMaximumLotSize(2, 0.0);

        Assert.Equal(0.0, parameter.GetMaximumLotSize(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => parameter.SetMaximumLotSize(1, -1.0));
    }

    [Fact]
    public void ProductionRouting_MaximumLotSizeParticipatesInLifecycle()
    {
        var routing =
            new ProductionRouting
            {
                MaximumLotSize =
                    new MaximumLotSize(3, 12.0)
            };

        Assert.True(routing.HasLotSizingConstraints);
        Assert.Equal(3, routing.PlanningHorizon);

        routing.ResizeTimeSeries(5);

        Assert.Equal(
            5,
            routing.MaximumLotSize!.PlanningHorizon);

        routing.ClearLotSizingConstraints();

        Assert.Null(routing.MaximumLotSize);
    }

    [Fact]
    public void SupplierDelivery_CapacityParticipatesInLifecycle()
    {
        var delivery =
            new SupplierDelivery
            {
                CapacityConstraint =
                    new CapacityConstraint(3, 40.0)
            };

        Assert.True(delivery.HasDecisionParameters);
        Assert.True(delivery.HasConsistentPlanningHorizon);
        Assert.Equal(3, delivery.PlanningHorizon);

        delivery.ResizeTimeSeries(5);

        Assert.Equal(
            5,
            delivery.CapacityConstraint!.PlanningHorizon);

        delivery.ClearDecisionParameters();

        Assert.Null(delivery.CapacityConstraint);
    }
}
