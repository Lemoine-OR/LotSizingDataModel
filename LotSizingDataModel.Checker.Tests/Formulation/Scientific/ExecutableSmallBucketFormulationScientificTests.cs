using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class ExecutableSmallBucketFormulationScientificTests
{
    [Theory]
    [InlineData(
        SmallBucketProductionMode.AllOrNothing,
        CanonicalLotSizingProblemClassId.DiscreteLotSizingAndScheduling,
        SmallBucketSchedulingFormulation.DlspFormulationId)]
    [InlineData(
        SmallBucketProductionMode.Continuous,
        CanonicalLotSizingProblemClassId.ContinuousSetupLotSizing,
        SmallBucketSchedulingFormulation.CslpFormulationId)]
    public void CanonicalSmallBucketClass_SelectsDedicatedCompatibleFormulation(
        SmallBucketProductionMode mode,
        CanonicalLotSizingProblemClassId expectedClass,
        string formulationId)
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(
                            Features(mode)));

        Assert.NotNull(
            classification.PrimaryProblemClass);

        Assert.Equal(
            expectedClass,
            classification.PrimaryProblemClass!
                .Definition.Id);

        Assert.Equal(
            LotSizingProblemClassSupportLevel.Executable,
            classification.PrimaryProblemClass!
                .Definition.SupportLevel);

        ScientificFormulationCompatibilityResult dedicated =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    formulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Compatible,
            dedicated.Kind);

        ScientificFormulationCompatibilityResult standard =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            standard.Kind);
    }

    [Fact]
    public void Plsp_RemainsClassifiableNotExecutable()
    {
        Assert.Equal(
            LotSizingProblemClassSupportLevel.Classifiable,
            LotSizingProblemClassCatalog.Plsp.SupportLevel);
    }

    private static LotSizingProblemFeatures Features(
        SmallBucketProductionMode mode) =>
            new()
            {
                ItemCount = 3,
                WorkCenterCount = 1,
                PlanningHorizon = 6,
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
                SmallBucketProductionMode = mode,
                SchedulingResourceCount = 1,
                HasMaximumProducedItemCountConstraint = true,
                MaximumProducedItemCountPerBucket = 1
            };
}
