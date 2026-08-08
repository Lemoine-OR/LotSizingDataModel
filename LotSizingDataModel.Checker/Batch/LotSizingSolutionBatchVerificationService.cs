using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Facade;
using LotSizingDataModel.Checker.Reporting;

namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Executes independent lot-sizing solution verification over a collection of
/// candidates with bounded parallelism and deterministic result ordering.
/// </summary>
/// <remarks>
/// <para>
/// Unexpected exceptions are isolated per candidate and converted into failed
/// batch items. They do not stop verification of unrelated candidates.
/// </para>
/// <para>
/// Cancellation is different: when the supplied cancellation token is
/// cancelled, the operation stops cooperatively and propagates
/// <see cref="OperationCanceledException"/> to the caller. A cancelled batch is
/// therefore never presented as a completed validation campaign.
/// </para>
/// </remarks>
public sealed class LotSizingSolutionBatchVerificationService
{
    private readonly Func<LotSizingSolutionVerificationService>
        _verificationServiceFactory;

    /// <summary>
    /// Initializes the batch service with the standard verification facade.
    /// </summary>
    public LotSizingSolutionBatchVerificationService()
        : this(
            static () =>
                new LotSizingSolutionVerificationService())
    {
    }

    /// <summary>
    /// Initializes the batch service with a factory creating an independent
    /// verification facade for each concurrently processed candidate.
    /// </summary>
    /// <param name="verificationServiceFactory">
    /// Factory used to create verification facades. It may be used
    /// concurrently and must return a non-null service on every call.
    /// </param>
    public LotSizingSolutionBatchVerificationService(
        Func<LotSizingSolutionVerificationService>
            verificationServiceFactory)
    {
        _verificationServiceFactory =
            verificationServiceFactory ??
            throw new ArgumentNullException(
                nameof(verificationServiceFactory));
    }

    /// <summary>
    /// Verifies all candidates with bounded parallelism.
    /// </summary>
    /// <param name="candidates">Candidates to verify.</param>
    /// <param name="options">Optional batch execution configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Per-candidate results in input order plus a checker-only batch summary.
    /// </returns>
    public Task<SolutionVerificationBatchResult> VerifyAsync(
        IEnumerable<SolutionVerificationBatchCandidate> candidates,
        SolutionVerificationBatchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return VerifyAsync(
            candidates,
            options,
            progress: null,
            cancellationToken);
    }

    /// <summary>
    /// Verifies all candidates with bounded parallelism and reports completed
    /// candidate counts while preserving deterministic result ordering.
    /// </summary>
    /// <param name="candidates">Candidates to verify.</param>
    /// <param name="options">Optional batch execution configuration.</param>
    /// <param name="progress">Optional progress sink.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Per-candidate results in input order plus a checker-only batch summary.
    /// </returns>
    public async Task<SolutionVerificationBatchResult> VerifyAsync(
        IEnumerable<SolutionVerificationBatchCandidate> candidates,
        SolutionVerificationBatchOptions? options,
        IProgress<SolutionVerificationBatchProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        options ??=
            new SolutionVerificationBatchOptions();

        options.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        SolutionVerificationBatchCandidate[] materializedCandidates =
            MaterializeAndValidateCandidates(candidates);

        ReportProgress(
            progress,
            totalCandidateCount: materializedCandidates.Length,
            completedCandidateCount: 0,
            lastItem: null);

        if (materializedCandidates.Length == 0)
        {
            return new SolutionVerificationBatchResult
            {
                Items =
                    Array.Empty<SolutionVerificationBatchItemResult>(),
                CheckSummary =
                    new SolutionCheckBatchSummary()
            };
        }

        var results =
            new SolutionVerificationBatchItemResult[
                materializedCandidates.Length];

        int completedCandidateCount =
            0;

        var parallelOptions =
            new ParallelOptions
            {
                CancellationToken =
                    cancellationToken,
                MaxDegreeOfParallelism =
                    options.MaxDegreeOfParallelism
            };

        await Parallel.ForEachAsync(
            Enumerable.Range(
                0,
                materializedCandidates.Length),
            parallelOptions,
            async (index, token) =>
            {
                SolutionVerificationBatchCandidate candidate =
                    materializedCandidates[index];

                SolutionVerificationBatchItemResult item =
                    await VerifyCandidateAsync(
                        index,
                        candidate,
                        options.VerificationOptions,
                        token)
                        .ConfigureAwait(false);

                results[index] =
                    item;

                int completed =
                    Interlocked.Increment(
                        ref completedCandidateCount);

                ReportProgress(
                    progress,
                    materializedCandidates.Length,
                    completed,
                    item);
            })
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        SolutionCheckBatchSummary checkSummary =
            BuildCheckSummary(results);

        return new SolutionVerificationBatchResult
        {
            Items =
                Array.AsReadOnly(results),
            CheckSummary =
                checkSummary
        };
    }

