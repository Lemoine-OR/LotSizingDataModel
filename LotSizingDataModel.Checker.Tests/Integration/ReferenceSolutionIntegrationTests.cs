using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Orchestration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Integration;

public sealed class ReferenceSolutionIntegrationTests
{
    [Fact]
    public async Task ReferenceSolution_FullCheck_IsValid()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Full
                });

        Assert.True(
            result.IsValid,
            ResultDiagnostics.Format(result));
        Assert.True(result.StructuralCheckCompleted);
        Assert.True(result.VariableDomainCheckCompleted);
        Assert.True(result.FeasibilityCheckCompleted);
        Assert.True(result.ObjectiveCheckCompleted);
        Assert.True(result.IsStructurallyValid);
        Assert.True(result.AreVariableDomainsValid);
        Assert.True(result.IsFeasible);
        Assert.True(result.IsObjectiveConsistent);
        Assert.NotNull(result.RecomputedObjectiveValue);
        Assert.InRange(
            result.RecomputedObjectiveValue!.Value,
            377.6799999,
            377.6800001);
    }

    [Fact]
    public async Task UncapacitatedReference_DoesNotRequireResourceCapacityContainers()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        Assert.Empty(data.Solution.WorkCenterCapacityDecisions);
        Assert.Empty(data.Solution.WarehouseCapacityDecisions);
        Assert.Empty(data.Solution.TransportResourceCapacityDecisions);

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Structural
                });

        Assert.True(
            result.IsStructurallyValid,
            ResultDiagnostics.Format(result));
        Assert.DoesNotContain(
            result.Issues,
            issue =>
                issue.Message.Contains(
                    "Capacity decision",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task WrongPlanningHorizon_FailsStructuralCheck()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        ReflectionMutation.SetScalarProperty(
            data.Solution,
            "PlanningHorizon",
            data.Solution.PlanningHorizon + 1);

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Structural
                });

        Assert.True(result.StructuralCheckCompleted);
        Assert.False(result.IsStructurallyValid);
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue =>
                issue.Kind == SolutionCheckIssueKind.Structural);
    }

    [Fact]
    public async Task PerturbedProductionQuantity_FailsFeasibility()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        object productionDecision =
            data.Solution.ProductionDecisions
                .Cast<object>()
                .First();

        double original =
            ReflectionMutation.GetFirstNumericSeriesValue(
                productionDecision,
                "Quantities");

        ReflectionMutation.SetFirstNumericSeriesValue(
            productionDecision,
            "Quantities",
            original + 1.0);

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Feasibility
                });

        Assert.True(result.StructuralCheckCompleted);
        Assert.True(result.VariableDomainCheckCompleted);
        Assert.True(result.FeasibilityCheckCompleted);
        Assert.True(result.IsStructurallyValid);
        Assert.True(result.AreVariableDomainsValid);
        Assert.False(result.IsFeasible);
        Assert.False(result.IsValid);
        Assert.True(result.ViolatedConstraintCount > 0);
        Assert.Contains(
            result.Issues,
            issue =>
                issue.Kind == SolutionCheckIssueKind.ConstraintViolation);
    }

    [Fact]
    public async Task WrongReportedObjective_FailsObjectiveConsistency()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        ReflectionMutation.SetScalarProperty(
            data.Solution.Evaluation,
            "ObjectiveValue",
            123456.0);

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Full
                });

        Assert.True(result.IsStructurallyValid);
        Assert.True(result.AreVariableDomainsValid);
        Assert.True(result.IsFeasible);
        Assert.True(result.ObjectiveCheckCompleted);
        Assert.False(result.IsObjectiveConsistent);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ObjectiveDifference);
        Assert.True(result.ObjectiveDifference!.Value > 1.0);
        Assert.Contains(
            result.Issues,
            issue =>
                issue.Kind == SolutionCheckIssueKind.ObjectiveMismatch);
    }
}
