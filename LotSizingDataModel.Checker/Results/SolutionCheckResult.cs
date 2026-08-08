using LotSizingDataModel.Checker.Common;

namespace LotSizingDataModel.Checker.Results;

/// <summary>
/// Contains the complete result of checking one lot-sizing solution.
/// </summary>
public sealed class SolutionCheckResult
{
    private readonly List<SolutionCheckIssue> _issues = new();

    /// <summary>
    /// Gets or sets the checking level that was executed.
    /// </summary>
    public SolutionCheckLevel Level
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets whether structural checking completed.
    /// </summary>
    public bool StructuralCheckCompleted
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether variable-domain checking completed.
    /// </summary>
    public bool VariableDomainCheckCompleted
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether mathematical feasibility checking completed.
    /// </summary>
    public bool FeasibilityCheckCompleted
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether objective checking completed.
    /// </summary>
    public bool ObjectiveCheckCompleted
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether structural validation succeeded.
    /// </summary>
    public bool IsStructurallyValid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether all checked variable-domain conditions are satisfied.
    /// </summary>
    public bool AreVariableDomainsValid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether all checked mathematical constraints are feasible.
    /// </summary>
    public bool IsFeasible
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether objective-value verification succeeded.
    /// </summary>
    public bool IsObjectiveConsistent
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective value independently recomputed by the checker.
    /// </summary>
    public double? RecomputedObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective value reported by the candidate solution.
    /// </summary>
    public double? ReportedObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the absolute objective-value difference.
    /// </summary>
    public double? ObjectiveDifference
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the relative objective-value difference, scaled by
    /// max(1, abs(reported), abs(recomputed)).
    /// </summary>
    public double? ObjectiveRelativeDifference
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the effective absolute comparison tolerance used for
    /// objective-value verification.
    /// </summary>
    public double? ObjectiveComparisonTolerance
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum mathematical-constraint violation.
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
    /// Gets or sets the number of violated mathematical constraints.
    /// </summary>
    public int ViolatedConstraintCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets all checker diagnostics.
    /// </summary>
    public IReadOnlyList<SolutionCheckIssue> Issues =>
        _issues;

    /// <summary>
    /// Gets whether the solution passed every check requested by
    /// <see cref="Level"/>.
    /// </summary>
    public bool IsValid =>
        Level switch
        {
            SolutionCheckLevel.Structural =>
                IsStructurallyValid,

            SolutionCheckLevel.Feasibility =>
                IsStructurallyValid &&
                AreVariableDomainsValid &&
                IsFeasible,

            SolutionCheckLevel.Full =>
                IsStructurallyValid &&
                AreVariableDomainsValid &&
                IsFeasible &&
                IsObjectiveConsistent,

            _ =>
                false
        };

    /// <summary>
    /// Adds one issue to the result.
    /// </summary>
    /// <param name="issue">Issue to append.</param>
    public void AddIssue(
        SolutionCheckIssue issue)
    {
        ArgumentNullException.ThrowIfNull(
            issue);

        _issues.Add(
            issue);
    }
}
