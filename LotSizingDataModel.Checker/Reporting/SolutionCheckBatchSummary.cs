namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Contains aggregated metrics for a collection of checked solutions.
/// </summary>
public sealed class SolutionCheckBatchSummary
{
    /// <summary>
    /// Gets or sets the number of candidates represented by the summary.
    /// </summary>
    public int CandidateCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of candidates that passed all requested checks.
    /// </summary>
    public int ValidCandidateCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of candidates that did not pass all requested
    /// checks.
    /// </summary>
    public int InvalidCandidateCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total number of informational diagnostics.
    /// </summary>
    public int InformationCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total number of warnings.
    /// </summary>
    public int WarningCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total number of errors.
    /// </summary>
    public int ErrorCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of candidates with at least one violated
    /// mathematical constraint.
    /// </summary>
    public int CandidateWithConstraintViolationCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of candidates for which objective checking
    /// completed and detected an inconsistency.
    /// </summary>
    public int CandidateWithObjectiveMismatchCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the largest constraint violation observed in the batch.
    /// </summary>
    public double MaximumConstraintViolation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets issue counts aggregated by category.
    /// </summary>
    public List<SolutionCheckIssueCount> IssueCountsByKind
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Gets or sets the per-candidate summaries in insertion order.
    /// </summary>
    public List<SolutionCheckBatchItemSummary> Items
    {
        get;
        set;
    } = new();
}
