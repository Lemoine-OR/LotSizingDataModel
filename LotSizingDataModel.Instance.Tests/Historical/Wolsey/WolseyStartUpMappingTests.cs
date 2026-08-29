using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Historical.BitranYanasse;
using LotSizingDataModel.Instance.Historical.Wolsey;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Historical.Wolsey;

public sealed class WolseyStartUpMappingTests
{
    [Fact]
    public void WolseySt_MapsToGenericStartUpTimeExactly()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.CC,
                new[]
                {
                    WolseyVariant.ST
                });

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Exact,
            mapping.Coverage);

        Assert.Empty(
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Contains(
            UniversalNotationFeature.StartUpTime,
            mapping.UniversalSpecification.Notation
                .Beta.Features);
    }

    [Fact]
    public void WolseyConstantStartUpTime_UsesGenericTemporalQualifier()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.CC,
                new[]
                {
                    WolseyVariant.STConstant
                });

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Exact,
            mapping.Coverage);

        Assert.Empty(
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Contains(
            mapping.UniversalSpecification.Notation
                .Beta.TemporalQualifiers,
            qualifier =>
                qualifier.Parameter ==
                    UniversalTemporalParameter.StartUpTime &&
                qualifier.Pattern ==
                    TemporalPatternType.Constant);
    }

    [Fact]
    public void MachineStartUpTime_MapsWithoutHistoricalStartUpGap()
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
                                WolseyMachineFeature.ST
                            }));

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Contains(
            UniversalNotationFeature.StartUpTime,
            mapping.UniversalSpecification.Notation
                .Beta.Features);

        Assert.DoesNotContain(
            "Machines.ST:StartUpTime",
            mapping.UnrepresentedHistoricalDimensions);
    }

    [Fact]
    public void WolseySales_RemainsOpenBecauseSellingPriceAloneIsNotAdditionalSales()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.CC,
                new[]
                {
                    WolseyVariant.SL
                });

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Partial,
            mapping.Coverage);

        Assert.Contains(
            "VAR.SL:AdditionalSales",
            mapping.UnrepresentedHistoricalDimensions);
    }
}
