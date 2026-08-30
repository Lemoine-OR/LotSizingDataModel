using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class SchedulingScientificCompatibilityTests
{
    [Fact]
    public void ClassifiableCslp_IsScientificallyIncompatibleWithStandardMilp()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 2,
                WorkCenterCount = 1,
                PlanningHorizon = 4,
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
                    SmallBucketProductionMode.Continuous,
                SchedulingResourceCount = 1,
                HasMaximumProducedItemCountConstraint = true,
                MaximumProducedItemCountPerBucket = 1
            };

        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(features));

        Assert.NotNull(
            classification.PrimaryProblemClass);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .ContinuousSetupLotSizing,
            classification.PrimaryProblemClass!
                .Definition.Id);

        Assert.Equal(
            LotSizingProblemClassSupportLevel.Classifiable,
            classification.PrimaryProblemClass!
                .Definition.SupportLevel);

        ScientificFormulationCompatibilityResult result =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            result.Kind);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .ContinuousSetupLotSizing,
            result.ProblemClass);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-010");

        Assert.Empty(
            result.KnownUnsupportedExtensions);
    }

    [Fact]
    public void IncompleteSchedulingClassification_ProducesUndeterminedCompatibility()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 2,
                WorkCenterCount = 1,
                PlanningHorizon = 4,
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
                SchedulingResourceCount = 1
            };

        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(features));

        Assert.Null(
            classification.PrimaryProblemClass);

        Assert.Contains(
            classification.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-SCI-020");

        ScientificFormulationCompatibilityResult result =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Undetermined,
            result.Kind);

        Assert.Null(
            result.ProblemClass);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-003");

        Assert.Empty(
            result.KnownUnsupportedExtensions);
    }

    [Fact]
    public void StandardProfile_StillMarksSchedulingExtensionsAsUnsupported()
    {
        MathematicalFormulationScientificProfile profile =
            MathematicalFormulationScientificCatalog.Standard;

        Assert.True(
            profile.IsExtensionKnownUnsupported(
                LotSizingProblemClassExtensionKind.IntegratedScheduling));

        Assert.True(
            profile.IsExtensionKnownUnsupported(
                LotSizingProblemClassExtensionKind.SmallBucketScheduling));

        Assert.True(
            profile.IsExtensionKnownUnsupported(
                LotSizingProblemClassExtensionKind
                    .SequenceDependentChangeoverTimes));

        Assert.True(
            profile.IsExtensionKnownUnsupported(
                LotSizingProblemClassExtensionKind
                    .SequenceDependentChangeoverCosts));
    }
}