    private static void ReportProgress(
        IProgress<SolutionVerificationBatchProgress>? progress,
        int totalCandidateCount,
        int completedCandidateCount,
        SolutionVerificationBatchItemResult? lastItem)
    {
        progress?.Report(
            new SolutionVerificationBatchProgress
            {
                TotalCandidateCount =
                    totalCandidateCount,
                CompletedCandidateCount =
                    completedCandidateCount,
                LastCompletedCandidateIndex =
                    lastItem?.Index ?? -1,
                LastCompletedCandidateKey =
                    lastItem?.CandidateKey,
                LastExecutionSucceeded =
                    lastItem?.ExecutionSucceeded,
                LastCandidateIsValid =
                    lastItem is null
                        ? null
                        : lastItem.IsValid
            });
    }

    private async Task<SolutionVerificationBatchItemResult>
        VerifyCandidateAsync(
            int index,
            SolutionVerificationBatchCandidate candidate,
            SolutionVerificationOptions verificationOptions,
            CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LotSizingSolutionVerificationService verificationService =
                _verificationServiceFactory() ??
                throw new InvalidOperationException(
                    "The verification-service factory returned null.");

            SolutionVerificationOptions candidateOptions =
                CloneVerificationOptions(
                    verificationOptions);

            LotSizingSolutionVerificationResult verificationResult =
                candidate.Kind switch
                {
                    SolutionVerificationBatchCandidateKind.StandaloneSolution =>
                        await verificationService.VerifyAsync(
                            candidate.Instance,
                            candidate.Solution ??
                                throw new InvalidOperationException(
                                    "The standalone batch candidate has no solution."),
                            candidateOptions,
                            cancellationToken)
                            .ConfigureAwait(false),

                    SolutionVerificationBatchCandidateKind.KnownResult =>
                        await verificationService.VerifyKnownResultAsync(
                            candidate.Instance,
                            candidate.KnownResult ??
                                throw new InvalidOperationException(
                                    "The known-result batch candidate has no known result."),
                            candidateOptions,
                            cancellationToken)
                            .ConfigureAwait(false),

                    _ =>
                        throw new InvalidOperationException(
                            "The batch candidate kind is not supported.")
                };

            cancellationToken.ThrowIfCancellationRequested();

            return new SolutionVerificationBatchItemResult
            {
                Index =
                    index,
                CandidateKey =
                    candidate.CandidateKey,
                CandidateName =
                    candidate.CandidateName,
                Kind =
                    candidate.Kind,
                ExecutionSucceeded =
                    true,
                VerificationResult =
                    verificationResult,
                Summary =
                    SolutionCheckSummaryFactory.Create(
                        verificationResult.CheckResult)
            };
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new SolutionVerificationBatchItemResult
            {
                Index =
                    index,
                CandidateKey =
                    candidate.CandidateKey,
                CandidateName =
                    candidate.CandidateName,
                Kind =
                    candidate.Kind,
                ExecutionSucceeded =
                    false,
                FailureExceptionType =
                    exception.GetType().FullName ??
                    exception.GetType().Name,
                FailureMessage =
                    exception.Message
            };
        }
    }

    private static SolutionVerificationBatchCandidate[]
        MaterializeAndValidateCandidates(
            IEnumerable<SolutionVerificationBatchCandidate> candidates)
    {
        SolutionVerificationBatchCandidate[] materialized =
            candidates.ToArray();

        for (int index = 0;
             index < materialized.Length;
             index++)
        {
            if (materialized[index] is null)
            {
                throw new ArgumentException(
                    $"The candidate at index {index} is null.",
                    nameof(candidates));
            }
        }

        string? duplicateKey =
            materialized
                .GroupBy(
                    candidate =>
                        candidate.CandidateKey,
                    StringComparer.Ordinal)
                .Where(group =>
                    group.Count() > 1)
                .Select(group =>
                    group.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .FirstOrDefault();

        if (duplicateKey is not null)
        {
            throw new ArgumentException(
                $"The candidate key '{duplicateKey}' occurs more than once. " +
                "Batch candidate keys must be unique.",
                nameof(candidates));
        }

        return materialized;
    }

    private static SolutionVerificationOptions CloneVerificationOptions(
        SolutionVerificationOptions source)
    {
        return new SolutionVerificationOptions
        {
            ApplyToSolutionEvaluation =
                source.ApplyToSolutionEvaluation,
            UpdateKnownResultFeasibility =
                source.UpdateKnownResultFeasibility,
            PromoteFullyVerifiedKnownResult =
                source.PromoteFullyVerifiedKnownResult,
            EvaluatorName =
                source.EvaluatorName,
            EvaluatorVersion =
                source.EvaluatorVersion,
            CheckOptions =
                CloneCheckOptions(
                    source.CheckOptions)
        };
    }

    private static SolutionCheckOptions CloneCheckOptions(
        SolutionCheckOptions source)
    {
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

    private static SolutionCheckBatchSummary BuildCheckSummary(
        IReadOnlyList<SolutionVerificationBatchItemResult> results)
    {
        var aggregator =
            new SolutionCheckBatchAggregator();

        foreach (
            SolutionVerificationBatchItemResult item
            in results)
        {
            if (!item.ExecutionSucceeded ||
                item.VerificationResult is null)
            {
                continue;
            }

            aggregator.Add(
                item.CandidateKey,
                item.VerificationResult.CheckResult,
                item.CandidateName);
        }

        return aggregator.BuildSummary();
    }
}
