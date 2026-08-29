using LotSizingDataModel.Instance.Historical.BitranYanasse;
using LotSizingDataModel.Instance.Historical.Wolsey;

namespace LotSizingDataModel.Instance.Tests.Historical.Wolsey;

public sealed class WolseyHistoricalMappingTests
{
    [Fact]
    public void SingleItemClassification_RendersWolseyFieldsCanonically()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.DLSI,
                WolseyCapacityRegime.CC,
                new[]
                {
                    WolseyVariant.B,
                    WolseyVariant.ST
                });

        Assert.Equal(
            "DLSI-CC-{B,ST}",
            classification.HistoricalCode);
    }

    [Fact]
    public void EmptyVariantField_IsOmittedLikeWolseyWagnerWhitinExample()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.WW,
                WolseyCapacityRegime.U);

        Assert.Equal(
            "WW-U",
            classification.HistoricalCode);
    }

    [Fact]
    public void HistoricalVariantSemantics_AreNotConflated()
    {
        Assert.NotEqual(
            WolseyVariant.SC,
            WolseyVariant.ST);

        Assert.NotEqual(
            WolseyVariant.SL,
            WolseyVariant.SS);

        Assert.Equal(
            "SC",
            WolseyVariant.SC.ToString());

        Assert.Equal(
            "SL",
            WolseyVariant.SL.ToString());
    }

    [Fact]
    public void RepresentableLsConstantCapacityVariants_MapExactly()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.CC,
                new[]
                {
                    WolseyVariant.B,
                    WolseyVariant.SC,
                    WolseyVariant.LB,
                    WolseyVariant.SS
                });

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Exact,
            mapping.Coverage);

        Assert.Empty(
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Equal(
            "1,SL,Net:UNK | " +
            "Dem,Det,Prod,Cap:P,SC,SU,MinLot,SS,BL," +
            "TP:CapP=C | Obj:Econ",
            mapping.UniversalSpecification.CanonicalText);
    }

    [Fact]
    public void WagnerWhitinCondition_IsPreservedAsUnrepresentedNotGuessed()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.WW,
                WolseyCapacityRegime.CC);

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Partial,
            mapping.Coverage);

        Assert.Contains(
            "PROB.WW:WagnerWhitinCostCondition",
            mapping.UnrepresentedHistoricalDimensions);
    }

    [Fact]
    public void UncapacitatedRegime_MapsExplicitlyAndExactly()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.U);

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Exact,
            mapping.Coverage);

        Assert.Empty(
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Equal(
            "1,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:Econ",
            mapping.UniversalSpecification.CanonicalText);
    }

    [Fact]
    public void WolseyScMeansStartupCostAndMapsToUniversalSu()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.C,
                new[]
                {
                    WolseyVariant.SC
                });

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Contains(
            "SU",
            mapping.UniversalSpecification.Notation
                .Beta.Features
                .Select(
                    feature =>
                        feature ==
                        LotSizingDataModel.Instance.Notation
                            .UniversalNotationFeature.StartUpCost
                            ? "SU"
                            : feature.ToString()));

        Assert.DoesNotContain(
            "VAR.SC",
            mapping.UnrepresentedHistoricalDimensions);
    }

    [Fact]
    public void SalesVariant_IsNotMappedToLostSales()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.C,
                new[]
                {
                    WolseyVariant.SL
                });

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Contains(
            "VAR.SL:AdditionalSales",
            mapping.UnrepresentedHistoricalDimensions);

        Assert.DoesNotContain(
            LotSizingDataModel.Instance.Notation
                .UniversalNotationFeature.LostSales,
            mapping.UniversalSpecification.Notation
                .Beta.Features);
    }

    [Fact]
    public void ExtendedClassification_PreservesMachineAndLevelBlocks()
    {
        var singleItem =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.C);

        var machines =
            new WolseyMachineClassification(
                machineCount: 2,
                machineMode: WolseyMachineMode.IM,
                bucketType: WolseyBucketType.BB,
                hasLeadTimes: true,
                features:
                    new[]
                    {
                        WolseyMachineFeature.SET,
                        WolseyMachineFeature.SQC
                    });

        var levels =
            new WolseyMultiLevelClassification(
                levelCount: 3,
                structure:
                    WolseyMultiLevelStructure.A);

        var classification =
            new WolseyExtendedClassification(
                singleItem,
                itemCount: 5,
                periodCount: 12,
                machines: machines,
                multiLevel: levels);

        Assert.Equal(
            "{NL=3,A}{NK=2,IM,LT,BB,SET,SQC}" +
            "{NI=5}{NT=12}{LS-C}",
            classification.HistoricalCode);

        WolseyHistoricalMapping mapping =
            new WolseyHistoricalMapper()
                .Map(classification);

        Assert.Equal(
            HistoricalMappingCoverage.Partial,
            mapping.Coverage);

        Assert.Contains(
            "Machines.SQC:SequenceDependentChangeoverCost",
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Contains(
            "NI.ExactCount=5",
            mapping.UnrepresentedHistoricalDimensions);

        Assert.Contains(
            "NL.ExactCount=3",
            mapping.UnrepresentedHistoricalDimensions);
    }

    [Fact]
    public void ConstantLowerBound_UsesGenericMinimumLotTemporalQualifier()
    {
        var classification =
            new WolseySingleItemClassification(
                WolseyProblemVersion.LS,
                WolseyCapacityRegime.CC,
                new[]
                {
                    WolseyVariant.LBConstant
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
                    LotSizingDataModel.Instance.Notation
                        .UniversalTemporalParameter.MinimumLotSize &&
                qualifier.Pattern ==
                    LotSizingDataModel.Instance.Descriptors.Temporal
                        .TemporalPatternType.Constant);
    }
}
