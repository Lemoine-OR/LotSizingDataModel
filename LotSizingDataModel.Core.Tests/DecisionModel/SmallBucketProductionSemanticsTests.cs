using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

public sealed class SmallBucketProductionSemanticsTests
{
    [Fact]
    public void MaximumProducedItemCount_UsesRealPeriodBounds()
    {
        var parameter =
            new MaximumProducedItemCount(
                planningHorizon: 3,
                defaultMaximumProducedItemCount: 1);

        parameter.SetCount(
            period: 2,
            count: 2);

        Assert.Equal(
            1,
            parameter.GetCount(1));

        Assert.Equal(
            2,
            parameter.GetCount(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                parameter.SetCount(
                    period: 3,
                    count: -1));
    }

    [Fact]
    public void SchedulingProfile_ExposesClassificationSemantics()
    {
        var profile =
            new ProductionSchedulingProfile
            {
                BucketMode =
                    SchedulingBucketMode.SmallBucket,
                SmallBucketProductionMode =
                    SmallBucketProductionMode.Continuous,
                MaximumProducedItemCount =
                    new MaximumProducedItemCount(3, 1),
                MaximumSetupCount =
                    new MaximumSetupCount(3, 1)
            };

        Assert.Equal(
            1,
            profile.MaximumProducedItemCountPerBucket);

        Assert.Equal(
            1,
            profile.MaximumSetupTransitionsPerBucket);

        profile.MaximumProducedItemCount.SetCount(
            2,
            2);

        Assert.Equal(
            2,
            profile.MaximumProducedItemCountPerBucket);

        profile.ResizeTimeSeries(5);

        Assert.Equal(
            5,
            profile.MaximumProducedItemCount.PlanningHorizon);
    }
}
