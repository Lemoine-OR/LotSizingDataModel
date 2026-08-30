using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

public sealed class ProductionSchedulingProfileTests
{
    [Fact]
    public void SchedulingProfile_ResizesAllPeriodDependentSemantics()
    {
        var profile =
            new ProductionSchedulingProfile
            {
                BucketMode =
                    SchedulingBucketMode.MacroMicro,
                SetupCarryOverPolicy =
                    SetupCarryOverPolicy.Allowed,
                InitialSetupItemId = 1,
                MicroPeriodCount =
                    new MicroPeriodCount(3, 2),
                MaximumSetupCount =
                    new MaximumSetupCount(3, 1)
            };

        profile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId = 1,
                ToItemId = 2,
                ChangeoverTime =
                    new SequenceDependentChangeoverTime(3, 0.5),
                ChangeoverCost =
                    new SequenceDependentChangeoverCost(3, 10.0)
            });

        Assert.True(profile.HasInitialSetupState);
        Assert.True(profile.HasSetupCarryOver);
        Assert.True(profile.HasSequenceDependentChangeoverTimes);
        Assert.True(profile.HasSequenceDependentChangeoverCosts);
        Assert.True(profile.HasConsistentPlanningHorizon);
        Assert.Equal(3, profile.PlanningHorizon);

        profile.ResizeTimeSeries(5);

        Assert.Equal(5, profile.PlanningHorizon);
        Assert.Equal(5, profile.MicroPeriodCount!.PlanningHorizon);
        Assert.Equal(5, profile.MaximumSetupCount!.PlanningHorizon);
        Assert.Equal(5, profile.Changeovers[0].PlanningHorizon);
    }

    [Fact]
    public void WorkCenter_SchedulingProfileParticipatesInDecisionLifecycle()
    {
        var workCenter =
            new WorkCenter
            {
                SchedulingProfile =
                    new ProductionSchedulingProfile
                    {
                        MaximumSetupCount =
                            new MaximumSetupCount(3, 1)
                    }
            };

        Assert.True(workCenter.HasDecisionParameters);
        Assert.Equal(3, workCenter.PlanningHorizon);

        workCenter.ResizeTimeSeries(4);

        Assert.Equal(
            4,
            workCenter.SchedulingProfile!
                .MaximumSetupCount!
                .PlanningHorizon);

        workCenter.ClearDecisionParameters();

        Assert.Null(workCenter.SchedulingProfile);
    }

    [Fact]
    public void Changeover_IsDirectionalAndNonTrivial()
    {
        var changeover =
            new ProductionChangeover
            {
                FromItemId = 1,
                ToItemId = 2
            };

        Assert.True(changeover.IsNonTrivialTransition);

        changeover.ToItemId = 1;

        Assert.False(changeover.IsNonTrivialTransition);
    }
}
