using LotSizingDataModel.Checker.Pipeline.Scientific;
using LotSizingDataModel.Checker.Tests.Infrastructure;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Tests.Pipeline.Scientific;

public sealed class ScientificLotSizingSolvePipelineTests
{
    [Fact]
    public async Task AutomaticPipeline_PinsScientificFormulationWithoutMutatingSourceRequest()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var sourceRequest =
            new SolverRequest(data.Instance)
            {
                PreferredSolver =
                    SolverKind.Automatic,
                FormulationName =
                    string.Empty,
                RunName =
                    "scientific-pipeline-test"
            };

        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                request =>
                    ScientificSolvePipelineFakeSolverService.Success(
                        request,
                        data.Solution));

        var registry =
            CreateStandardRegistry();

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                registry);

        ScientificSolvePipelineResult result =
            await pipeline.SolveAsync(
                new ScientificSolvePipelineRequest(
                    sourceRequest,
                    verifyNumerically: false,
                    verifyProvenance: false));

        Assert.Equal(
            ScientificSolvePipelineStatus.Completed,
            result.Status);

        Assert.Equal(
            1,
            fakeSolver.SolveCallCount);

        Assert.Equal(
            string.Empty,
            sourceRequest.FormulationName);

        Assert.NotNull(
            fakeSolver.LastRequest);

        Assert.Equal(
            StandardLotSizingFormulation.StandardFormulationId,
            fakeSolver.LastRequest!.FormulationName);

        Assert.NotSame(
            sourceRequest,
            fakeSolver.LastRequest);

        Assert.Equal(
            StandardLotSizingFormulation.StandardFormulationId,
            result.FormulationSelection.Formulation!.FormulationId);

        Assert.True(
            result.ResolutionPlan.IsReady);

        Assert.Equal(
            "MILP-GENERAL",
            result.ResolutionPlan.SelectedMethodId);
    }

    [Fact]
    public async Task UnknownExplicitFormulation_IsRejectedBeforeTechnicalSolver()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var request =
            new SolverRequest(data.Instance)
            {
                FormulationName =
                    "unknown-formulation"
            };

        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                delegated =>
                    throw new InvalidOperationException(
                        "Technical solver must not be called."));

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                CreateStandardRegistry());

        ScientificSolvePipelineResult result =
            await pipeline.SolveAsync(
                new ScientificSolvePipelineRequest(
                    request));

        Assert.Equal(
            ScientificSolvePipelineStatus.PreflightRejected,
            result.Status);

        Assert.Equal(
            0,
            fakeSolver.SolveCallCount);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-PIPE-001");
    }

    [Fact]
    public async Task TechnicalRunWithoutSolution_DoesNotCaptureProvenance()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                ScientificSolvePipelineFakeSolverService.NoSolution);

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                CreateStandardRegistry());

        ScientificSolvePipelineResult result =
            await pipeline.SolveAsync(
                new ScientificSolvePipelineRequest(
                    new SolverRequest(data.Instance)));

        Assert.Equal(
            ScientificSolvePipelineStatus.CompletedWithoutSolution,
            result.Status);

        Assert.Null(
            result.CapturedProvenance);

        Assert.Null(
            result.ProvenanceVerification);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-PIPE-010");
    }

    [Fact]
    public async Task ActualFormulationDrift_IsBlockingAndPreventsProvenanceCapture()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                request =>
                    ScientificSolvePipelineFakeSolverService.Success(
                        request,
                        data.Solution,
                        formulationName:
                            "different-formulation"));

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                CreateStandardRegistry());

        ScientificSolvePipelineResult result =
            await pipeline.SolveAsync(
                new ScientificSolvePipelineRequest(
                    new SolverRequest(data.Instance)));

        Assert.Equal(
            ScientificSolvePipelineStatus.FormulationDrift,
            result.Status);

        Assert.Null(
            result.CapturedProvenance);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-PIPE-011");
    }

    [Fact]
    public async Task FullPipeline_CapturesAndIndependentlyVerifiesSolutionAndProvenance()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        // The fixture is known to pass the full independent checker.
        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                request =>
                    ScientificSolvePipelineFakeSolverService.Success(
                        request,
                        data.Solution));

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                CreateStandardRegistry());

        ScientificSolvePipelineResult result =
            await pipeline.SolveAsync(
                new ScientificSolvePipelineRequest(
                    new SolverRequest(data.Instance),
                    verifyNumerically:
                        true,
                    verifyProvenance:
                        true));

        Assert.Equal(
            ScientificSolvePipelineStatus.Completed,
            result.Status);

        Assert.NotNull(
            result.CapturedProvenance);

        SolutionScientificProvenanceReadResult provenanceRead =
            SolutionScientificProvenanceCodec
                .Read(data.Solution.GenerationMetadata);

        Assert.Equal(
            SolutionScientificProvenanceReadKind.Valid,
            provenanceRead.Kind);

        Assert.Equal(
            SolutionScientificProvenance.CurrentSchemaVersion,
            provenanceRead.Provenance!.SchemaVersion);

        Assert.Equal(
            "MILP-GENERAL",
            provenanceRead.Provenance.SolutionMethodId);

        Assert.Equal(
            SolverKind.Cplex.ToString(),
            provenanceRead.Provenance.SolverBackendKind);

        Assert.NotNull(
            result.NumericalVerification);

        Assert.True(
            result.NumericalVerification!.IsValid);

        Assert.NotNull(
            result.ProvenanceVerification);

        Assert.True(
            result.ProvenanceVerification!.IsCoherent);

        Assert.True(
            result.IsEndToEndCoherent);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-PIPE-100");
    }

    [Fact]
    public void RequestStop_IsDelegatedToTechnicalSolver()
    {
        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                request =>
                    ScientificSolvePipelineFakeSolverService.NoSolution(
                        request));

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                CreateStandardRegistry());

        pipeline.RequestStop();

        Assert.True(
            fakeSolver.StopRequested);
    }


    [Fact]
    public async Task ExplicitBackendDrift_IsBlocking()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var sourceRequest =
            new SolverRequest(data.Instance)
            {
                PreferredSolver =
                    SolverKind.Gurobi
            };

        var fakeSolver =
            new ScientificSolvePipelineFakeSolverService(
                request =>
                    ScientificSolvePipelineFakeSolverService.Success(
                        request,
                        data.Solution));

        var pipeline =
            new ScientificLotSizingSolvePipeline(
                fakeSolver,
                CreateStandardRegistry());

        ScientificSolvePipelineResult result =
            await pipeline.SolveAsync(
                new ScientificSolvePipelineRequest(
                    sourceRequest,
                    verifyNumerically: false,
                    verifyProvenance: false));

        Assert.Equal(
            ScientificSolvePipelineStatus.BackendDrift,
            result.Status);

        Assert.Null(
            result.CapturedProvenance);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-PIPE-013");
    }

    private static MathematicalModelFormulationRegistry
        CreateStandardRegistry()
    {
        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        return registry;
    }
}
