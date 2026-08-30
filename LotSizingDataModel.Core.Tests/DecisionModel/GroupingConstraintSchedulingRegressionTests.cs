using LotSizingDataModel.Core.DecisionModel.Constraints;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

/// <summary>
/// Guards the existing grouping semantic while scheduling concepts are added.
/// </summary>
public sealed class GroupingConstraintSchedulingRegressionTests
{
    [Fact]
    public void GroupingWindowValue_RemainsStrictlyPositivePeriodDistance()
    {
        var grouping =
            new GroupingConstraint(
                planningHorizon: 5,
                defaultGroupingPeriodCount: 3);

        Assert.Equal(
            3,
            grouping.GetGroupingPeriodCount(1));

        grouping.SetGroupingPeriodCount(2, 1);

        Assert.Equal(
            1,
            grouping.GetGroupingPeriodCount(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                grouping.SetGroupingPeriodCount(
                    3,
                    0));
    }
}
