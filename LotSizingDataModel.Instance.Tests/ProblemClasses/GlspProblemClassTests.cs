using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class GlspProblemClassTests
{
    [Fact]
    public void CanonicalGlsp_IsUniquelyClassified()
    {
        LotSizingProblemClassMatchResult result =
            Assert.Single(
                new LotSizingProblemClassDetector()
                    .Detect(Descriptor()));

        Assert.Equal(
            CanonicalLotSizingProblemClassId.GeneralLotSizingAndScheduling,
            result.Definition.Id);
        Assert.Equal(
            LotSizingProblemClassSupportLevel.Classifiable,
            result.Definition.SupportLevel);
        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.Kind);
    }

    [Fact]
    public void FixedLengthMicroPeriods_AreNotCanonicalGlsp()
    {
        LotSizingProblemClassMatchResult result =
            new MacroMicroSchedulingProblemClassAnalyzer()
                .Assess(
                    Descriptor(MicroPeriodLengthMode.Fixed),
                    LotSizingProblemClassCatalog.Glsp);

        Assert.Equal(
            LotSizingProblemClassMatchKind.NotApplicable,
            result.Kind);
    }

    [Fact]
    public void MissingGrid_IsIncomplete()
    {
        LotSizingProblemClassMatchResult result =
            new MacroMicroSchedulingProblemClassAnalyzer()
                .Assess(
                    Descriptor(hasGrid: false),
                    LotSizingProblemClassCatalog.Glsp);

        Assert.Equal(
            LotSizingProblemClassMatchKind.Incomplete,
            result.Kind);
        Assert.Contains(
            "Scheduling:MicroPeriodGrid",
            result.FailedRequirements);
    }

    [Fact]
    public void SequenceDependentChangeoverCost_IsAnExtension()
    {
        LotSizingProblemClassMatchResult result =
            Assert.Single(
                new LotSizingProblemClassDetector()
                    .Detect(
                        Descriptor(
                            hasSequenceDependentChangeoverCost: true)));

        Assert.Equal(
            LotSizingProblemClassMatchKind.CompatibleExtension,
            result.Kind);
        Assert.Contains(
            LotSizingProblemClassExtensionKind.SequenceDependentChangeoverCosts,
            result.Extensions);
    }

    private static LotSizingProblemDescriptor Descriptor(
        MicroPeriodLengthMode lengthMode =
            MicroPeriodLengthMode.Variable,
        bool hasGrid = true,
        bool hasSequenceDependentChangeoverCost = false) =>
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 4,
                    WorkCenterCount = 1,
                    PlanningHorizon = 5,
                    ProductStructureType =
                        ProductStructureType.IndependentItems,
                    HasDemand = true,
                    HasDeterministicDemand = true,
                    HasProduction = true,
                    HasProductionCapacityConstraints = true,
                    HasSharedProductionCapacity = true,
                    HasIntegratedScheduling = true,
                    SchedulingBucketMode =
                        SchedulingBucketMode.MacroMicro,
                    SchedulingResourceCount = 1,
                    MicroPeriodLengthMode = lengthMode,
                    MicroPeriodAssignmentMode =
                        MicroPeriodAssignmentMode.SingleItem,
                    HasExplicitMicroPeriodGrid = hasGrid,
                    TotalMicroPeriodCount = hasGrid ? 15 : 0,
                    MaximumMicroPeriodCountPerMacroPeriod =
                        hasGrid ? 3 : 0,
                    HasSequenceDependentChangeoverCosts =
                        hasSequenceDependentChangeoverCost
                });
}
