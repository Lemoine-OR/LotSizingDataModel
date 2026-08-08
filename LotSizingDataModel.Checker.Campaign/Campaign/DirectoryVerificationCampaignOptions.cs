using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Reporting;
using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Configures discovery, selection, verification and report generation for a
/// directory-based lot-sizing validation campaign.
/// </summary>
public sealed class DirectoryVerificationCampaignOptions
{
    /// <summary>
    /// Gets or sets the file search pattern used when discovering candidate
    /// instance files.
    /// </summary>
    public string SearchPattern
    {
        get;
        set;
    } = "*.xml";

    /// <summary>
    /// Gets or sets whether input discovery recursively scans subdirectories.
    /// </summary>
    public bool SearchSubdirectories
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether XML files whose root element is not
    /// <c>lotSizingInstance</c> are ignored instead of reported as load
    /// failures.
    /// </summary>
    public bool IgnoreNonLotSizingInstanceXml
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets an optional predicate selecting known results after an
    /// instance has been loaded.
    /// </summary>
    /// <remarks>
    /// A known result must still contain a detailed solution to become a
    /// verification candidate. The predicate is evaluated sequentially during
    /// campaign preparation and is never invoked concurrently by the service.
    /// </remarks>
    public Func<KnownResult, bool>? KnownResultPredicate
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the bounded-parallelism and checker policy used for the
    /// selected candidates.
    /// </summary>
    public SolutionVerificationBatchOptions BatchOptions
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Gets or sets whether the campaign writes human-readable report files.
    /// </summary>
    public bool WriteReports
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets the report formatting policy.
    /// </summary>
    public SolutionCheckReportOptions ReportOptions
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Gets or sets whether existing campaign report files are replaced.
    /// </summary>
    /// <remarks>
    /// Only files owned by this campaign writer are overwritten. The service
    /// never deletes the report directory recursively.
    /// </remarks>
    public bool OverwriteExistingReports
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Validates the campaign configuration.
    /// </summary>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(SearchPattern))
        {
            throw new InvalidOperationException(
                "SearchPattern cannot be empty.");
        }

        if (BatchOptions is null)
        {
            throw new InvalidOperationException(
                "BatchOptions cannot be null.");
        }

        if (ReportOptions is null)
        {
            throw new InvalidOperationException(
                "ReportOptions cannot be null.");
        }

        BatchOptions.EnsureValid();
        ReportOptions.EnsureValid();
    }
}
