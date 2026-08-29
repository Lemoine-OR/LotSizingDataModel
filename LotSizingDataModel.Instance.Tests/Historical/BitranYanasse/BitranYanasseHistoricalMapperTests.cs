using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Historical.BitranYanasse;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Tests.Historical.BitranYanasse;

public sealed class BitranYanasseHistoricalMapperTests
{
    [Fact]
    public void Map_PreservesCompleteHistoricalCode()
    {
        BitranYanasseTemporalProfile profile =
            new BitranYanasseProfileAnalyzer()
                .Analyze(
                    setupCost: new[] { 9.0, 8.0, 4.0 },
                    holdingCost: new[] { 2.0, 4.0, 1.0 },
                    productionCost: new[] { 7.0, 6.0, 3.0 },
                    capacity: new[] { 10.0, 12.0, 15.0 });

        BitranYanasseHistoricalMapping mapping =
            new BitranYanasseHistoricalMapper()
                .Map(profile);

        Assert.Equal(
            "NI/G/NI/ND",
            mapping.HistoricalCode);

        Assert.Equal(
            HistoricalMappingCoverage.Partial,
            mapping.Coverage);

        Assert.Equal(
            4,
            mapping.UnrepresentedHistoricalDimensions.Count);
    }

    [Fact]
    public void Map_UsesConservativeUniversalDomainProjection()
    {
        BitranYanasseTemporalProfile profile =
            CreateProfile();

        BitranYanasseHistoricalMapping mapping =
            new BitranYanasseHistoricalMapper()
                .Map(profile);

        Assert.Equal(
            "1,SL,Net:UNK | Dem,Det,Prod,Cap:P | Obj:Econ",
            mapping.UniversalDomainSpecification.CanonicalText);
    }

    [Fact]
    public void AssessApplicability_ClassicalDescriptor_IsExactHistoricalDomain()
    {
        LotSizingProblemDescriptor descriptor =
            CreateClassicalDescriptor();

        BitranYanasseApplicabilityAssessment assessment =
            new BitranYanasseHistoricalMapper()
                .AssessApplicability(descriptor);

        Assert.Equal(
            BitranYanasseApplicabilityKind.ExactHistoricalDomain,
            assessment.Kind);

        Assert.True(assessment.IsApplicable);
        Assert.Empty(assessment.FailedRequirements);
        Assert.Empty(assessment.Extensions);
    }

    [Fact]
    public void AssessApplicability_ExtendedDescriptor_IsProjectableButNotClassical()
    {
        LotSizingProblemFeatures features =
            CreateClassicalFeatures();

        features.HasBacklogging = true;
        features.HasMinimumLotSizes = true;

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        BitranYanasseApplicabilityAssessment assessment =
            new BitranYanasseHistoricalMapper()
                .AssessApplicability(descriptor);

        Assert.Equal(
            BitranYanasseApplicabilityKind.ExtendedButProjectable,
            assessment.Kind);

        Assert.Contains(
            "backlogging",
            assessment.Extensions);

        Assert.Contains(
            "lotSizeRestrictions",
            assessment.Extensions);
    }

    [Fact]
    public void AssessApplicability_MultiItemDescriptor_IsNotApplicable()
    {
        LotSizingProblemFeatures features =
            CreateClassicalFeatures();

        features.ItemCount = 3;

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        BitranYanasseApplicabilityAssessment assessment =
            new BitranYanasseHistoricalMapper()
                .AssessApplicability(descriptor);

        Assert.Equal(
            BitranYanasseApplicabilityKind.NotApplicable,
            assessment.Kind);

        Assert.Contains(
            "singleItem",
            assessment.FailedRequirements);
    }

    [Fact]
    public void UniversalDomainProjection_MatchesClassicalDescriptor()
    {
        LotSizingProblemDescriptor descriptor =
            CreateClassicalDescriptor();

        UniversalNotationMatchResult match =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    BitranYanasseHistoricalMapper
                        .ClassicalDomainSpecification);

        Assert.Equal(
            UniversalNotationMatchKind.Compatible,
            match.Kind);
    }

    [Fact]
    public void MappingWithDescriptor_EmbedsApplicabilityAssessment()
    {
        BitranYanasseHistoricalMapping mapping =
            new BitranYanasseHistoricalMapper()
                .Map(
                    CreateProfile(),
                    CreateClassicalDescriptor());

        Assert.NotNull(mapping.Applicability);

        Assert.Equal(
            BitranYanasseApplicabilityKind.ExactHistoricalDomain,
            mapping.Applicability!.Kind);
    }

    private static BitranYanasseTemporalProfile CreateProfile()
    {
        return new BitranYanasseProfileAnalyzer()
            .Analyze(
                setupCost: new[] { 1.0, 1.0, 1.0 },
                holdingCost: new[] { 2.0, 2.0, 2.0 },
                productionCost: new[] { 0.0, 0.0, 0.0 },
                capacity: new[] { 10.0, 10.0, 10.0 });
    }

    private static LotSizingProblemDescriptor
        CreateClassicalDescriptor()
    {
        return LotSizingProblemDescriptor
            .FromLegacyFeatures(
                CreateClassicalFeatures());
    }

    private static LotSizingProblemFeatures
        CreateClassicalFeatures()
    {
        return new LotSizingProblemFeatures
        {
            ItemCount = 1,
            PlanningHorizon = 6,
            ProductStructureRelationshipCount = 0,
            ProductStructureType =
                ProductStructureType.IndependentItems,
            HasDemand = true,
            HasDeterministicDemand = true,
            HasTimeVaryingDemand = true,
            HasProduction = true,
            HasProductionCapacityConstraints = true,
            HasSetupCosts = true
        };
    }
}
