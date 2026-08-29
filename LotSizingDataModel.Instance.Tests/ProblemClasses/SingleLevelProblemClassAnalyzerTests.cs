using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class SingleLevelProblemClassAnalyzerTests
{
    private readonly LotSizingProblemClassAnalyzer _analyzer =
        new();

    [Fact]
    public void SingleItemUncapacitatedCore_IsExact()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                itemCount: 1,
                capacitated: false);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .SingleItemUncapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);

        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void SingleItemCapacitatedCore_IsExact()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                itemCount: 1,
                capacitated: true);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .SingleItemCapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void MultiItemClsp_RequiresSharedCapacity()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                itemCount: 4,
                capacitated: true,
                sharedCapacity: false);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .MultiItemCapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.NotApplicable,
            result.Kind);
    }

    [Fact]
    public void MultiItemSharedCapacityClsp_IsExact()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                itemCount: 4,
                capacitated: true,
                sharedCapacity: true);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .MultiItemCapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void Backlogging_IsClassifiedAsCompatibleExtension()
    {
        LotSizingProblemFeatures features =
            BaseFeatures(
                itemCount: 1,
                capacitated: false);

        features.HasBacklogging = true;

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .SingleItemUncapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.CompatibleExtension,
            result.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.Backlogging,
            result.Extensions);
    }

    [Fact]
    public void UnknownItemCount_IsIncomplete()
    {
        LotSizingProblemFeatures features =
            BaseFeatures(
                itemCount: 0,
                capacitated: false);

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        LotSizingProblemClassMatchResult result =
            _analyzer.Assess(
                descriptor,
                LotSizingProblemClassCatalog
                    .SingleItemUncapacitated);

        Assert.Equal(
            LotSizingProblemClassMatchKind.Incomplete,
            result.Kind);
    }

    private static LotSizingProblemDescriptor Descriptor(
        int itemCount,
        bool capacitated,
        bool sharedCapacity = false) =>
            LotSizingProblemDescriptor.FromLegacyFeatures(
                BaseFeatures(
                    itemCount,
                    capacitated,
                    sharedCapacity));

    private static LotSizingProblemFeatures BaseFeatures(
        int itemCount,
        bool capacitated,
        bool sharedCapacity = false) =>
            new()
            {
                ItemCount = itemCount,
                PlanningHorizon = 6,
                ProductStructureRelationshipCount = 0,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasTimeVaryingDemand = true,
                HasProduction = true,
                HasProductionCapacityConstraints = capacitated,
                HasSharedProductionCapacity =
                    capacitated && sharedCapacity,
                HasSetupCosts = true
            };
}
