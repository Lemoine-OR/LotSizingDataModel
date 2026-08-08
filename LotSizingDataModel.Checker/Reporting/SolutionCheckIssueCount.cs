using LotSizingDataModel.Checker.Common;

namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Associates an issue category with its occurrence count.
/// </summary>
public sealed class SolutionCheckIssueCount
{
    /// <summary>
    /// Gets or sets the issue category.
    /// </summary>
    public SolutionCheckIssueKind Kind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of issues in this category.
    /// </summary>
    public int Count
    {
        get;
        set;
    }
}
