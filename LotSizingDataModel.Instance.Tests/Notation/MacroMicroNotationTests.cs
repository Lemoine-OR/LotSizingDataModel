using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class MacroMicroNotationTests
{
    [Fact]
    public void MacroMicroGrid_RendersAndRoundTripsGenericTokens()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Scheduling =
                    new SchedulingDescriptor
                    {
                        HasIntegratedScheduling = true,
                        BucketMode = SchedulingBucketMode.MacroMicro,
                        SchedulingResourceCount = 1,
                        MicroPeriodLengthMode = MicroPeriodLengthMode.Variable,
                        MicroPeriodAssignmentMode =
                            MicroPeriodAssignmentMode.SingleItem,
                        HasExplicitMicroPeriodGrid = true,
                        TotalMicroPeriodCount = 12,
                        MaximumMicroPeriodCountPerMacroPeriod = 4,
                        HasVariableMicroPeriodCount = true
                    }
            };

        string notation =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.Contains("Bucket:MM", notation, StringComparison.Ordinal);
        Assert.Contains("MicroGrid", notation, StringComparison.Ordinal);
        Assert.Contains("MicroLen:Var", notation, StringComparison.Ordinal);
        Assert.Contains("MicroItem:1", notation, StringComparison.Ordinal);
        Assert.Contains("MicroN:Var", notation, StringComparison.Ordinal);

        Assert.Equal(
            notation,
            new UniversalNotationParser()
                .Parse(notation)
                .Render());
    }
}
