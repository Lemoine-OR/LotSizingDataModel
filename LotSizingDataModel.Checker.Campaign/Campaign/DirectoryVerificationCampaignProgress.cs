namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Reports high-level, count-based progress for a directory verification
/// campaign.
/// </summary>
public sealed class DirectoryVerificationCampaignProgress
{
    /// <summary>
    /// Gets the current campaign stage.
    /// </summary>
    public DirectoryVerificationCampaignStage Stage
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the total number of discovered files matching the search pattern.
    /// </summary>
    public int DiscoveredFileCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of discovered files already inspected by the loader.
    /// </summary>
    public int ProcessedFileCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of lot-sizing instances loaded successfully so far.
    /// </summary>
    public int LoadedInstanceCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the total number of selected verification candidates when known.
    /// </summary>
    public int CandidateCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of selected candidates whose verification attempt has
    /// completed.
    /// </summary>
    public int CompletedCandidateCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets an optional stable key describing the most recently completed
    /// candidate.
    /// </summary>
    public string? LastCandidateKey
    {
        get;
        init;
    }
}
