namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Configures human-readable checker reports.
/// </summary>
public sealed class SolutionCheckReportOptions
{
    /// <summary>
    /// Gets or sets whether stage status details are included.
    /// </summary>
    public bool IncludeStageDetails
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether mathematical feasibility metrics are included.
    /// </summary>
    public bool IncludeFeasibilityMetrics
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether objective-comparison metrics are included.
    /// </summary>
    public bool IncludeObjectiveMetrics
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether informational diagnostics are included.
    /// </summary>
    public bool IncludeInformationIssues
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether warning diagnostics are included.
    /// </summary>
    public bool IncludeWarningIssues
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether error diagnostics are included.
    /// </summary>
    public bool IncludeErrorIssues
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets the maximum number of detailed issues written per
    /// candidate. Zero means no limit.
    /// </summary>
    public int MaximumDetailedIssues
    {
        get;
        set;
    } = 100;

    /// <summary>
    /// Gets or sets whether detailed issues are sorted by decreasing
    /// severity before formatting.
    /// </summary>
    public bool SortIssuesBySeverity
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Validates the report options.
    /// </summary>
    public void EnsureValid()
    {
        if (MaximumDetailedIssues < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumDetailedIssues),
                MaximumDetailedIssues,
                "The maximum number of detailed issues cannot be negative.");
        }
    }
}
