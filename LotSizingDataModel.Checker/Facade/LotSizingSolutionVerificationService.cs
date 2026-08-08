using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Integration;
using LotSizingDataModel.Checker.Orchestration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Results;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Facade;

/// <summary>
/// High-level facade for checking a lot-sizing solution and, when requested,
/// recording the independent evaluation on serializable domain objects.
/// </summary>
public sealed class LotSizingSolutionVerificationService
{
    private readonly ILotSizingSolutionChecker _checker;
    private readonly ISolutionCheckResultApplier _applier;

    /// <summary>
    /// Initializes the facade with the standard checker and result applier.
    /// </summary>
    public LotSizingSolutionVerificationService()
        : this(
            new LotSizingSolutionChecker(),
            new SolutionCheckResultApplier())
    {
    }

    /// <summary>
    /// Initializes the facade with explicit dependencies.
    /// </summary>
    /// <param name="checker">Independent lot-sizing solution checker.</param>
    /// <param name="applier">Checker-result domain integration policy.</param>
    public LotSizingSolutionVerificationService(
        ILotSizingSolutionChecker checker,
        ISolutionCheckResultApplier applier)
    {
        _checker =
            checker ??
            throw new ArgumentNullException(nameof(checker));

        _applier =
            applier ??
            throw new ArgumentNullException(nameof(applier));
    }

