using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class SchedulingNotationTests
{
    [Fact]
    public void GeneratorAndParser_RoundTripSchedulingFeatures()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Scheduling =
                    new SchedulingDescriptor
                    {
                        HasIntegratedScheduling = true,
                        BucketMode =
                            SchedulingBucketMode.MacroMicro,
                        HasInitialSetupState = true,
                        HasSetupCarryOver = true,
                        HasSequenceDependentChangeoverTimes = true,
                        HasSequenceDependentChangeoverCosts = true,
                        HasMaximumSetupCountConstraints = true
                    }
            };

        string text =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        UniversalLotSizingNotation parsed =
            new UniversalNotationParser()
                .Parse(text);

        Assert.Equal(text, parsed.Render());

        Assert.Contains("Sched", text, StringComparison.Ordinal);
        Assert.Contains("Bucket:MM", text, StringComparison.Ordinal);
        Assert.Contains("InitSetup", text, StringComparison.Ordinal);
        Assert.Contains("SCO", text, StringComparison.Ordinal);
        Assert.Contains("SDCT", text, StringComparison.Ordinal);
        Assert.Contains("SDCC", text, StringComparison.Ordinal);
        Assert.Contains("MaxSetup", text, StringComparison.Ordinal);
    }
}
