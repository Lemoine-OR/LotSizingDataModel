using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class MultiLevelProblemClassAnalyzerTests
{
    private readonly LotSizingProblemClassAnalyzer _analyzer =
        new();

    [Theory]
    [InlineData(ProductStructureType.Serial)]
    [InlineData(ProductStructureType.Assembly)]
    [InlineData(ProductStructureType.Arborescent)]
    [InlineData(ProductStructureType.General)]
    public void Umlsp_AcceptsAllRepresentedMultiLevelBomTopologies(
        ProductStructureType productStructure)
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                productStructure,
                capacitated: false);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .UncapacitatedMultiLevel);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void Mlclsp_CapacitatedMultiLevelCore_IsExact()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                ProductStructureType.General,
                capacitated: true);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .MultiLevelCapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void Umlsp_RejectsCapacitatedProduction()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                ProductStructureType.General,
                capacitated: true);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .UncapacitatedMultiLevel);

        Assert.Equal(
            LotSizingProblemClassMatchKind.NotApplicable,
            result.Kind);
    }

    [Fact]
    public void Mlclsp_RejectsSingleLevelDescriptor()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 3,
                    PlanningHorizon = 6,
                    ProductStructureRelationshipCount = 0,
                    ProductStructureType =
                        ProductStructureType.IndependentItems,
                    HasDemand = true,
                    HasDeterministicDemand = true,
                    HasProduction = true,
                    HasProductionCapacityConstraints = true,
                    HasSetupCosts = true
                });

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .MultiLevelCapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.NotApplicable,
            result.Kind);
    }

    [Fact]
    public void LeadTimesRemainExplicitClassExtension()
    {
        LotSizingProblemFeatures features =
            BaseFeatures(
                ProductStructureType.General,
                capacitated: true);

        features.HasProductionLeadTimes = true;

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                LotSizingProblemDescriptor.FromLegacyFeatures(
                    features),
                LotSizingProblemClassCatalog
                    .MultiLevelCapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.CompatibleExtension,
            result.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.ProductionLeadTimes,
            result.Extensions);
    }

    private static LotSizingProblemDescriptor Descriptor(
        ProductStructureType productStructure,
        bool capacitated) =>
            LotSizingProblemDescriptor.FromLegacyFeatures(
                BaseFeatures(
                    productStructure,
                    capacitated));

    private static LotSizingProblemFeatures BaseFeatures(
        ProductStructureType productStructure,
        bool capacitated) =>
            new()
            {
                ItemCount = 5,
                PlanningHorizon = 8,
                ProductStructureRelationshipCount = 4,
                MaximumProductStructureDepth = 3,
                ProductStructureType = productStructure,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasTimeVaryingDemand = true,
                HasProduction = true,
                HasProductionCapacityConstraints = capacitated,
                HasSetupCosts = true
            };
}
