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
    public void SchedulingCore_RemainsScientificallyUnsupportedByStandardMilp()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 2,
                PlanningHorizon = 4,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true,
                HasSetupCosts = true,
                HasIntegratedScheduling = true,
                SchedulingBucketMode =
                    SchedulingBucketMode.MacroMicro,
                HasSequenceDependentChangeoverTimes = true,
                HasSequenceDependentChangeoverCosts = true
            };

        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(features));

        ScientificFormulationCompatibilityResult result =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            result.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.IntegratedScheduling,
            result.KnownUnsupportedExtensions);

        Assert.Contains(
            LotSizingProblemClassExtensionKind
                .SequenceDependentChangeoverTimes,
            result.KnownUnsupportedExtensions);
    }
}
