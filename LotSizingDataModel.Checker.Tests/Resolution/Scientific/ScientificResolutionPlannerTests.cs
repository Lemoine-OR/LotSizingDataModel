using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;
using LotSizingDataModel.Solver.Resolution.Scientific;

namespace LotSizingDataModel.Checker.Tests.Resolution.Scientific;

public sealed class ScientificResolutionPlannerTests
{
    [Fact]
    public void BasicUls_ProducesReadyMilpPlan()
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(
                        Features()));

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificFormulationSelectionResult formulation =
            new ScientificFormulationSelectionService()
                .Select(
                    classification,
                    registry);

        ScientificResolutionPlan plan =
            new ScientificResolutionPlanner()
                .Create(
                    classification,
                    formulation,
                    SolverKind.Automatic);

        Assert.True(plan.IsReady);

        Assert.Equal(
            "MILP-GENERAL",
            plan.SelectedMethodId);

        Assert.Null(
            plan.SelectedBackend);

        Assert.Equal(
            4,
            plan.BackendCandidates.Count);

        Assert.Contains(
            plan.MethodCandidates,
            candidate =>
                candidate.Method.MethodId == "DP-SI-ULS" &&
                candidate.Compatibility ==
                    ScientificSolutionMethodCompatibilityKind
                        .CatalogOnlyRelevant);
    }

    [Fact]
    public void ExplicitCplex_BackendIsScientificallyPinnedInPlan()
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(
                        Features()));

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificFormulationSelectionResult formulation =
            new ScientificFormulationSelectionService()
                .Select(
                    classification,
                    registry);

        ScientificResolutionPlan plan =
            new ScientificResolutionPlanner()
                .Create(
                    classification,
                    formulation,
                    SolverKind.Cplex);

        Assert.True(plan.IsReady);

        Assert.NotNull(
            plan.SelectedBackend);

        Assert.Equal(
            SolverKind.Cplex,
            plan.SelectedBackend!.SolverKind);
    }

    private static LotSizingProblemFeatures Features() =>
        new()
        {
            ItemCount = 1,
            PlanningHorizon = 6,
            ProductStructureRelationshipCount = 0,
            ProductStructureType =
                ProductStructureType.IndependentItems,
            HasDemand = true,
            HasDeterministicDemand = true,
            HasProduction = true,
            HasSetupCosts = true
        };
}
