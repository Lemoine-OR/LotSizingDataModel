using LotSizingDataModel.Checker.Common;

namespace LotSizingDataModel.Checker.Results;

/// <summary>
/// Describes one issue detected by a lot-sizing solution checker.
/// </summary>
public sealed class SolutionCheckIssue
{
    /// <summary>
    /// Gets or sets the issue severity.
    /// </summary>
    public SolutionCheckSeverity Severity
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the issue category.
    /// </summary>
    public SolutionCheckIssueKind Kind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the related mathematical-domain key, when available.
    /// </summary>
    public string? DomainKey
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the related mathematical constraint name, when available.
    /// </summary>
    public string? ConstraintName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the actual value involved in the issue, when meaningful.
    /// </summary>
    public double? ActualValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an expected or reference value, when meaningful.
    /// </summary>
    public double? ExpectedValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the numerical violation magnitude, when meaningful.
    /// </summary>
    public double? Violation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a human-readable diagnostic message.
    /// </summary>
    public string Message
    {
        get;
        set;
    } = string.Empty;
}
