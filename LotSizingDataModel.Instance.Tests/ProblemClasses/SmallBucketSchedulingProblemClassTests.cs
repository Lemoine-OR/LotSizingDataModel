using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class SmallBucketSchedulingProblemClassTests
{
    [Fact]
    public void Dlsp_IsUniquelyClassified()
    {
        LotSizingProblemClassMatchResult result =
            AssertSingle(
                SmallBucketProductionMode.AllOrNothing,
                maximumProducedItems: 1,
                maximumSetupTransitions: 0,
                hasSetupLimit: false);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .DiscreteLotSizingAndScheduling,
            result.Definition.Id);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void Cslp_IsUniquelyClassified()
    {
        LotSizingProblemClassMatchResult result =
            AssertSingle(
                SmallBucketProductionMode.Continuous,
                maximumProducedItems: 1,
                maximumSetupTransitions: 0,
                hasSetupLimit: false);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .ContinuousSetupLotSizing,
            result.Definition.Id);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void Plsp_IsUniquelyClassified()
    {
        LotSizingProblemClassMatchResult result =
            AssertSingle(
                SmallBucketProductionMode.Continuous,
                maximumProducedItems: 2,
                maximumSetupTransitions: 1,
                hasSetupLimit: true);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .ProportionalLotSizingAndScheduling,
            result.Definition.Id);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void SchedulingDescriptor_NeverFallsBackToGenericMiClsp()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                SmallBucketProductionMode.Unspecified,
                maximumProducedItems: 0,
                maximumSetupTransitions: 0,
                hasProducedItemLimit: false,
                hasSetupLimit: false);

        IReadOnlyList<LotSizingProblemClassMatchResult> matches =
            new LotSizingProblemClassDetector()
                .Detect(descriptor);

        Assert.Empty(matches);
    }

    [Fact]
    public void Plsp_WithTwoSetupTransitions_IsRejected()
    {
        LotSizingProblemDescriptor descriptor =
            Descriptor(
                SmallBucketProductionMode.Continuous,
                maximumProducedItems: 2,
                maximumSetupTransitions: 2);

        LotSizingProblemClassMatchResult result =
            new SmallBucketSchedulingProblemClassAnalyzer()
                .Assess(
                    descriptor,
                    LotSizingProblemClassCatalog.Plsp);

        Assert.Equal(
            LotSizingProblemClassMatchKind.NotApplicable,
            result.Kind);
    }

    [Fact]
    public void SequenceDependentChangeover_IsAnExtensionNotCoreIdentity()
    {
        LotSizingProblemFeatures features =
            Features(
                SmallBucketProductionMode.Continuous,
                maximumProducedItems: 1,
                maximumSetupTransitions: 0,
                hasSetupLimit: false);

        features.HasSequenceDependentChangeoverCosts =
            true;

        LotSizingProblemClassMatchResult result =
            Assert.Single(
                new LotSizingProblemClassDetector()
                    .Detect(
                        LotSizingProblemDescriptor
                            .FromLegacyFeatures(features)));

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .ContinuousSetupLotSizing,
            result.Definition.Id);

        Assert.Equal(
            LotSizingProblemClassMatchKind.CompatibleExtension,
            result.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind
                .SequenceDependentChangeoverCosts,
            result.Extensions);
    }

    private static LotSizingProblemClassMatchResult AssertSingle(
        SmallBucketProductionMode productionMode,
        int maximumProducedItems,
        int maximumSetupTransitions,
        bool hasSetupLimit)
    {
        return Assert.Single(
            new LotSizingProblemClassDetector()
                .Detect(
                    Descriptor(
                        productionMode,
                        maximumProducedItems,
                        maximumSetupTransitions,
                        hasSetupLimit:
                            hasSetupLimit)));
    }

    private static LotSizingProblemDescriptor Descriptor(
        SmallBucketProductionMode productionMode,
        int maximumProducedItems,
        int maximumSetupTransitions,
        bool hasProducedItemLimit = true,
        bool hasSetupLimit = true) =>
            LotSizingProblemDescriptor
                .FromLegacyFeatures(
                    Features(
                        productionMode,
                        maximumProducedItems,
                        maximumSetupTransitions,
                        hasProducedItemLimit,
                        hasSetupLimit));

    private static LotSizingProblemFeatures Features(
        SmallBucketProductionMode productionMode,
        int maximumProducedItems,
        int maximumSetupTransitions,
        bool hasProducedItemLimit = true,
        bool hasSetupLimit = true) =>
            new()
            {
                ItemCount = 4,
                WorkCenterCount = 1,
                PlanningHorizon = 8,
                ProductStructureRelationshipCount = 0,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true,
                HasProductionCapacityConstraints = true,
                HasSharedProductionCapacity = true,
                HasIntegratedScheduling = true,
                SchedulingBucketMode =
                    SchedulingBucketMode.SmallBucket,
                SmallBucketProductionMode =
                    productionMode,
                SchedulingResourceCount = 1,
                HasMaximumProducedItemCountConstraint =
                    hasProducedItemLimit,
                MaximumProducedItemCountPerBucket =
                    maximumProducedItems,
                HasMaximumSetupCountConstraints =
                    hasSetupLimit,
                MaximumSetupTransitionsPerBucket =
                    maximumSetupTransitions
            };
}
