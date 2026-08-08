namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Reports deterministic progress information for a running batch-verification
/// operation.
/// </summary>
public sealed class SolutionVerificationBatchProgress
{
    /// <summary>
    /// Gets the total number of candidates in the materialized batch.
    /// </summary>
    public int TotalCandidateCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of candidates whose verification attempt has completed.
    /// </summary>
    public int CompletedCandidateCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the zero-based index of the most recently completed candidate, or
    /// <c>-1</c> before any candidate has completed.
    /// </summary>
    public int LastCompletedCandidateIndex
    {
        get;
        init;
    } = -1;

    /// <summary>
    /// Gets the stable key of the most recently completed candidate.
    /// </summary>
    public string? LastCompletedCandidateKey
    {
        get;
        init;
    }

    /// <summary>
    /// Gets whether the most recently completed candidate finished without an
    /// unexpected execution exception.
    /// </summary>
    public bool? LastExecutionSucceeded
    {
        get;
        init;
    }

    /// <summary>
    /// Gets whether the most recently completed candidate passed every
    /// requested check when execution succeeded.
    /// </summary>
    public bool? LastCandidateIsValid
    {
        get;
        init;
    }
}
