namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Identifies report files produced by a completed directory campaign.
/// </summary>
public sealed class DirectoryVerificationCampaignReportFiles
{
    /// <summary>
    /// Gets the path of the global human-readable campaign summary.
    /// </summary>
    public string? SummaryReportPath
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the path of the tab-separated campaign manifest.
    /// </summary>
    public string? ManifestPath
    {
        get;
        init;
    }


    /// <summary>
    /// Gets the path of the compact global validation report containing one
    /// line per candidate plus aggregate checker statistics.
    /// </summary>
    public string? GlobalValidationReportPath
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the individual candidate report paths in candidate order.
    /// </summary>
    public IReadOnlyList<string> CandidateReportPaths
    {
        get;
        init;
    } = Array.Empty<string>();
}
