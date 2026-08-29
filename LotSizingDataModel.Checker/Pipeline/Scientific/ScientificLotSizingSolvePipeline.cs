using LotSizingDataModel.Checker.Facade;
using LotSizingDataModel.Checker.Scientific;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;
using LotSizingDataModel.Solver.Mapping.Scientific;
using LotSizingDataModel.Solver.Resolution.Scientific;

namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// End-to-end scientific solve orchestration.
/// </summary>
/// <remarks>
/// The pipeline never reimplements the technical solver workflow. It delegates
/// model build, solver selection/execution and solution mapping to the existing
/// <see cref="ILotSizingSolverService"/> after scientific preflight has pinned
/// one verified formulation.
///
/// Numerical solution verification and scientific provenance verification are
/// retained as separate result channels.
/// </remarks>
public sealed class ScientificLotSizingSolvePipeline :
    IScientificLotSizingSolvePipeline
{
    private readonly ILotSizingSolverService _solverService;

    private readonly MathematicalModelFormulationRegistry
        _formulationRegistry;

    private readonly ScientificClassificationEngine
        _classificationEngine;

    private readonly ScientificFormulationSelectionService
        _formulationSelectionService;

    private readonly ScientificResolutionPlanner
        _resolutionPlanner;

    private readonly LotSizingSolutionVerificationService
        _solutionVerificationService;

    private readonly SolutionScientificProvenanceChecker
        _provenanceChecker;

    public ScientificLotSizingSolvePipeline(
        ILotSizingSolverService solverService,
        MathematicalModelFormulationRegistry formulationRegistry)
        : this(
            solverService,
            formulationRegistry,
            new ScientificClassificationEngine(),
            new ScientificFormulationSelectionService(),
            new ScientificResolutionPlanner(),
            new LotSizingSolutionVerificationService(),
            new SolutionScientificProvenanceChecker())
    {
    }

    public ScientificLotSizingSolvePipeline(
        ILotSizingSolverService solverService,
        MathematicalModelFormulationRegistry formulationRegistry,
        ScientificClassificationEngine classificationEngine,
        ScientificFormulationSelectionService formulationSelectionService,
        ScientificResolutionPlanner resolutionPlanner,
        LotSizingSolutionVerificationService solutionVerificationService,
        SolutionScientificProvenanceChecker provenanceChecker)
    {
        _solverService =
            solverService ??
            throw new ArgumentNullException(nameof(solverService));

        _formulationRegistry =
            formulationRegistry ??
            throw new ArgumentNullException(nameof(formulationRegistry));

        _classificationEngine =
            classificationEngine ??
            throw new ArgumentNullException(nameof(classificationEngine));

        _formulationSelectionService =
            formulationSelectionService ??
            throw new ArgumentNullException(
                nameof(formulationSelectionService));

        _resolutionPlanner =
            resolutionPlanner ??
            throw new ArgumentNullException(nameof(resolutionPlanner));

        _solutionVerificationService =
            solutionVerificationService ??
            throw new ArgumentNullException(
                nameof(solutionVerificationService));

        _provenanceChecker =
            provenanceChecker ??
            throw new ArgumentNullException(nameof(provenanceChecker));
    }

    public async ValueTask<ScientificSolvePipelineResult> SolveAsync(
        ScientificSolvePipelineRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        SolverRequest sourceRequest =
            request.SolverRequest;

        sourceRequest.EnsureValid();

        ScientificClassificationResult classification =
            _classificationEngine.Analyze(
                sourceRequest.Instance!,
                request.ClassificationRequest);

        bool callerRequestedSpecificFormulation =
            !string.IsNullOrWhiteSpace(
                sourceRequest.FormulationName);

        ScientificFormulationSelectionResult selection =
            _formulationSelectionService.Select(
                classification,
                _formulationRegistry,
                requestedFormulationId:
                    sourceRequest.FormulationName ?? string.Empty,
                allowFallback:
                    !callerRequestedSpecificFormulation);

        ScientificResolutionPlan resolutionPlan =
            _resolutionPlanner.Create(
                classification,
                selection,
                sourceRequest.PreferredSolver);

        var diagnostics =
            new List<ScientificSolvePipelineDiagnostic>();

        if (
            classification.IsBlocked ||
            !selection.IsSuccessful ||
            selection.Formulation is null ||
            !resolutionPlan.IsReady ||
            resolutionPlan.SelectedMethod is null)
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-001",
                    ScientificSolvePipelineDiagnosticSeverity.Error,
                    "scientificPreflight",
                    classification.IsBlocked
                        ? "Scientific classification is blocked."
                        : !selection.IsSuccessful
                            ? "No scientifically selectable mathematical " +
                              "formulation is available."
                            : "No executable scientific resolution plan is " +
                              "available."));

            return Result(
                ScientificSolvePipelineStatus.PreflightRejected,
                classification,
                selection,
                resolutionPlan,
                solverRun: null,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        if (
            !selection.Formulation.CanBuild(
                sourceRequest.Instance!))
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-002",
                    ScientificSolvePipelineDiagnosticSeverity.Error,
                    "scientificPreflight.formulation",
                    $"Scientifically selected formulation " +
                    $"'{selection.Formulation.FormulationId}' failed its " +
                    "technical CanBuild(instance) contract."));

            return Result(
                ScientificSolvePipelineStatus.PreflightRejected,
                classification,
                selection,
                resolutionPlan,
                solverRun: null,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        string selectedFormulationId =
            selection.Formulation.FormulationId;

        SolverRequest delegatedRequest =
            ScientificSolverRequestFactory.CreateDelegated(
                sourceRequest,
                selectedFormulationId);

        // Critical invariant: scientific selection pins the technical request.
        if (
            !delegatedRequest.FormulationName.Equals(
                selectedFormulationId,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Internal scientific solver-request pinning failed.");
        }

        SolverRunResult solverRun =
            await _solverService.SolveAsync(
                delegatedRequest,
                cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (!solverRun.HasSolution)
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-010",
                    solverRun.IsSuccessful
                        ? ScientificSolvePipelineDiagnosticSeverity.Warning
                        : ScientificSolvePipelineDiagnosticSeverity.Error,
                    "solverRun",
                    $"Solver run completed with termination reason " +
                    $"'{solverRun.TerminationReason}' and produced no " +
                    "normalized LotSizingSolution."));

            return Result(
                ScientificSolvePipelineStatus.CompletedWithoutSolution,
                classification,
                selection,
                resolutionPlan,
                solverRun,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        if (
            !solverRun.FormulationName.Equals(
                selectedFormulationId,
                StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-011",
                    ScientificSolvePipelineDiagnosticSeverity.Error,
                    "solverRun.formulationName",
                    $"Scientific preflight selected formulation " +
                    $"'{selectedFormulationId}', but the solver run reports " +
                    $"'{solverRun.FormulationName}'."));

            return Result(
                ScientificSolvePipelineStatus.FormulationDrift,
                classification,
                selection,
                resolutionPlan,
                solverRun,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        if (
            resolutionPlan.SelectedMethod!.Method.RequiresMilpBackend &&
            (
                ScientificSolverBackendCatalog.Find(
                    solverRun.SolverKind) is not
                    ScientificSolverBackendDefinition actualBackend ||
                !actualBackend.Supports(
                    resolutionPlan.SelectedMethod.Method)
            ))
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-012",
                    ScientificSolvePipelineDiagnosticSeverity.Error,
                    "solverRun.solverKind",
                    $"Solver backend '{solverRun.SolverKind}' is not " +
                    $"compatible with scientific solution method " +
                    $"'{resolutionPlan.SelectedMethod.Method.MethodId}'."));

            return Result(
                ScientificSolvePipelineStatus.BackendDrift,
                classification,
                selection,
                resolutionPlan,
                solverRun,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        if (
            sourceRequest.PreferredSolver !=
                LotSizingDataModel.Solver.Common.SolverKind.Automatic &&
            solverRun.SolverKind !=
                sourceRequest.PreferredSolver)
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-013",
                    ScientificSolvePipelineDiagnosticSeverity.Error,
                    "solverRun.solverKind",
                    $"Caller requested solver backend " +
                    $"'{sourceRequest.PreferredSolver}', but the solver run " +
                    $"reports '{solverRun.SolverKind}'."));

            return Result(
                ScientificSolvePipelineStatus.BackendDrift,
                classification,
                selection,
                resolutionPlan,
                solverRun,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        LotSizingSolution solution =
            solverRun.Solution!;

        SolutionScientificProvenance provenance;

        try
        {
            provenance =
                ScientificSolutionProvenanceMapper.Apply(
                    solution,
                    selection,
                    resolutionPlan.SelectedMethod!.Method,
                    solverRun.SolverKind);
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-020",
                    ScientificSolvePipelineDiagnosticSeverity.Error,
                    "solution.provenance",
                    "Scientific provenance capture failed: " +
                    exception.Message));

            return Result(
                ScientificSolvePipelineStatus.ProvenanceCaptureFailed,
                classification,
                selection,
                resolutionPlan,
                solverRun,
                provenance: null,
                numericalVerification: null,
                provenanceVerification: null,
                diagnostics);
        }

        LotSizingSolutionVerificationResult? numericalVerification =
            null;

        if (request.VerifyNumerically)
        {
            numericalVerification =
                await _solutionVerificationService.VerifyAsync(
                    sourceRequest.Instance!,
                    solution,
                    cancellationToken:
                        cancellationToken)
                .ConfigureAwait(false);

            if (!numericalVerification.IsValid)
            {
                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PIPE-030",
                        ScientificSolvePipelineDiagnosticSeverity.Warning,
                        "solution.numericalVerification",
                        "The solver produced a normalized solution and " +
                        "scientific provenance was captured, but independent " +
                        "numerical solution verification did not validate all " +
                        "requested checks."));
            }
        }

        SolutionScientificProvenanceCheckResult? provenanceVerification =
            null;

        if (request.VerifyProvenance)
        {
            provenanceVerification =
                _provenanceChecker.Check(
                    sourceRequest.Instance!,
                    solution);

            if (!provenanceVerification.IsCoherent)
            {
                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PIPE-031",
                        provenanceVerification.HasErrors
                            ? ScientificSolvePipelineDiagnosticSeverity.Error
                            : ScientificSolvePipelineDiagnosticSeverity.Warning,
                        "solution.provenanceVerification",
                        $"Scientific provenance verification returned " +
                        $"'{provenanceVerification.Kind}'."));
            }
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PIPE-100",
                    ScientificSolvePipelineDiagnosticSeverity.Information,
                    "pipeline",
                    "Scientific preflight, solver execution, provenance " +
                    "capture and requested independent verifications " +
                    "completed coherently."));
        }

        return Result(
            ScientificSolvePipelineStatus.Completed,
            classification,
            selection,
            resolutionPlan,
            solverRun,
            provenance,
            numericalVerification,
            provenanceVerification,
            diagnostics);
    }

    public void RequestStop()
    {
        _solverService.RequestStop();
    }

    private static ScientificSolvePipelineDiagnostic Diagnostic(
        string code,
        ScientificSolvePipelineDiagnosticSeverity severity,
        string path,
        string message) =>
            new(
                code,
                severity,
                path,
                message);

    private static ScientificSolvePipelineResult Result(
        ScientificSolvePipelineStatus status,
        ScientificClassificationResult classification,
        ScientificFormulationSelectionResult selection,
        ScientificResolutionPlan resolutionPlan,
        SolverRunResult? solverRun,
        SolutionScientificProvenance? provenance,
        LotSizingSolutionVerificationResult? numericalVerification,
        SolutionScientificProvenanceCheckResult? provenanceVerification,
        IEnumerable<ScientificSolvePipelineDiagnostic> diagnostics) =>
            new(
                status,
                classification,
                selection,
                resolutionPlan,
                solverRun,
                provenance,
                numericalVerification,
                provenanceVerification,
                diagnostics);
}
