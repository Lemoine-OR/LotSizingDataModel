using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Domain;
using LotSizingDataModel.Checker.Feasibility;
using LotSizingDataModel.Checker.Objective;
using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Checker.Structural;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Orchestration;

/// <summary>
/// Orchestrates the complete, solver-independent validation of a
/// <see cref="LotSizingSolution"/> against a <see cref="LotSizingInstance"/>.
/// </summary>
/// <remarks>
/// <para>
/// The checker executes the validation pipeline in increasing order of
/// dependency: structural consistency, numerical variable domains,
/// mathematical feasibility, and objective consistency.
/// </para>
/// <para>
/// For mathematical checks, the mathematical model is either generated once
/// from the instance by the configured formulation registry or supplied by
/// the caller when it has already been built. No native solver is created or
/// invoked.
/// </para>
/// <para>
/// Structural errors stop the pipeline by default because subsequent
/// projection may no longer have a well-defined candidate value for every
/// mathematical variable. This behavior can be changed through
/// <see cref="SolutionCheckOptions.ContinueAfterStructuralErrors"/>.
/// </para>
/// </remarks>
public sealed class LotSizingSolutionChecker :
    ILotSizingSolutionChecker
{
    private readonly ISolutionStructuralChecker _structuralChecker;
    private readonly ISolutionVariableDomainChecker _variableDomainChecker;
    private readonly IMathematicalFeasibilityChecker _feasibilityChecker;
    private readonly IMathematicalObjectiveChecker _objectiveChecker;
    private readonly IMathematicalModelBuildService _modelBuildService;
    private readonly MathematicalModelFormulationRegistry _formulationRegistry;
    private readonly string _formulationId;

    /// <summary>
    /// Initializes the checker with the standard LotSizingDataModel
    /// formulation and the standard checker components.
    /// </summary>
    public LotSizingSolutionChecker()
    {
        var projector =
            new MathematicalSolutionValueProjector();

        _structuralChecker =
            new SolutionStructuralChecker();

        _variableDomainChecker =
            new SolutionVariableDomainChecker();

        _feasibilityChecker =
            new MathematicalFeasibilityChecker(
                projector);

        _objectiveChecker =
            new MathematicalObjectiveChecker(
                projector);

        _modelBuildService =
            new MathematicalModelBuildService();

        _formulationRegistry =
            LotSizingFormulationRegistryFactory.CreateDefault();

        _formulationId =
            string.Empty;
    }

    /// <summary>
    /// Initializes the checker with explicitly supplied components.
    /// </summary>
    /// <param name="structuralChecker">
    /// Structural solution checker.
    /// </param>
    /// <param name="variableDomainChecker">
    /// Numerical-domain checker.
    /// </param>
    /// <param name="feasibilityChecker">
    /// Mathematical-feasibility checker.
    /// </param>
    /// <param name="objectiveChecker">
    /// Mathematical-objective checker.
    /// </param>
    /// <param name="modelBuildService">
    /// Solver-independent mathematical-model build service.
    /// </param>
    /// <param name="formulationRegistry">
    /// Registry containing the formulation used for independent checking.
    /// </param>
    /// <param name="formulationId">
    /// Identifier of the formulation to use when constructing the model.
    /// </param>
    public LotSizingSolutionChecker(
        ISolutionStructuralChecker structuralChecker,
        ISolutionVariableDomainChecker variableDomainChecker,
        IMathematicalFeasibilityChecker feasibilityChecker,
        IMathematicalObjectiveChecker objectiveChecker,
        IMathematicalModelBuildService modelBuildService,
        MathematicalModelFormulationRegistry formulationRegistry,
        string formulationId)
    {
        _structuralChecker =
            structuralChecker ??
            throw new ArgumentNullException(
                nameof(structuralChecker));

        _variableDomainChecker =
            variableDomainChecker ??
            throw new ArgumentNullException(
                nameof(variableDomainChecker));

        _feasibilityChecker =
            feasibilityChecker ??
            throw new ArgumentNullException(
                nameof(feasibilityChecker));

        _objectiveChecker =
            objectiveChecker ??
            throw new ArgumentNullException(
                nameof(objectiveChecker));

        _modelBuildService =
            modelBuildService ??
            throw new ArgumentNullException(
                nameof(modelBuildService));

        _formulationRegistry =
            formulationRegistry ??
            throw new ArgumentNullException(
                nameof(formulationRegistry));

        if (string.IsNullOrWhiteSpace(
                formulationId))
        {
            throw new ArgumentException(
                "The mathematical formulation identifier cannot be empty.",
                nameof(formulationId));
        }

        _formulationId =
            formulationId;
    }

    /// <inheritdoc/>
    public Task<SolutionCheckResult> CheckAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionCheckOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return CheckCoreAsync(
            instance,
            solution,
            mathematicalModel: null,
            options,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SolutionCheckResult> CheckAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        MathematicalModel mathematicalModel,
        SolutionCheckOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            mathematicalModel);

        return CheckCoreAsync(
            instance,
            solution,
            mathematicalModel,
            options,
            cancellationToken);
    }

    private async Task<SolutionCheckResult> CheckCoreAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        MathematicalModel? mathematicalModel,
        SolutionCheckOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            solution);

        options ??=
            new SolutionCheckOptions();

        options.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        var result =
            new SolutionCheckResult
            {
                Level =
                    options.Level,
                IsStructurallyValid =
                    false,
                AreVariableDomainsValid =
                    false,
                IsFeasible =
                    false,
                IsObjectiveConsistent =
                    false
            };

        try
        {
            SolutionCheckResult structuralResult =
                _structuralChecker.Check(
                    instance,
                    solution,
                    options);

            MergeStructuralResult(
                structuralResult,
                result);

            result.StructuralCheckCompleted =
                true;

            if (options.Level ==
                SolutionCheckLevel.Structural)
            {
                return result;
            }

            if (!result.IsStructurallyValid &&
                !options.ContinueAfterStructuralErrors)
            {
                return result;
            }

            cancellationToken.ThrowIfCancellationRequested();

            SolutionCheckResult domainResult =
                _variableDomainChecker.Check(
                    solution,
                    options);

            MergeVariableDomainResult(
                domainResult,
                result);

            result.VariableDomainCheckCompleted =
                true;

            cancellationToken.ThrowIfCancellationRequested();

            MathematicalModel? effectiveMathematicalModel =
                mathematicalModel;

            if (effectiveMathematicalModel is null)
            {
                effectiveMathematicalModel =
                    await BuildMathematicalModelAsync(
                        instance,
                        result,
                        cancellationToken)
                        .ConfigureAwait(false);
            }

            if (effectiveMathematicalModel is null)
            {
                return result;
            }

            cancellationToken.ThrowIfCancellationRequested();

            SolutionCheckResult feasibilityResult =
                _feasibilityChecker.Check(
                    effectiveMathematicalModel,
                    solution,
                    options);

            MergeFeasibilityResult(
                feasibilityResult,
                result);

            result.FeasibilityCheckCompleted =
                true;

            if (options.Level ==
                SolutionCheckLevel.Feasibility)
            {
                return result;
            }

            if (HasProjectionFailure(
                    feasibilityResult))
            {
                return result;
            }

            cancellationToken.ThrowIfCancellationRequested();

            SolutionCheckResult objectiveResult =
                _objectiveChecker.Check(
                    effectiveMathematicalModel,
                    solution,
                    options);

            MergeObjectiveResult(
                objectiveResult,
                result);

            result.ObjectiveCheckCompleted =
                true;

            return result;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            result.IsFeasible =
                false;

            if (options.Level ==
                SolutionCheckLevel.Full)
            {
                result.IsObjectiveConsistent =
                    false;
            }

            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Error,
                    Kind =
                        SolutionCheckIssueKind.CheckFailure,
                    Message =
                        "Complete lot-sizing solution checking failed: " +
                        exception.Message
                });

            return result;
        }
    }

    private async Task<MathematicalModel?> BuildMathematicalModelAsync(
        LotSizingInstance instance,
        SolutionCheckResult result,
        CancellationToken cancellationToken)
    {
        var buildOptions =
            new MathematicalModelBuildOptions
            {
                RequestedFormulationId =
                    _formulationId,
                AllowFallback =
                    false,
                ValidateGeneratedModel =
                    true,
                CloneGeneratedModel =
                    false
            };

        MathematicalModelBuildResult buildResult =
            await _modelBuildService.BuildAsync(
                instance,
                _formulationRegistry,
                buildOptions,
                cancellationToken)
                .ConfigureAwait(false);

        if (buildResult.IsSuccessful &&
            buildResult.Model is not null)
        {
            return buildResult.Model;
        }

        string failureMessage =
            string.IsNullOrWhiteSpace(
                buildResult.FailureMessage)
                ? "The mathematical model required for solution checking " +
                  "could not be constructed."
                : buildResult.FailureMessage;

        result.AddIssue(
            new SolutionCheckIssue
            {
                Severity =
                    SolutionCheckSeverity.Error,
                Kind =
                    SolutionCheckIssueKind.CheckFailure,
                Message =
                    "Mathematical model construction failed: " +
                    failureMessage
            });

        foreach (
            string diagnostic
            in buildResult.Diagnostics)
        {
            if (string.IsNullOrWhiteSpace(
                    diagnostic))
            {
                continue;
            }

            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Warning,
                    Kind =
                        SolutionCheckIssueKind.CheckFailure,
                    Message =
                        "Mathematical model diagnostic: " +
                        diagnostic
                });
        }

        return null;
    }

    private static void MergeStructuralResult(
        SolutionCheckResult source,
        SolutionCheckResult target)
    {
        target.IsStructurallyValid =
            source.IsStructurallyValid;

        MergeIssues(
            source,
            target);
    }

    private static void MergeVariableDomainResult(
        SolutionCheckResult source,
        SolutionCheckResult target)
    {
        target.AreVariableDomainsValid =
            source.AreVariableDomainsValid;

        MergeIssues(
            source,
            target);
    }

    private static void MergeFeasibilityResult(
        SolutionCheckResult source,
        SolutionCheckResult target)
    {
        target.IsFeasible =
            source.IsFeasible;

        target.MaximumConstraintViolation =
            source.MaximumConstraintViolation;

        target.TotalConstraintViolation =
            source.TotalConstraintViolation;

        target.ViolatedConstraintCount =
            source.ViolatedConstraintCount;

        MergeIssues(
            source,
            target);
    }

    private static void MergeObjectiveResult(
        SolutionCheckResult source,
        SolutionCheckResult target)
    {
        target.IsObjectiveConsistent =
            source.IsObjectiveConsistent;

        target.RecomputedObjectiveValue =
            source.RecomputedObjectiveValue;

        target.ReportedObjectiveValue =
            source.ReportedObjectiveValue;

        target.ObjectiveDifference =
            source.ObjectiveDifference;

        target.ObjectiveRelativeDifference =
            source.ObjectiveRelativeDifference;

        target.ObjectiveComparisonTolerance =
            source.ObjectiveComparisonTolerance;

        MergeIssues(
            source,
            target);
    }

    private static void MergeIssues(
        SolutionCheckResult source,
        SolutionCheckResult target)
    {
        foreach (
            SolutionCheckIssue issue
            in source.Issues)
        {
            target.AddIssue(
                issue);
        }
    }

    private static bool HasProjectionFailure(
        SolutionCheckResult feasibilityResult)
    {
        return
            feasibilityResult.Issues.Any(
                issue =>
                    issue.Severity ==
                        SolutionCheckSeverity.Error &&
                    issue.Kind ==
                        SolutionCheckIssueKind.MissingVariableValue);
    }
}
