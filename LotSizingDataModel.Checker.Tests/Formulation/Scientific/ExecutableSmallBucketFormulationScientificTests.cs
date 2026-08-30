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
        CanonicalLotSizingProblemClassId.DiscreteLotSizingAndScheduling,
        SmallBucketSchedulingFormulation.DlspFormulationId)]
    [InlineData(
        CanonicalLotSizingProblemClassId.ContinuousSetupLotSizing,
        SmallBucketSchedulingFormulation.CslpFormulationId)]
    [InlineData(
        CanonicalLotSizingProblemClassId.ProportionalLotSizingAndScheduling,
        SmallBucketSchedulingFormulation.PlspFormulationId)]
    public void CanonicalSmallBucketClass_SelectsDedicatedCompatibleFormulation(
        CanonicalLotSizingProblemClassId expectedClass,
        string formulationId)
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(
                            Features(expectedClass)));

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
    public void Glsp_RemainsCatalogOnly()
    {
        Assert.Equal(
            LotSizingProblemClassSupportLevel.CatalogOnly,
            LotSizingProblemClassCatalog.Glsp.SupportLevel);
    }

    private static LotSizingProblemFeatures Features(
        CanonicalLotSizingProblemClassId problemClass)
    {
        bool isDlsp =
            problemClass ==
            CanonicalLotSizingProblemClassId
                .DiscreteLotSizingAndScheduling;

        bool isPlsp =
            problemClass ==
            CanonicalLotSizingProblemClassId
                .ProportionalLotSizingAndScheduling;

        return new LotSizingProblemFeatures
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
            SmallBucketProductionMode =
                isDlsp
                    ? SmallBucketProductionMode.AllOrNothing
                    : SmallBucketProductionMode.Continuous,
            SchedulingResourceCount = 1,
            HasMaximumProducedItemCountConstraint = true,
            MaximumProducedItemCountPerBucket =
                isPlsp ? 2 : 1,
            HasMaximumSetupCountConstraints =
                isPlsp,
            MaximumSetupTransitionsPerBucket =
                isPlsp ? 1 : 0
        };
    }
}
