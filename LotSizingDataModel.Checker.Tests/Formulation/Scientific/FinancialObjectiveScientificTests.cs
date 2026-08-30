using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class FinancialObjectiveScientificTests
{
    [Fact]
    public void FinancialConstraint_IsVerifiedSupported()
    {
        Assert.True(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionVerifiedSupported(
                    LotSizingProblemClassExtensionKind.FinancialConstraints));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionKnownUnsupported(
                    LotSizingProblemClassExtensionKind.FinancialConstraints));
    }

    [Fact]
    public void StandardProfile_SupportsEconomicObjectiveOnly()
    {
        Assert.True(
            MathematicalFormulationScientificCatalog.Standard
                .SupportsObjectiveKind(
                    OptimizationObjectiveKind.Economic));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .SupportsObjectiveKind(
                    OptimizationObjectiveKind.Financial));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .SupportsObjectiveKind(
                    OptimizationObjectiveKind.Sustainability));
    }

    [Fact]
    public void FinancialSingleObjective_IsScientificallyIncompatible()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.PrimaryObjectiveKind =
            OptimizationObjectiveKind.Financial;

        ScientificFormulationCompatibilityResult result =
            Assess(features);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            result.Kind);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-013");
    }

    [Fact]
    public void MultipleObjectives_RemainKnownUnsupported()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.HasMultipleObjectives = true;
        features.ObjectiveCriterionCount = 2;
        features.ObjectiveAggregationMode =
            ObjectiveAggregationMode.WeightedSum;

        ScientificFormulationCompatibilityResult result =
            Assess(features);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            result.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.MultipleObjectives,
            result.KnownUnsupportedExtensions);
    }

    private static ScientificFormulationCompatibilityResult Assess(
        LotSizingProblemFeatures features) =>
            new ScientificFormulationCompatibilityService()
                .Assess(
                    new ScientificClassificationEngine()
                        .Analyze(
                            LotSizingProblemDescriptor
                                .FromLegacyFeatures(features)),
                    StandardLotSizingFormulation.StandardFormulationId);

    private static LotSizingProblemFeatures BaseFeatures() =>
        new()
        {
            ItemCount = 1,
            PlanningHorizon = 4,
            ProductStructureType =
                ProductStructureType.IndependentItems,
            HasDemand = true,
            HasDeterministicDemand = true,
            HasProduction = true,
            HasSetupCosts = true,
            PrimaryObjectiveKind =
                OptimizationObjectiveKind.Economic,
            ObjectiveCriterionCount = 1,
            ObjectiveAggregationMode =
                ObjectiveAggregationMode.Single
        };
}
