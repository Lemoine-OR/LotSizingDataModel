using LotSizingDataModel.Checker.Facade;
using LotSizingDataModel.Checker.Reporting;

namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Contains the execution and checker result for one batch candidate.
/// </summary>
public sealed class SolutionVerificationBatchItemResult
{
    /// <summary>
    /// Gets the zero-based position of the candidate in the original input.
    /// </summary>
    public int Index
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the stable caller-defined candidate identifier.
    /// </summary>
    public string CandidateKey
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the optional human-readable candidate name.
    /// </summary>
    public string? CandidateName
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the candidate representation kind.
    /// </summary>
    public SolutionVerificationBatchCandidateKind Kind
    {
        get;
        init;
    }

    /// <summary>
    /// Gets whether verification execution completed without an unexpected
    /// per-candidate exception.
    /// </summary>
    public bool ExecutionSucceeded
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the complete verification result when execution succeeded.
    /// </summary>
    public LotSizingSolutionVerificationResult? VerificationResult
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the detached compact checker summary when execution succeeded.
    /// </summary>
    public SolutionCheckSummary? Summary
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the fully qualified exception type when execution failed.
    /// </summary>
    public string? FailureExceptionType
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the exception message when execution failed.
    /// </summary>
    public string? FailureMessage
    {
        get;
        init;
    }

    /// <summary>
    /// Gets whether the candidate passed every requested independent check.
    /// </summary>
    public bool IsValid =>
        ExecutionSucceeded &&
        Summary?.IsValid == true;
}
