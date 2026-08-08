using LotSizingDataModel.Checker.Batch;

namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Associates a batch checker item with the serialized source from which the
/// candidate was created.
/// </summary>
public sealed class DirectoryVerificationCampaignItem
{
    /// <summary>
    /// Gets the source metadata for this candidate.
    /// </summary>
    public DirectoryVerificationCandidateSource Source
    {
        get;
        init;
    } = new();

    /// <summary>
    /// Gets the checker execution result for this candidate.
    /// </summary>
    public SolutionVerificationBatchItemResult Verification
    {
        get;
        init;
    } = new();
}
