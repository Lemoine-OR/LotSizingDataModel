using LotSizingDataModel.Instance.Historical.Wolsey;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Historical.Wolsey;

public sealed class WolseyChangeoverMappingTests
{
    [Fact]
    public void MachineSqtAndSqc_MapToGenericChangeoverSemantics()
    {
        var classification =
            new WolseyExtendedClassification(
                new WolseySingleItemClassification(
                    WolseyProblemVersion.LS,
                    WolseyCapacityRegime.CC),
                machines:
                    new WolseyMachineClassification(
                        machineCount: 2,
                        machineMode: WolseyMachineMode.IM,
                        bucketType: WolseyBucketType.BB,
                        features:
                            new[]
                            {
                                WolseyMachineFeature.SQT,
                                WolseyMachineFeature.SQC
                            }));

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Contains(
            UniversalNotationFeature.SequenceDependentChangeoverTime,
            mapping.UniversalSpecification.Notation.Beta.Features);

        Assert.Contains(
            UniversalNotationFeature.SequenceDependentChangeoverCost,
            mapping.UniversalSpecification.Notation.Beta.Features);

        Assert.DoesNotContain(
            "Machines.SQT:SequenceDependentChangeoverTime",
            mapping.UnrepresentedHistoricalDimensions);

        Assert.DoesNotContain(
            "Machines.SQC:SequenceDependentChangeoverCost",
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Contains(
            mapping.UnrepresentedHistoricalDimensions,
            value =>
                value.StartsWith(
                    "Machines.Bucket=",
                    StringComparison.Ordinal));
    }
}
