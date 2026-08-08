namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Identifies the domain object represented by a batch-verification candidate.
/// </summary>
public enum SolutionVerificationBatchCandidateKind
{
    /// <summary>
    /// The candidate is a standalone <c>LotSizingSolution</c>.
    /// </summary>
    StandaloneSolution = 0,

    /// <summary>
    /// The candidate is the detailed solution attached to a known result.
    /// </summary>
    KnownResult = 1
}
