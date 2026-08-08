namespace LotSizingDataModel.Checker.Common;

/// <summary>
/// Defines the severity of a solution-check issue.
/// </summary>
public enum SolutionCheckSeverity
{
    /// <summary>
    /// Informational diagnostic.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Warning that does not by itself invalidate the solution.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error that invalidates at least one checked property of the solution.
    /// </summary>
    Error = 2
}