    /// <summary>
    /// Checks a standalone candidate solution and optionally writes the
    /// resulting evaluation into the candidate.
    /// </summary>
    /// <param name="instance">Lot-sizing instance defining the problem.</param>
    /// <param name="solution">Candidate solution to verify.</param>
    /// <param name="options">Optional verification configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The independent check and application results.</returns>
    public Task<LotSizingSolutionVerificationResult> VerifyAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionVerificationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return VerifyCoreAsync(
            instance,
            solution,
            knownResult: null,
            mathematicalModel: null,
            CloneVerificationOptions(options),
            cancellationToken);
    }

    /// <summary>
    /// Checks a standalone candidate solution while reusing a mathematical
    /// model already built for the same instance.
    /// </summary>
    /// <param name="instance">Lot-sizing instance defining the problem.</param>
    /// <param name="solution">Candidate solution to verify.</param>
    /// <param name="mathematicalModel">
    /// Prebuilt mathematical model representing <paramref name="instance"/>.
    /// </param>
    /// <param name="options">Optional verification configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The independent check and application results.</returns>
    public Task<LotSizingSolutionVerificationResult> VerifyAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        MathematicalModel mathematicalModel,
        SolutionVerificationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mathematicalModel);

        return VerifyCoreAsync(
            instance,
            solution,
            knownResult: null,
            mathematicalModel,
            CloneVerificationOptions(options),
            cancellationToken);
    }

    /// <summary>
    /// Checks the detailed solution attached to a known result and optionally
    /// updates both the detailed solution evaluation and the known-result
    /// verification metadata.
    /// </summary>
    /// <remarks>
    /// When the known result contains a reported objective value, that value
    /// is used as the independent external reference for the objective check.
    /// This avoids comparing the checker result with an objective value that
    /// may already have been recomputed and stored in the detailed solution.
    /// </remarks>
    /// <param name="instance">Lot-sizing instance defining the problem.</param>
    /// <param name="knownResult">Known result whose detailed solution is checked.</param>
    /// <param name="options">Optional verification configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The independent check and application results.</returns>
    public Task<LotSizingSolutionVerificationResult> VerifyKnownResultAsync(
        LotSizingInstance instance,
        KnownResult knownResult,
        SolutionVerificationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return VerifyKnownResultCoreAsync(
            instance,
            knownResult,
            mathematicalModel: null,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Checks the detailed solution attached to a known result while reusing
    /// a mathematical model already built for the same instance.
    /// </summary>
    /// <remarks>
    /// When available, <c>knownResult.ReportedObjectiveValue</c> is used as
    /// the objective value reported by the producing solver. The checker then
    /// recomputes the objective independently from the detailed decisions and
    /// compares the two values.
    /// </remarks>
    /// <param name="instance">Lot-sizing instance defining the problem.</param>
    /// <param name="knownResult">Known result whose detailed solution is checked.</param>
    /// <param name="mathematicalModel">
    /// Prebuilt mathematical model representing <paramref name="instance"/>.
    /// </param>
    /// <param name="options">Optional verification configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The independent check and application results.</returns>
    public Task<LotSizingSolutionVerificationResult> VerifyKnownResultAsync(
        LotSizingInstance instance,
        KnownResult knownResult,
        MathematicalModel mathematicalModel,
        SolutionVerificationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mathematicalModel);

        return VerifyKnownResultCoreAsync(
            instance,
            knownResult,
            mathematicalModel,
            options,
            cancellationToken);
    }

    private Task<LotSizingSolutionVerificationResult> VerifyKnownResultCoreAsync(
        LotSizingInstance instance,
        KnownResult knownResult,
        MathematicalModel? mathematicalModel,
        SolutionVerificationOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(knownResult);

        if (knownResult.DetailedSolution is null)
        {
            throw new InvalidOperationException(
                "The known result does not contain a detailed solution " +
                "that can be independently checked.");
        }

        SolutionVerificationOptions effectiveOptions =
            CloneVerificationOptions(options);

        if (knownResult.ReportedObjectiveValue.HasValue)
        {
            effectiveOptions.CheckOptions.ReportedObjectiveValueOverride =
                knownResult.ReportedObjectiveValue.Value;
        }

        return VerifyCoreAsync(
            instance,
            knownResult.DetailedSolution,
            knownResult,
            mathematicalModel,
            effectiveOptions,
            cancellationToken);
    }

    private async Task<LotSizingSolutionVerificationResult> VerifyCoreAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        KnownResult? knownResult,
        MathematicalModel? mathematicalModel,
        SolutionVerificationOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();

        SolutionCheckResult checkResult =
            mathematicalModel is null
                ? await _checker.CheckAsync(
                    instance,
                    solution,
                    options.CheckOptions,
                    cancellationToken)
                    .ConfigureAwait(false)
                : await _checker.CheckAsync(
                    instance,
                    solution,
                    mathematicalModel,
                    options.CheckOptions,
                    cancellationToken)
                    .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        DateTime evaluatedAtUtc =
            DateTime.UtcNow;

        SolutionCheckApplicationResult applicationResult =
            _applier.Apply(
                solution,
                checkResult,
                options,
                evaluatedAtUtc,
                knownResult);

        return new LotSizingSolutionVerificationResult
        {
            CheckResult =
                checkResult,
            ApplicationResult =
                applicationResult
        };
    }

    private static SolutionVerificationOptions CloneVerificationOptions(
        SolutionVerificationOptions? source)
    {
        source ??=
            new SolutionVerificationOptions();

        source.EnsureValid();

        return new SolutionVerificationOptions
        {
            CheckOptions =
                CloneCheckOptions(source.CheckOptions),
            ApplyToSolutionEvaluation =
                source.ApplyToSolutionEvaluation,
            UpdateKnownResultFeasibility =
                source.UpdateKnownResultFeasibility,
            PromoteFullyVerifiedKnownResult =
                source.PromoteFullyVerifiedKnownResult,
            EvaluatorName =
                source.EvaluatorName,
            EvaluatorVersion =
                source.EvaluatorVersion
        };
    }

    private static SolutionCheckOptions CloneCheckOptions(
        SolutionCheckOptions source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new SolutionCheckOptions
        {
            Level =
                source.Level,
            FeasibilityTolerance =
                source.FeasibilityTolerance,
            ZeroTolerance =
                source.ZeroTolerance,
            IntegralityTolerance =
                source.IntegralityTolerance,
            ObjectiveAbsoluteTolerance =
                source.ObjectiveAbsoluteTolerance,
            ObjectiveRelativeTolerance =
                source.ObjectiveRelativeTolerance,
            ReportedObjectiveValueOverride =
                source.ReportedObjectiveValueOverride,
            IgnoreDisabledConstraints =
                source.IgnoreDisabledConstraints,
            ContinueAfterStructuralErrors =
                source.ContinueAfterStructuralErrors
        };
    }
}
