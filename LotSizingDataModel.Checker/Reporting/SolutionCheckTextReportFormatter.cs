using System.Globalization;
using System.Text;
using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Results;

namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Produces deterministic plain-text reports for checker results.
/// </summary>
public sealed class SolutionCheckTextReportFormatter :
    ISolutionCheckReportFormatter
{
    private static readonly CultureInfo ReportCulture =
        CultureInfo.InvariantCulture;

    /// <inheritdoc/>
    public string Format(
        SolutionCheckResult result,
        string? candidateName = null,
        SolutionCheckReportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        options ??=
            new SolutionCheckReportOptions();

        options.EnsureValid();

        SolutionCheckSummary summary =
            SolutionCheckSummaryFactory.Create(result);

        var builder =
            new StringBuilder();

        builder.AppendLine("Lot-sizing solution check");
        builder.AppendLine(new string('=', 50));

        if (!string.IsNullOrWhiteSpace(candidateName))
        {
            builder.Append("Candidate : ");
            builder.AppendLine(candidateName);
        }

        builder.Append("Result    : ");
        builder.AppendLine(
            summary.IsValid
                ? "VALID"
                : "INVALID");
        builder.Append("Level     : ");
        builder.AppendLine(summary.Level.ToString());
        builder.Append("Issues    : ");
        builder.Append(summary.IssueCount);
        builder.Append(" (errors=");
        builder.Append(summary.ErrorCount);
        builder.Append(", warnings=");
        builder.Append(summary.WarningCount);
        builder.Append(", information=");
        builder.Append(summary.InformationCount);
        builder.AppendLine(")");

        if (options.IncludeStageDetails)
        {
            AppendStages(builder, summary);
        }

        if (options.IncludeFeasibilityMetrics &&
            summary.FeasibilityStatus !=
            SolutionCheckStageStatus.NotRequested)
        {
            AppendFeasibility(builder, summary);
        }

        if (options.IncludeObjectiveMetrics &&
            summary.ObjectiveStatus !=
            SolutionCheckStageStatus.NotRequested)
        {
            AppendObjective(builder, summary);
        }

        AppendIssues(
            builder,
            result,
            options);

        return builder
            .ToString()
            .TrimEnd();
    }

    /// <inheritdoc/>
    public string FormatBatch(
        SolutionCheckBatchSummary summary,
        SolutionCheckReportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(summary);

        options ??=
            new SolutionCheckReportOptions();

        options.EnsureValid();

        var builder =
            new StringBuilder();

        builder.AppendLine("Lot-sizing solution-check batch");
        builder.AppendLine(new string('=', 50));
        builder.Append("Candidates : ");
        builder.AppendLine(summary.CandidateCount.ToString(ReportCulture));
        builder.Append("Valid      : ");
        builder.AppendLine(summary.ValidCandidateCount.ToString(ReportCulture));
        builder.Append("Invalid    : ");
        builder.AppendLine(summary.InvalidCandidateCount.ToString(ReportCulture));
        builder.Append("Errors     : ");
        builder.AppendLine(summary.ErrorCount.ToString(ReportCulture));
        builder.Append("Warnings   : ");
        builder.AppendLine(summary.WarningCount.ToString(ReportCulture));
        builder.Append("Information: ");
        builder.AppendLine(summary.InformationCount.ToString(ReportCulture));
        builder.Append("With constraint violations : ");
        builder.AppendLine(
            summary.CandidateWithConstraintViolationCount
                .ToString(ReportCulture));
        builder.Append("With objective mismatch    : ");
        builder.AppendLine(
            summary.CandidateWithObjectiveMismatchCount
                .ToString(ReportCulture));
        builder.Append("Maximum constraint violation: ");
        builder.AppendLine(
            FormatDouble(summary.MaximumConstraintViolation));

        if (summary.IssueCountsByKind.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Issue categories");
            builder.AppendLine("----------------");

            foreach (SolutionCheckIssueCount issueCount in
                     summary.IssueCountsByKind
                         .OrderByDescending(item => item.Count)
                         .ThenBy(item => item.Kind))
            {
                builder.Append("  ");
                builder.Append(issueCount.Kind);
                builder.Append(" : ");
                builder.AppendLine(
                    issueCount.Count.ToString(ReportCulture));
            }
        }

        if (summary.Items.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Candidates");
            builder.AppendLine("----------");

            foreach (SolutionCheckBatchItemSummary item in summary.Items)
            {
                builder.Append("  ");
                builder.Append(
                    item.Summary.IsValid
                        ? "PASS"
                        : "FAIL");
                builder.Append("  ");
                builder.Append(item.CandidateKey);

                if (!string.IsNullOrWhiteSpace(item.CandidateName))
                {
                    builder.Append(" - ");
                    builder.Append(item.CandidateName);
                }

                builder.Append("  errors=");
                builder.Append(item.Summary.ErrorCount);
                builder.Append(" warnings=");
                builder.Append(item.Summary.WarningCount);

                if (item.Summary.ViolatedConstraintCount > 0)
                {
                    builder.Append(" violatedConstraints=");
                    builder.Append(
                        item.Summary.ViolatedConstraintCount);
                }

                if (item.Summary.ObjectiveStatus ==
                    SolutionCheckStageStatus.Failed)
                {
                    builder.Append(" objectiveMismatch=true");
                }

                builder.AppendLine();
            }
        }

        return builder
            .ToString()
            .TrimEnd();
    }

    private static void AppendStages(
        StringBuilder builder,
        SolutionCheckSummary summary)
    {
        builder.AppendLine();
        builder.AppendLine("Stages");
        builder.AppendLine("------");
        AppendStage(builder, "Structure", summary.StructuralStatus);
        AppendStage(builder, "Variable domains", summary.VariableDomainStatus);
        AppendStage(builder, "Feasibility", summary.FeasibilityStatus);
        AppendStage(builder, "Objective", summary.ObjectiveStatus);
    }

    private static void AppendStage(
        StringBuilder builder,
        string name,
        SolutionCheckStageStatus status)
    {
        builder.Append("  ");
        builder.Append(name.PadRight(18));
        builder.Append(" : ");
        builder.AppendLine(FormatStageStatus(status));
    }

    private static string FormatStageStatus(
        SolutionCheckStageStatus status)
    {
        return status switch
        {
            SolutionCheckStageStatus.NotRequested =>
                "NOT REQUESTED",
            SolutionCheckStageStatus.NotCompleted =>
                "NOT COMPLETED",
            SolutionCheckStageStatus.Passed =>
                "PASS",
            SolutionCheckStageStatus.Failed =>
                "FAIL",
            _ =>
                "UNKNOWN"
        };
    }

    private static void AppendFeasibility(
        StringBuilder builder,
        SolutionCheckSummary summary)
    {
        builder.AppendLine();
        builder.AppendLine("Feasibility metrics");
        builder.AppendLine("-------------------");
        builder.Append("  Violated constraints       : ");
        builder.AppendLine(
            summary.ViolatedConstraintCount.ToString(ReportCulture));
        builder.Append("  Maximum violation          : ");
        builder.AppendLine(
            FormatDouble(summary.MaximumConstraintViolation));
        builder.Append("  Total violation            : ");
        builder.AppendLine(
            FormatDouble(summary.TotalConstraintViolation));
    }

    private static void AppendObjective(
        StringBuilder builder,
        SolutionCheckSummary summary)
    {
        builder.AppendLine();
        builder.AppendLine("Objective metrics");
        builder.AppendLine("-----------------");
        AppendNullableDouble(
            builder,
            "Reported objective",
            summary.ReportedObjectiveValue);
        AppendNullableDouble(
            builder,
            "Recomputed objective",
            summary.RecomputedObjectiveValue);
        AppendNullableDouble(
            builder,
            "Absolute difference",
            summary.ObjectiveDifference);
        AppendNullableDouble(
            builder,
            "Relative difference",
            summary.ObjectiveRelativeDifference);
        AppendNullableDouble(
            builder,
            "Comparison tolerance",
            summary.ObjectiveComparisonTolerance);
    }

    private static void AppendNullableDouble(
        StringBuilder builder,
        string label,
        double? value)
    {
        builder.Append("  ");
        builder.Append(label.PadRight(24));
        builder.Append(" : ");
        builder.AppendLine(
            value.HasValue
                ? FormatDouble(value.Value)
                : "n/a");
    }

    private static void AppendIssues(
        StringBuilder builder,
        SolutionCheckResult result,
        SolutionCheckReportOptions options)
    {
        IEnumerable<SolutionCheckIssue> issues =
            result.Issues.Where(
                issue =>
                    ShouldIncludeIssue(
                        issue,
                        options));

        if (options.SortIssuesBySeverity)
        {
            issues =
                issues
                    .OrderByDescending(issue => issue.Severity)
                    .ThenBy(issue => issue.Kind)
                    .ThenBy(issue => issue.DomainKey ?? string.Empty)
                    .ThenBy(issue => issue.ConstraintName ?? string.Empty)
                    .ThenBy(issue => issue.Message);
        }

        List<SolutionCheckIssue> materializedIssues =
            issues.ToList();

        if (materializedIssues.Count == 0)
        {
            return;
        }

        int displayedCount =
            options.MaximumDetailedIssues == 0
                ? materializedIssues.Count
                : Math.Min(
                    options.MaximumDetailedIssues,
                    materializedIssues.Count);

        builder.AppendLine();
        builder.Append("Diagnostics (");
        builder.Append(materializedIssues.Count);
        builder.AppendLine(")");
        builder.AppendLine("-----------");

        for (int index = 0;
             index < displayedCount;
             index++)
        {
            AppendIssue(
                builder,
                materializedIssues[index]);
        }

        int omittedCount =
            materializedIssues.Count - displayedCount;

        if (omittedCount > 0)
        {
            builder.Append("  ... ");
            builder.Append(omittedCount);
            builder.AppendLine(" additional diagnostic(s) omitted.");
        }
    }

    private static bool ShouldIncludeIssue(
        SolutionCheckIssue issue,
        SolutionCheckReportOptions options)
    {
        return issue.Severity switch
        {
            SolutionCheckSeverity.Information =>
                options.IncludeInformationIssues,
            SolutionCheckSeverity.Warning =>
                options.IncludeWarningIssues,
            SolutionCheckSeverity.Error =>
                options.IncludeErrorIssues,
            _ =>
                true
        };
    }

    private static void AppendIssue(
        StringBuilder builder,
        SolutionCheckIssue issue)
    {
        builder.Append("  [");
        builder.Append(issue.Severity.ToString().ToUpperInvariant());
        builder.Append("][");
        builder.Append(issue.Kind);
        builder.Append("] ");
        builder.Append(issue.Message);

        if (!string.IsNullOrWhiteSpace(issue.DomainKey))
        {
            builder.Append(" | domain=");
            builder.Append(issue.DomainKey);
        }

        if (!string.IsNullOrWhiteSpace(issue.ConstraintName))
        {
            builder.Append(" | constraint=");
            builder.Append(issue.ConstraintName);
        }

        if (issue.ActualValue.HasValue)
        {
            builder.Append(" | actual=");
            builder.Append(FormatDouble(issue.ActualValue.Value));
        }

        if (issue.ExpectedValue.HasValue)
        {
            builder.Append(" | expected=");
            builder.Append(FormatDouble(issue.ExpectedValue.Value));
        }

        if (issue.Violation.HasValue)
        {
            builder.Append(" | violation=");
            builder.Append(FormatDouble(issue.Violation.Value));
        }

        builder.AppendLine();
    }

    private static string FormatDouble(
        double value)
    {
        return value.ToString(
            "G17",
            ReportCulture);
    }
}
