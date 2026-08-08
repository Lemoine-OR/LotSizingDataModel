using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Results;

namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Builds compact summaries from complete checker results.
/// </summary>
public static class SolutionCheckSummaryFactory
{
    /// <summary>
    /// Creates a compact machine-oriented summary.
    /// </summary>
    /// <param name="result">Complete checker result.</param>
    /// <returns>A compact summary containing no mutable reference to the result.</returns>
    public static SolutionCheckSummary Create(
        SolutionCheckResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var summary =
            new SolutionCheckSummary
            {
                Level =
                    result.Level,
                IsValid =
                    result.IsValid,
                StructuralStatus =
                    GetStatus(
                        requested: true,
                        completed: result.StructuralCheckCompleted,
                        passed: result.IsStructurallyValid),
                VariableDomainStatus =
                    GetStatus(
                        requested:
                            result.Level >= SolutionCheckLevel.Feasibility,
                        completed: result.VariableDomainCheckCompleted,
                        passed: result.AreVariableDomainsValid),
                FeasibilityStatus =
                    GetStatus(
                        requested:
                            result.Level >= SolutionCheckLevel.Feasibility,
                        completed: result.FeasibilityCheckCompleted,
                        passed: result.IsFeasible),
                ObjectiveStatus =
                    GetStatus(
                        requested:
                            result.Level == SolutionCheckLevel.Full,
                        completed: result.ObjectiveCheckCompleted,
                        passed: result.IsObjectiveConsistent),
                IssueCount =
                    result.Issues.Count,
                InformationCount =
                    result.Issues.Count(
                        issue =>
                            issue.Severity ==
                            SolutionCheckSeverity.Information),
                WarningCount =
                    result.Issues.Count(
                        issue =>
                            issue.Severity ==
                            SolutionCheckSeverity.Warning),
                ErrorCount =
                    result.Issues.Count(
                        issue =>
                            issue.Severity ==
                            SolutionCheckSeverity.Error),
                ViolatedConstraintCount =
                    result.ViolatedConstraintCount,
                MaximumConstraintViolation =
                    result.MaximumConstraintViolation,
                TotalConstraintViolation =
                    result.TotalConstraintViolation,
                ReportedObjectiveValue =
                    result.ReportedObjectiveValue,
                RecomputedObjectiveValue =
                    result.RecomputedObjectiveValue,
                ObjectiveDifference =
                    result.ObjectiveDifference,
                ObjectiveRelativeDifference =
                    result.ObjectiveRelativeDifference,
                ObjectiveComparisonTolerance =
                    result.ObjectiveComparisonTolerance
            };

        foreach (SolutionCheckIssueKind kind in
                 Enum.GetValues<SolutionCheckIssueKind>())
        {
            int count =
                result.Issues.Count(
                    issue =>
                        issue.Kind == kind);

            if (count == 0)
            {
                continue;
            }

            summary.IssueCountsByKind.Add(
                new SolutionCheckIssueCount
                {
                    Kind =
                        kind,
                    Count =
                        count
                });
        }

        return summary;
    }

    private static SolutionCheckStageStatus GetStatus(
        bool requested,
        bool completed,
        bool passed)
    {
        if (!requested)
        {
            return SolutionCheckStageStatus.NotRequested;
        }

        if (!completed)
        {
            return SolutionCheckStageStatus.NotCompleted;
        }

        return passed
            ? SolutionCheckStageStatus.Passed
            : SolutionCheckStageStatus.Failed;
    }
}
