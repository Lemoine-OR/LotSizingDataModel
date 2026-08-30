using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

public sealed class MacroMicroSchedulingProfileTests
{
    [Fact]
    public void Profile_EnumeratesOrderedVariableLengthMacroMicroGrid()
    {
        var count =
            new MicroPeriodCount(
                planningHorizon: 3,
                defaultMicroPeriodCount: 2);

        count.SetCount(2, 3);
        count.SetCount(3, 1);

        var profile =
            new ProductionSchedulingProfile
            {
                BucketMode = SchedulingBucketMode.MacroMicro,
                MicroPeriodLengthMode = MicroPeriodLengthMode.Variable,
                MicroPeriodAssignmentMode = MicroPeriodAssignmentMode.SingleItem,
                MicroPeriodCount = count
            };

        Assert.True(profile.HasExplicitMicroPeriodGrid);
        Assert.True(profile.HasVariableLengthMicroPeriods);
        Assert.True(profile.HasSingleItemPerMicroPeriod);
        Assert.True(profile.HasVariableMicroPeriodCount);
        Assert.Equal(6, profile.TotalMicroPeriodCount);
        Assert.Equal(3, profile.MaximumMicroPeriodCountPerMacroPeriod);

        ProductionMicroPeriodReference[] grid =
            profile.EnumerateMicroPeriods().ToArray();

        Assert.Equal(6, grid.Length);
        Assert.Equal(
            (2, 3),
            (grid[4].MacroPeriod, grid[4].MicroPeriodIndex));
    }
}
