using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Facade;
using LotSizingDataModel.Checker.Orchestration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Checker.Tests.Infrastructure;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Integration;

/// <summary>
/// Covers integration points used by solver pipelines that already own a
/// mathematical model and a solver-reported objective value.
/// </summary>
public sealed class SolverPipelineVerificationIntegrationTests
{
    [Fact]
    public async Task PrebuiltModel_FullCheck_IsValid()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        MathematicalModel mathematicalModel =
            await BuildModelAsync(data.Instance);

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                mathematicalModel,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Full
                });

        Assert.True(
            result.IsValid,
            ResultDiagnostics.Format(result));
        Assert.True(result.FeasibilityCheckCompleted);
        Assert.True(result.ObjectiveCheckCompleted);
        Assert.InRange(
            result.RecomputedObjectiveValue ?? double.NaN,
            377.6799999,
            377.6800001);
    }

    [Fact]
    public async Task KnownResultVerification_UsesSolverReportedObjective()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        double? solutionEvaluationObjective =
            data.Solution.Evaluation.ObjectiveValue;

        Assert.NotNull(solutionEvaluationObjective);
        Assert.InRange(
            solutionEvaluationObjective!.Value,
            377.6799999,
            377.6800001);

        data.KnownResult.ReportedObjectiveValue =
            123456.0;

        MathematicalModel mathematicalModel =
            await BuildModelAsync(data.Instance);

        var verificationService =
            new LotSizingSolutionVerificationService();

        LotSizingSolutionVerificationResult verification =
            await verificationService.VerifyKnownResultAsync(
                data.Instance,
                data.KnownResult,
                mathematicalModel,
                new SolutionVerificationOptions
                {
                    CheckOptions =
                        new SolutionCheckOptions
                        {
                            Level = SolutionCheckLevel.Full
                        },
                    ApplyToSolutionEvaluation =
                        false,
                    UpdateKnownResultFeasibility =
                        false,
                    PromoteFullyVerifiedKnownResult =
                        false
                });

        Assert.True(verification.CheckResult.IsFeasible);
        Assert.True(verification.CheckResult.ObjectiveCheckCompleted);
        Assert.False(verification.CheckResult.IsObjectiveConsistent);
        Assert.False(verification.IsValid);
        Assert.Equal(
            123456.0,
            verification.CheckResult.ReportedObjectiveValue);
        Assert.InRange(
            verification.CheckResult.RecomputedObjectiveValue ?? double.NaN,
            377.6799999,
            377.6800001);
        Assert.Contains(
            verification.CheckResult.Issues,
            issue =>
                issue.Kind == SolutionCheckIssueKind.ObjectiveMismatch);
    }

    private static async Task<MathematicalModel> BuildModelAsync(
        LotSizingDataModel.Instance.LotSizingInstance instance)
    {
        MathematicalModelFormulationRegistry formulationRegistry =
            StandardLotSizingFormulationRegistryFactory.Create(
                new StandardLotSizingFormulationOptions());

        var buildOptions =
            new MathematicalModelBuildOptions
            {
                RequestedFormulationId =
                    StandardLotSizingFormulation.StandardFormulationId,
                AllowFallback =
                    false,
                ValidateGeneratedModel =
                    true,
                CloneGeneratedModel =
                    false
            };

        var buildService =
            new MathematicalModelBuildService();

        MathematicalModelBuildResult buildResult =
            await buildService.BuildAsync(
                instance,
                formulationRegistry,
                buildOptions,
                CancellationToken.None);

        Assert.True(
            buildResult.IsSuccessful,
            buildResult.FailureMessage ??
            string.Join(" | ", buildResult.Diagnostics));
        Assert.NotNull(buildResult.Model);

        return buildResult.Model!;
    }
}
