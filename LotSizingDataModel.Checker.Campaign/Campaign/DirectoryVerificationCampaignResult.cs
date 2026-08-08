using LotSizingDataModel.Checker.Batch;

namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Contains file discovery, loading, candidate selection, checker execution
/// and report metadata for one completed directory campaign.
/// </summary>
public sealed class DirectoryVerificationCampaignResult
{
    /// <summary>
    /// Gets the absolute input directory scanned by the campaign.
    /// </summary>
    public string InputDirectory
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the absolute report output directory.
    /// </summary>
    public string OutputDirectory
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the number of files matching the campaign search pattern after
    /// exclusion of the output directory.
    /// </summary>
    public int DiscoveredXmlFileCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of XML files ignored because their root element was not
    /// <c>lotSizingInstance</c>.
    /// </summary>
    public int IgnoredNonInstanceXmlFileCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of serialized lot-sizing instances loaded successfully.
    /// </summary>
    public int LoadedInstanceCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the total number of known-result records encountered in loaded
    /// instances before optional predicate filtering.
    /// </summary>
    public int KnownResultCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of known results rejected by the optional selection
    /// predicate.
    /// </summary>
    public int PredicateFilteredKnownResultCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the number of selected known results that could not be checked
    /// because they contained no detailed solution.
    /// </summary>
    public int KnownResultWithoutDetailedSolutionCount
    {
        get;
        init;
    }

    /// <summary>
    /// Gets serialized instance load failures in deterministic file order.
    /// </summary>
    public IReadOnlyList<InstanceFileLoadFailure> FileLoadFailures
    {
        get;
        init;
    } = Array.Empty<InstanceFileLoadFailure>();

    /// <summary>
    /// Gets candidate results enriched with their source-file metadata.
    /// </summary>
    public IReadOnlyList<DirectoryVerificationCampaignItem> Items
    {
        get;
        init;
    } = Array.Empty<DirectoryVerificationCampaignItem>();

    /// <summary>
    /// Gets the underlying Package 10 batch checker result.
    /// </summary>
    public SolutionVerificationBatchResult BatchResult
    {
        get;
        init;
    } = new();

    /// <summary>
    /// Gets report paths created by the campaign writer.
    /// </summary>
    public DirectoryVerificationCampaignReportFiles ReportFiles
    {
        get;
        internal set;
    } = new();

    /// <summary>
    /// Gets the number of serialized files that failed to load.
    /// </summary>
    public int FileLoadFailureCount =>
        FileLoadFailures.Count;

    /// <summary>
    /// Gets the number of selected verification candidates.
    /// </summary>
    public int CandidateCount =>
        BatchResult.CandidateCount;

    /// <summary>
    /// Gets whether the whole campaign completed without file-loading or
    /// per-candidate execution failures.
    /// </summary>
    public bool ExecutionSucceeded =>
        FileLoadFailureCount == 0 &&
        BatchResult.ExecutionSucceeded;

    /// <summary>
    /// Gets whether every loadable selected candidate executed successfully
    /// and passed every requested independent check, with no input load
    /// failure.
    /// </summary>
    public bool IsValid =>
        FileLoadFailureCount == 0 &&
        BatchResult.IsValid;
}
