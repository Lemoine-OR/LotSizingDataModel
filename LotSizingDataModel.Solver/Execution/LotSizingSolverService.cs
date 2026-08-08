using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Discovery;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Evaluation;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Orchestrates the complete solution of a lot-sizing instance,
/// from formulation selection to normalized solution mapping.
/// </summary>
public sealed class LotSizingSolverService :
    ILotSizingSolverService
{
    private readonly IMathematicalModelBuildService
        _modelBuildService;

    private readonly IMathematicalModelSolverExecutionService
        _solverExecutionService;

    private readonly IMathematicalSolutionMappingService
        _solutionMappingService;

    private readonly MathematicalModelFormulationRegistry
        _formulationRegistry;

    private readonly SolverAdapterRegistry
        _solverAdapterRegistry;

    private readonly IReadOnlyList<SolverAvailabilityInfo>
        _availabilityInformation;

    private readonly SolverSelectionOptions
        _solverSelectionOptions;

    private readonly MathematicalSolutionMappingOptions
        _mappingOptions;

    /// <summary>
    /// Initializes the high-level lot-sizing solver service
    /// using the standard service implementations.
    /// </summary>
    /// <param name="formulationRegistry">
    /// Registry containing the mathematical formulations
    /// available to the application.
    /// </param>
    /// <param name="solverAdapterRegistry">
    /// Registry containing the loaded solver adapters.
    /// </param>
    /// <param name="availabilityInformation">
    /// Availability information for the detected solvers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the required dependencies is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingSolverService(
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

    /// <summary>
    /// Initializes the high-level lot-sizing solver service with
    /// explicitly supplied service implementations and options.
    /// </summary>
    /// <param name="modelBuildService">
    /// Mathematical-model construction service.
    /// </param>
    /// <param name="solverExecutionService">
    /// Mathematical-model solver execution service.
    /// </param>
    /// <param name="solutionMappingService">
    /// Mathematical-solution mapping service.
    /// </param>
    /// <param name="formulationRegistry">
    /// Registry containing available mathematical formulations.
    /// </param>
    /// <param name="solverAdapterRegistry">
    /// Registry containing loaded solver adapters.
    /// </param>
    /// <param name="availabilityInformation">
    /// Availability information associated with detected
    /// solvers.
    /// </param>
    /// <param name="solverSelectionOptions">
    /// Solver-selection options.
    /// </param>
    /// <param name="mappingOptions">
    /// Mathematical-solution mapping options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the supplied dependencies is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingSolverService(
        IMathematicalModelBuildService modelBuildService,
        IMathematicalModelSolverExecutionService solverExecutionService,
        IMathematicalSolutionMappingService solutionMappingService,
        MathematicalModelFormulationRegistry formulationRegistry,
        SolverAdapterRegistry solverAdapterRegistry,
        IEnumerable<SolverAvailabilityInfo> availabilityInformation,
        SolverSelectionOptions solverSelectionOptions,
        MathematicalSolutionMappingOptions mappingOptions)
    {
        ArgumentNullException.ThrowIfNull(
            modelBuildService);

        ArgumentNullException.ThrowIfNull(
            solverExecutionService);

        ArgumentNullException.ThrowIfNull(
            solutionMappingService);

        ArgumentNullException.ThrowIfNull(
            formulationRegistry);

        ArgumentNullException.ThrowIfNull(
            solverAdapterRegistry);

        ArgumentNullException.ThrowIfNull(
            availabilityInformation);

        ArgumentNullException.ThrowIfNull(
            solverSelectionOptions);

        ArgumentNullException.ThrowIfNull(
            mappingOptions);

        _modelBuildService =
            modelBuildService;

        _solverExecutionService =
            solverExecutionService;

        _solutionMappingService =
            solutionMappingService;

        _formulationRegistry =
            formulationRegistry;

        _solverAdapterRegistry =
            solverAdapterRegistry;

        _availabilityInformation =
            availabilityInformation.ToArray();

        _solverSelectionOptions =
            solverSelectionOptions;

        _mappingOptions =
            mappingOptions;
    }

    /// <summary>
    /// Solves a complete lot-sizing instance.
    /// </summary>
    /// <param name="request">
    /// Normalized solver request.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the complete solution workflow.
    /// </param>
    /// <returns>
    /// Normalized solver-run result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is
    /// <see langword="null"/>.
    /// </exception>
    public async ValueTask<SolverRunResult> SolveAsync(
        SolverRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        request.EnsureValid();

        DateTime startedAtUtc =
            DateTime.UtcNow;

        var stopwatch =
            Stopwatch.StartNew();

        MathematicalModelBuildResult? buildResult =
            null;

        MathematicalModelSolveResult? solveResult =
            null;

        MathematicalSolutionMappingResult? mappingResult =
            null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buildOptions =
                new MathematicalModelBuildOptions
                {
                    RequestedFormulationId =
                        request.FormulationName ?? string.Empty,

                    AllowFallback =
                        string.IsNullOrWhiteSpace(
                            request.FormulationName),

                    ValidateGeneratedModel =
                        true,

                    CloneGeneratedModel =
                        false
                };

            buildResult =
                await _modelBuildService.BuildAsync(
                    request.Instance!,
                    _formulationRegistry,
                    buildOptions,
                    cancellationToken);

            if (!buildResult.IsSuccessful ||
                buildResult.Model is null)
            {
                stopwatch.Stop();

                return CreateWorkflowFailure(
                    request,
                    startedAtUtc,
                    stopwatch.Elapsed,
                    SolverTerminationReason.ModelError,
                    buildResult.FailureMessage,
                    buildResult.Diagnostics);
            }

            var mathematicalRequest =
                new MathematicalModelSolveRequest
                {
                    Model =
                        buildResult.Model,

                    RunName =
                        request.RunName ?? string.Empty,

                    FormulationId =
                        buildResult.SelectedFormulationId,

                    Parameters =
                        request.Parameters
                };

            foreach (
                var observer
                in request.ProgressObservers)
            {
                mathematicalRequest.ProgressObservers.Add(
                    observer);
            }

            mathematicalRequest.EnsureValid();

            solveResult =
                await _solverExecutionService.SolveAsync(
                    mathematicalRequest,
                    request.PreferredSolver,
                    _solverAdapterRegistry,
                    _availabilityInformation,
                    _solverSelectionOptions,
                    cancellationToken);

            if (!solveResult.HasFeasibleSolution)
            {
                stopwatch.Stop();

                var failedMapping =
                    MathematicalSolutionMappingResult.Failure(
                        "No feasible mathematical solution was " +
                        "available for normalized solution mapping.",
                        TimeSpan.Zero);

                SolverRunResult result =
                    MathematicalSolverRunResultMapper.Create(
                        solveResult,
                        failedMapping,
                        startedAtUtc,
                        stopwatch.Elapsed);

                AddDiagnostics(
                    result,
                    buildResult.Diagnostics);

                return result;
            }

            mappingResult =
                await _solutionMappingService.MapAsync(
                    request.Instance!,
                    buildResult.Model,
                    solveResult,
                    _mappingOptions,
                    cancellationToken);

            stopwatch.Stop();

            SolverRunResult finalResult =
                MathematicalSolverRunResultMapper.Create(
                    solveResult,
                    mappingResult,
                    startedAtUtc,
                    stopwatch.Elapsed);

            AddDiagnostics(
                finalResult,
                buildResult.Diagnostics);

            if (mappingResult.IsSuccessful &&
                mappingResult.Solution is not null)
            {
                MathematicalObjectiveRecalculationResult
                    objectiveCheck =
                        MathematicalObjectiveRecalculator.Recalculate(
                            buildResult.Model,
                            solveResult,
                            _mappingOptions);

                finalResult.RecomputedObjectiveValue =
                    objectiveCheck.RecomputedObjectiveValue;

                finalResult.ObjectiveDifference =
                    objectiveCheck.AbsoluteDifference;

                finalResult.ObjectiveVerificationStatus =
                    objectiveCheck.Status;

                foreach (
                    string diagnostic
                    in objectiveCheck.Diagnostics)
                {
                    finalResult.AddDiagnostic(
                        diagnostic);
                }

                if (objectiveCheck.RecomputedObjectiveValue.HasValue)
                {
                    mappingResult.Solution.Evaluation.ObjectiveValue =
                        objectiveCheck.RecomputedObjectiveValue.Value;

                    mappingResult.Solution.Evaluation.ObjectiveValueSource =
                        "Recomputed from normalized mathematical " +
                        "decision-variable values";

                    mappingResult.Solution.Evaluation.EvaluatorName =
                        nameof(MathematicalObjectiveRecalculator);

                    mappingResult.Solution.Evaluation.EvaluatorVersion =
                        "1.0";

                    mappingResult.Solution.Evaluation.EvaluatedAtUtc =
                        DateTime.UtcNow;
                }
            }

            if (!mappingResult.IsSuccessful)
            {
                finalResult.AddDiagnostic(
                    "The solver produced a feasible mathematical " +
                    "solution, but mapping to LotSizingSolution " +
                    "failed.");
            }

            return finalResult;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            return CreateWorkflowFailure(
                request,
                startedAtUtc,
                stopwatch.Elapsed,
                SolverTerminationReason.UserInterrupted,
                "The lot-sizing solve workflow was cancelled.",
                buildResult?.Diagnostics);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            SolverRunResult result =
                CreateWorkflowFailure(
                    request,
                    startedAtUtc,
                    stopwatch.Elapsed,
                    SolverTerminationReason.InternalError,
                    exception.Message,
                    buildResult?.Diagnostics);

            result.AddDiagnostic(
                exception.ToString());

            return result;
        }
    }

    /// <summary>
    /// Requests interruption of the currently running native
    /// mathematical solver.
    /// </summary>
    public void RequestStop()
    {
        _solverExecutionService.RequestStop();
    }

    private static SolverRunResult CreateWorkflowFailure(
        SolverRequest request,
        DateTime startedAtUtc,
        TimeSpan elapsed,
        SolverTerminationReason terminationReason,
        string? message,
        IEnumerable<string>? diagnostics)
    {
        var result =
            new SolverRunResult
            {
                RunName =
                    request.RunName ?? string.Empty,

                SolverKind =
                    request.PreferredSolver,

                FormulationName =
                    request.FormulationName ?? string.Empty,

                StartedAtUtc =
                    startedAtUtc,

                CompletedAtUtc =
                    startedAtUtc + elapsed,

                ElapsedSeconds =
                    elapsed.TotalSeconds,

                TerminationReason =
                    terminationReason,

                SolutionCount =
                    0,

                Solution =
                    null
            };

        AddDiagnostics(
            result,
            diagnostics);

        if (!string.IsNullOrWhiteSpace(
                message))
        {
            result.AddDiagnostic(
                message);
        }

        return result;
    }

    private static void AddDiagnostics(
        SolverRunResult target,
        IEnumerable<string>? diagnostics)
    {
        if (diagnostics is null)
        {
            return;
        }

        foreach (
            string diagnostic
            in diagnostics)
        {
            if (!string.IsNullOrWhiteSpace(
                    diagnostic))
            {
                target.AddDiagnostic(
                    diagnostic);
            }
        }
    }
}
