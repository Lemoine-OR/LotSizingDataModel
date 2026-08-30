using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class GlspScientificCompatibilityTests
{
    [Fact]
    public void ClassifiableGlsp_IsNotClaimedByStandardMilp()
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(
                            new LotSizingProblemFeatures
                            {
                                ItemCount = 3,
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
                                    SchedulingBucketMode.MacroMicro,
                                SchedulingResourceCount = 1,
                                MicroPeriodLengthMode =
                                    MicroPeriodLengthMode.Variable,
                                MicroPeriodAssignmentMode =
                                    MicroPeriodAssignmentMode.SingleItem,
                                HasExplicitMicroPeriodGrid = true,
                                TotalMicroPeriodCount = 12,
                                MaximumMicroPeriodCountPerMacroPeriod = 3
                            }));

        Assert.NotNull(classification.PrimaryProblemClass);
        Assert.Equal(
            CanonicalLotSizingProblemClassId.GeneralLotSizingAndScheduling,
            classification.PrimaryProblemClass!.Definition.Id);
        Assert.Equal(
            LotSizingProblemClassSupportLevel.Classifiable,
            classification.PrimaryProblemClass.Definition.SupportLevel);

        ScientificFormulationCompatibilityResult result =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            result.Kind);
        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-010");
    }
}
