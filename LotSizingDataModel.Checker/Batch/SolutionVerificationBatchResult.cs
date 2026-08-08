using LotSizingDataModel.Checker.Reporting;

namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Contains the deterministic result of a complete batch-verification run.
/// </summary>
public sealed class SolutionVerificationBatchResult
{
    /// <summary>
    /// Gets the per-candidate results in exactly the same order as the input.
    /// </summary>
    public IReadOnlyList<SolutionVerificationBatchItemResult> Items
    {
        get;
        init;
    } = Array.Empty<SolutionVerificationBatchItemResult>();

    /// <summary>
    /// Gets the checker-only aggregate summary for candidates whose execution
    /// reached a checker result.
    /// </summary>
    /// <remarks>
    /// Candidates that failed because of an unexpected execution exception are
    /// deliberately excluded from this checker summary and are counted through
    /// <see cref="ExecutionFailureCount"/> instead.
    /// </remarks>
    public SolutionCheckBatchSummary CheckSummary
    {
        get;
        init;
    } = new();

    /// <summary>
    /// Gets the total number of input candidates.
    /// </summary>
    public int CandidateCount =>
        Items.Count;

    /// <summary>
    /// Gets the number of candidates for which verification execution
    /// completed normally.
    /// </summary>
    public int CompletedCandidateCount =>
        Items.Count(item => item.ExecutionSucceeded);

    /// <summary>
    /// Gets the number of candidates that failed because of an unexpected
    /// execution exception.
    /// </summary>
    public int ExecutionFailureCount =>
        Items.Count(item => !item.ExecutionSucceeded);

    /// <summary>
    /// Gets the number of candidates that were checked successfully and passed
    /// all requested checks.
    /// </summary>
    public int ValidCandidateCount =>
        Items.Count(item => item.IsValid);

    /// <summary>
    /// Gets the number of candidates whose execution succeeded but whose
    /// solution did not pass all requested checks.
    /// </summary>
    public int InvalidCandidateCount =>
        Items.Count(
            item =>
                item.ExecutionSucceeded &&
                !item.IsValid);

    /// <summary>
    /// Gets whether no candidate suffered an execution failure.
    /// </summary>
    public bool ExecutionSucceeded =>
        ExecutionFailureCount == 0;

    /// <summary>
    /// Gets whether every candidate executed successfully and passed every
    /// requested check.
    /// </summary>
    public bool IsValid =>
        CandidateCount == CompletedCandidateCount &&
        InvalidCandidateCount == 0;
}
