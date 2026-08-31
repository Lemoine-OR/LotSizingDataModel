using System.Diagnostics;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Discovery;
using LotSizingDataModel.Solver.Evaluation;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// High-level solver service executing Single, WeightedSum and exact
/// Lexicographic objective policies.
/// </summary>
/// <remarks>
/// Lexicographic execution requires each completed stage to be proven optimal
/// before its objective level can be frozen for the next stage.
/// </remarks>
public sealed class MultiObjectiveLotSizingSolverService :
    ILotSizingSolverService
{
    private readonly IMathematicalModelBuildService _modelBuildService;
    private readonly IMathematicalModelSolverExecutionService _solverExecutionService;
    private readonly IMathematicalSolutionMappingService _solutionMappingService;
    private readonly MathematicalModelFormulationRegistry _formulationRegistry;
    private readonly SolverAdapterRegistry _solverAdapterRegistry;
    private readonly IReadOnlyList<SolverAvailabilityInfo> _availabilityInformation;
    private readonly SolverSelectionOptions _solverSelectionOptions;
    private readonly MathematicalSolutionMappingOptions _mappingOptions;

    public MultiObjectiveLotSizingSolverService(
        MathematicalModelFormulationRegistry formulationRegistry,
        SolverAdapterRegistry solverAdapterRegistry,
        IEnumerable<SolverAvailabilityInfo> availabilityInformation)
        : this(
            new MathematicalModelBuildService(),
            new MathematicalModelSolverExecutionService(),
            new MathematicalSolutionMappingService(
                MathematicalDecisionMapperRegistryFactory.CreateDefault()),
            formulationRegistry,
            solverAdapterRegistry,
            availabilityInformation,
            new SolverSelectionOptions(),
            new MathematicalSolutionMappingOptions())
    {
    }

    public MultiObjectiveLotSizingSolverService(
        IMathematicalModelBuildService modelBuildService,
        IMathematicalModelSolverExecutionService solverExecutionService,
        IMathematicalSolutionMappingService solutionMappingService,
        MathematicalModelFormulationRegistry formulationRegistry,
        SolverAdapterRegistry solverAdapterRegistry,
        IEnumerable<SolverAvailabilityInfo> availabilityInformation,
        SolverSelectionOptions solverSelectionOptions,
        MathematicalSolutionMappingOptions mappingOptions)
    {
        ArgumentNullException.ThrowIfNull(modelBuildService);
        ArgumentNullException.ThrowIfNull(solverExecutionService);
        ArgumentNullException.ThrowIfNull(solutionMappingService);
        ArgumentNullException.ThrowIfNull(formulationRegistry);
        ArgumentNullException.ThrowIfNull(solverAdapterRegistry);
        ArgumentNullException.ThrowIfNull(availabilityInformation);
        ArgumentNullException.ThrowIfNull(solverSelectionOptions);
        ArgumentNullException.ThrowIfNull(mappingOptions);

        _modelBuildService = modelBuildService;
        _solverExecutionService = solverExecutionService;
        _solutionMappingService = solutionMappingService;
        _formulationRegistry = formulationRegistry;
        _solverAdapterRegistry = solverAdapterRegistry;
        _availabilityInformation = availabilityInformation.ToArray();
        _solverSelectionOptions = solverSelectionOptions;
        _mappingOptions = mappingOptions;
    }

    public async ValueTask<SolverRunResult> SolveAsync(
        SolverRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.EnsureValid();

        DateTime startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var buildOptions =
                new MathematicalModelBuildOptions
                {
                    RequestedFormulationId =
                        request.FormulationName ?? string.Empty,
                    AllowFallback =
                        string.IsNullOrWhiteSpace(
                            request.FormulationName),
                    ValidateGeneratedModel = true,
                    CloneGeneratedModel = false
                };

            MathematicalModelBuildResult buildResult =
                await _modelBuildService.BuildAsync(
                    request.Instance!,
                    _formulationRegistry,
                    buildOptions,
                    cancellationToken);

            if (!buildResult.IsSuccessful ||
                buildResult.Model is null)
            {
                stopwatch.Stop();

                return Failure(
                    request,
                    startedAtUtc,
                    stopwatch.Elapsed,
                    SolverTerminationReason.ModelError,
                    buildResult.FailureMessage,
                    buildResult.Diagnostics);
            }

            FinancialExecutionModelContext financialContext =
                FinancialExecutionModelDecorator.Decorate(
                    request.Instance!,
                    buildResult.Model);

            IReadOnlyList<ExecutableObjectiveCriterion> criteria =
                MultiObjectiveModelPlanner.ResolveCriteria(
                    request.Instance!,
                    financialContext);

            ObjectiveAggregationMode aggregationMode =
                request.Instance!.SupplyChain.ObjectivePolicy?
                    .AggregationMode ??
                ObjectiveAggregationMode.Single;

            MathematicalModel finalModel;
            MathematicalModelSolveResult finalSolve;
            var stageDiagnostics = new List<string>();

            if (aggregationMode ==
                ObjectiveAggregationMode.WeightedSum)
            {
                finalModel =
                    MultiObjectiveModelPlanner.CreateWeightedSumModel(
                        financialContext,
                        criteria);

                finalSolve =
                    await SolveModelAsync(
                        request,
                        buildResult.SelectedFormulationId,
                        finalModel,
                        cancellationToken);

                if (!finalSolve.HasFeasibleSolution)
                {
                    stopwatch.Stop();

                    return Failure(
                        request,
                        startedAtUtc,
                        stopwatch.Elapsed,
                        finalSolve.TerminationReason,
                        "WeightedSum solve did not produce a feasible solution.",
                        finalSolve.Diagnostics);
                }

                stageDiagnostics.Add(
                    $"WeightedSum solved with {criteria.Count} explicit criteria.");
            }
            else if (aggregationMode ==
                     ObjectiveAggregationMode.Lexicographic)
            {
                var preserved =
                    new List<(ExecutableObjectiveCriterion Criterion, double Value)>();

                MathematicalModel? currentModel = null;
                MathematicalModelSolveResult? currentSolve = null;
                int stageIndex = 0;

                foreach (ExecutableObjectiveCriterion criterion in criteria)
                {
                    stageIndex++;

                    currentModel =
                        MultiObjectiveModelPlanner.CreateLexicographicStageModel(
                            financialContext,
                            criterion,
                            preserved);

                    currentSolve =
                        await SolveModelAsync(
                            request,
                            buildResult.SelectedFormulationId,
                            currentModel,
                            cancellationToken);

                    if (!currentSolve.HasFeasibleSolution ||
                        !currentSolve.IsOptimal ||
                        !currentSolve.ObjectiveValue.HasValue)
                    {
                        stopwatch.Stop();

                        return Failure(
                            request,
                            startedAtUtc,
                            stopwatch.Elapsed,
                            currentSolve.TerminationReason,
                            $"Lexicographic stage {stageIndex} ({criterion.Kind}) must be proven optimal before the next stage.",
                            currentSolve.Diagnostics);
                    }

                    double value =
                        currentSolve.ObjectiveValue.Value;

                    preserved.Add(
                        (criterion, value));

                    stageDiagnostics.Add(
                        $"Lexicographic stage {stageIndex}: kind={criterion.Kind}; value={value:R}; tolerance={criterion.AbsoluteTolerance:R}; solver={currentSolve.SolverName} {currentSolve.SolverVersion}.");
                }

                if (currentModel is null ||
                    currentSolve is null)
                {
                    throw new InvalidOperationException(
                        "Lexicographic execution produced no stage.");
                }

                finalModel = currentModel;
                finalSolve = currentSolve;
            }
            else
            {
                ExecutableObjectiveCriterion criterion =
                    criteria.Single();

                finalModel =
                    MultiObjectiveModelPlanner.CreateLexicographicStageModel(
                        financialContext,
                        criterion,
                        Array.Empty<(ExecutableObjectiveCriterion, double)>());

                finalSolve =
                    await SolveModelAsync(
                        request,
                        buildResult.SelectedFormulationId,
                        finalModel,
                        cancellationToken);

                if (!finalSolve.HasFeasibleSolution)
                {
                    stopwatch.Stop();

                    return Failure(
                        request,
                        startedAtUtc,
                        stopwatch.Elapsed,
                        finalSolve.TerminationReason,
                        $"Single objective '{criterion.Kind}' did not produce a feasible solution.",
                        finalSolve.Diagnostics);
                }
            }

            MathematicalSolutionMappingResult mappingResult =
                await _solutionMappingService.MapAsync(
                    request.Instance!,
                    finalModel,
                    finalSolve,
                    _mappingOptions,
                    cancellationToken);

            stopwatch.Stop();

            SolverRunResult result =
                MathematicalSolverRunResultMapper.Create(
                    finalSolve,
                    mappingResult,
                    startedAtUtc,
                    stopwatch.Elapsed);

            foreach (string diagnostic in buildResult.Diagnostics)
            {
                result.AddDiagnostic(diagnostic);
            }

            foreach (string diagnostic in stageDiagnostics)
            {
                result.AddDiagnostic(diagnostic);
            }

            if (mappingResult.IsSuccessful &&
                mappingResult.Solution is not null)
            {
                MathematicalObjectiveRecalculationResult objectiveCheck =
                    MathematicalObjectiveRecalculator.Recalculate(
                        finalModel,
                        finalSolve,
                        _mappingOptions);

                result.RecomputedObjectiveValue =
                    objectiveCheck.RecomputedObjectiveValue;

                result.ObjectiveDifference =
                    objectiveCheck.AbsoluteDifference;

                result.ObjectiveVerificationStatus =
                    objectiveCheck.Status;

                foreach (string diagnostic in objectiveCheck.Diagnostics)
                {
                    result.AddDiagnostic(diagnostic);
                }
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            return Failure(
                request,
                startedAtUtc,
                stopwatch.Elapsed,
                SolverTerminationReason.UserInterrupted,
                "The multiobjective solve workflow was cancelled.",
                null);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            return Failure(
                request,
                startedAtUtc,
                stopwatch.Elapsed,
                SolverTerminationReason.InternalError,
                exception.Message,
                new[] { exception.ToString() });
        }
    }

    public void RequestStop()
    {
        _solverExecutionService.RequestStop();
    }

    private async ValueTask<MathematicalModelSolveResult> SolveModelAsync(
        SolverRequest request,
        string formulationId,
        MathematicalModel model,
        CancellationToken cancellationToken)
    {
        var mathematicalRequest =
            new MathematicalModelSolveRequest
            {
                Model = model,
                RunName = request.RunName ?? string.Empty,
                FormulationId = formulationId,
                Parameters = request.Parameters
            };

        foreach (var observer in request.ProgressObservers)
        {
            mathematicalRequest.ProgressObservers.Add(observer);
        }

        mathematicalRequest.EnsureValid();

        return await _solverExecutionService.SolveAsync(
            mathematicalRequest,
            request.PreferredSolver,
            _solverAdapterRegistry,
            _availabilityInformation,
            _solverSelectionOptions,
            cancellationToken);
    }

    private static SolverRunResult Failure(
        SolverRequest request,
        DateTime startedAtUtc,
        TimeSpan elapsed,
        SolverTerminationReason reason,
        string? message,
        IEnumerable<string>? diagnostics)
    {
        var result =
            new SolverRunResult
            {
                RunName = request.RunName ?? string.Empty,
                SolverKind = request.PreferredSolver,
                FormulationName =
                    request.FormulationName ?? string.Empty,
                StartedAtUtc = startedAtUtc,
                CompletedAtUtc = startedAtUtc + elapsed,
                ElapsedSeconds = elapsed.TotalSeconds,
                TerminationReason = reason,
                SolutionCount = 0,
                Solution = null
            };

        if (!string.IsNullOrWhiteSpace(message))
        {
            result.AddDiagnostic(message);
        }

        if (diagnostics is not null)
        {
            foreach (string diagnostic in diagnostics)
            {
                result.AddDiagnostic(diagnostic);
            }
        }

        return result;
    }
}
