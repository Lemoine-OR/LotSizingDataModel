namespace LotSizingDataModel.Checker.Common;

/// <summary>
/// Defines how deeply a lot-sizing solution must be checked.
/// </summary>
public enum SolutionCheckLevel
{
    /// <summary>
    /// Checks only the structural consistency of the solution.
    /// </summary>
    Structural = 0,

    /// <summary>
    /// Checks structure, numerical domains, and mathematical feasibility.
    /// </summary>
    Feasibility = 1,

    /// <summary>
    /// Performs all available checks, including objective-value verification.
    /// </summary>
    Full = 2
}
