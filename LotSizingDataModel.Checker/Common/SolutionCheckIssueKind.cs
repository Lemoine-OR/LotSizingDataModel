namespace LotSizingDataModel.Checker.Common;

/// <summary>
/// Identifies the category of an issue detected while checking a solution.
/// </summary>
public enum SolutionCheckIssueKind
{
    /// <summary>
    /// The issue could not be classified more precisely.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The solution structure is inconsistent with the instance.
    /// </summary>
    Structural = 1,

    /// <summary>
    /// A decision value violates its numerical domain.
    /// </summary>
    VariableDomain = 2,

    /// <summary>
    /// A mathematical variable required by the model has no candidate value.
    /// </summary>
    MissingVariableValue = 3,

    /// <summary>
    /// A mathematical constraint is violated.
    /// </summary>
    ConstraintViolation = 4,

    /// <summary>
    /// The recomputed objective is inconsistent with a reported value.
    /// </summary>
    ObjectiveMismatch = 5,

    /// <summary>
    /// A requested check could not be completed.
    /// </summary>
    CheckFailure = 6
}
