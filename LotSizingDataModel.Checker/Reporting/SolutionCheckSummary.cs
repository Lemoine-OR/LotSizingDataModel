using LotSizingDataModel.Checker.Common;

namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Provides a compact machine-oriented summary of one solution check.
/// </summary>
public sealed class SolutionCheckSummary
{
    /// <summary>
    /// Gets or sets the checking level requested for the candidate.
    /// </summary>
    public SolutionCheckLevel Level
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether every requested check passed.
    /// </summary>
    public bool IsValid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the structural-stage status.
    /// </summary>
    public SolutionCheckStageStatus StructuralStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the variable-domain-stage status.
    /// </summary>
    public SolutionCheckStageStatus VariableDomainStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mathematical-feasibility-stage status.
    /// </summary>
    public SolutionCheckStageStatus FeasibilityStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective-check-stage status.
    /// </summary>
    public SolutionCheckStageStatus ObjectiveStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total number of diagnostics.
    /// </summary>
    public int IssueCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of informational diagnostics.
    /// </summary>
    public int InformationCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of warning diagnostics.
    /// </summary>
    public int WarningCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of error diagnostics.
    /// </summary>
    public int ErrorCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets issue counts grouped by issue category.
    /// </summary>
    public List<SolutionCheckIssueCount> IssueCountsByKind
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Gets or sets the number of violated mathematical constraints.
    /// </summary>
    public int ViolatedConstraintCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the largest mathematical-constraint violation.
    /// </summary>
    public double MaximumConstraintViolation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the sum of mathematical-constraint violations.
    /// </summary>
    public double TotalConstraintViolation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective value reported by the candidate.
    /// </summary>
    public double? ReportedObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective value independently recomputed by the
    /// checker.
    /// </summary>
    public double? RecomputedObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the absolute objective difference.
    /// </summary>
    public double? ObjectiveDifference
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the relative objective difference.
    /// </summary>
    public double? ObjectiveRelativeDifference
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the effective objective comparison tolerance.
    /// </summary>
    public double? ObjectiveComparisonTolerance
    {
        get;
        set;
    }
}
