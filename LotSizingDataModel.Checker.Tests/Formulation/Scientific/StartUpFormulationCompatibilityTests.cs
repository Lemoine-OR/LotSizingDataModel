using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class StartUpFormulationCompatibilityTests
{
    [Fact]
    public void StandardFormulation_RejectsDetectedStartUpTimeExtension()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 4,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true,
                HasSetupCosts = true,
                HasStartUpTimes = true
            };

        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(features));

        ScientificFormulationCompatibilityResult compatibility =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            compatibility.Kind);

        Assert.Contains(
            LotSizingDataModel.Instance.ProblemClasses
                .LotSizingProblemClassExtensionKind.StartUpTimes,
            compatibility.KnownUnsupportedExtensions);
    }
}
