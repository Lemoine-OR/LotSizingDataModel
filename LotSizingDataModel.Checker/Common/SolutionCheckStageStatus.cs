namespace LotSizingDataModel.Checker.Common;

/// <summary>
/// Describes the execution and outcome of one logical checker stage.
/// </summary>
public enum SolutionCheckStageStatus
{
    /// <summary>
    /// The stage was not requested by the selected checking level.
    /// </summary>
    NotRequested = 0,

    /// <summary>
    /// The stage was requested but could not be executed to completion.
    /// </summary>
    NotCompleted = 1,

    /// <summary>
    /// The stage completed and its checked conditions were satisfied.
    /// </summary>
    Passed = 2,

    /// <summary>
    /// The stage completed and at least one checked condition failed.
    /// </summary>
    Failed = 3
}
