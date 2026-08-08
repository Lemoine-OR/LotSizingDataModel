namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Associates a batch candidate identifier with its compact checker summary.
/// </summary>
public sealed class SolutionCheckBatchItemSummary
{
    /// <summary>
    /// Gets or sets the caller-defined candidate identifier.
    /// </summary>
    public string CandidateKey
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Gets or sets an optional human-readable candidate name.
    /// </summary>
    public string? CandidateName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the compact checker summary for this candidate.
    /// </summary>
    public SolutionCheckSummary Summary
    {
        get;
        set;
    } = new();
}
