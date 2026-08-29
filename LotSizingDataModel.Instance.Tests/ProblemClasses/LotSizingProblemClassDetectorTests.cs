using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class LotSizingProblemClassDetectorTests
{
    [Fact]
    public void Detector_ReturnsOneCanonicalClassForBasicUmlsp()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 6,
                    PlanningHorizon = 12,
                    ProductStructureRelationshipCount = 7,
                    ProductStructureType =
                        ProductStructureType.General,
                    HasDemand = true,
                    HasDeterministicDemand = true,
                    HasProduction = true,
                    HasSetupCosts = true
                });

        IReadOnlyList<LotSizingProblemClassMatchResult> matches =
            new LotSizingProblemClassDetector()
                .Detect(descriptor);

        Assert.Single(matches);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .UncapacitatedMultiLevelLotSizing,
            matches[0].Definition.Id);
    }

    [Fact]
    public void Detector_PreservesCompatibleExtensions()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 1,
                    PlanningHorizon = 12,
                    ProductStructureRelationshipCount = 0,
                    ProductStructureType =
                        ProductStructureType.IndependentItems,
                    HasDemand = true,
                    HasDeterministicDemand = true,
                    HasProduction = true,
                    HasSetupCosts = true,
                    HasSafetyStockRequirements = true,
                    HasFinancialConstraints = true
                });

        LotSizingProblemClassMatchResult? result =
            new LotSizingProblemClassDetector()
                .DetectSingle(descriptor);

        Assert.NotNull(result);

        Assert.Equal(
            LotSizingProblemClassMatchKind.CompatibleExtension,
            result!.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.SafetyStock,
            result.Extensions);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.FinancialConstraints,
            result.Extensions);
    }

    [Fact]
    public void SchedulingClassAssessment_IsExplicitlyNotRepresentable()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 2,
                    PlanningHorizon = 6,
                    HasDemand = true,
                    HasDeterministicDemand = true,
                    HasProduction = true,
                    HasProductionCapacityConstraints = true,
                    HasSharedProductionCapacity = true,
                    HasSetupCosts = true
                });

        LotSizingProblemClassMatchResult result =
            new LotSizingProblemClassAnalyzer()
                .Assess(
                    descriptor,
                    LotSizingProblemClassCatalog.Dlsp);

        Assert.Equal(
            LotSizingProblemClassMatchKind.NotRepresentable,
            result.Kind);

        Assert.Contains(
            "IntegratedScheduling",
            result.FailedRequirements);
    }
}
