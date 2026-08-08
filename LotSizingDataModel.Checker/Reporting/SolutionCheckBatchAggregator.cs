using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Results;

namespace LotSizingDataModel.Checker.Reporting;

/// <summary>
/// Aggregates independent checker results for batch-validation campaigns.
/// </summary>
public sealed class SolutionCheckBatchAggregator
{
    private readonly List<SolutionCheckBatchItemSummary> _items = new();

    /// <summary>
    /// Adds one checked candidate to the batch.
    /// </summary>
    /// <param name="candidateKey">Stable caller-defined candidate identifier.</param>
    /// <param name="result">Checker result for the candidate.</param>
    /// <param name="candidateName">Optional display name.</param>
    public void Add(
        string candidateKey,
        SolutionCheckResult result,
        string? candidateName = null)
    {
        if (string.IsNullOrWhiteSpace(candidateKey))
        {
            throw new ArgumentException(
                "The candidate key cannot be empty.",
                nameof(candidateKey));
        }

        ArgumentNullException.ThrowIfNull(result);

        _items.Add(
            new SolutionCheckBatchItemSummary
            {
                CandidateKey =
                    candidateKey,
                CandidateName =
                    candidateName,
                Summary =
                    SolutionCheckSummaryFactory.Create(result)
            });
    }

    /// <summary>
    /// Removes all accumulated candidate summaries.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Builds a detached aggregate summary from all accumulated candidates.
    /// </summary>
    /// <returns>A new batch summary.</returns>
    public SolutionCheckBatchSummary BuildSummary()
    {
        var summary =
            new SolutionCheckBatchSummary
            {
                CandidateCount =
                    _items.Count,
                ValidCandidateCount =
                    _items.Count(
                        item =>
                            item.Summary.IsValid),
                InvalidCandidateCount =
                    _items.Count(
                        item =>
                            !item.Summary.IsValid),
                InformationCount =
                    _items.Sum(
                        item =>
                            item.Summary.InformationCount),
                WarningCount =
                    _items.Sum(
                        item =>
                            item.Summary.WarningCount),
                ErrorCount =
                    _items.Sum(
                        item =>
                            item.Summary.ErrorCount),
                CandidateWithConstraintViolationCount =
                    _items.Count(
                        item =>
                            item.Summary.ViolatedConstraintCount > 0),
                CandidateWithObjectiveMismatchCount =
                    _items.Count(
                        item =>
                            item.Summary.IssueCountsByKind.Any(
                                issueCount =>
                                    issueCount.Kind ==
                                    SolutionCheckIssueKind.ObjectiveMismatch &&
                                    issueCount.Count > 0)),
                MaximumConstraintViolation =
                    _items.Count == 0
                        ? 0.0
                        : _items.Max(
                            item =>
                                item.Summary.MaximumConstraintViolation)
            };

        foreach (SolutionCheckIssueKind kind in
                 Enum.GetValues<SolutionCheckIssueKind>())
        {
            int count =
                _items.Sum(
                    item =>
                        item.Summary.IssueCountsByKind
                            .Where(issueCount =>
                                issueCount.Kind == kind)
                            .Sum(issueCount =>
                                issueCount.Count));

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

        foreach (SolutionCheckBatchItemSummary item in _items)
        {
            summary.Items.Add(
                CloneItem(item));
        }

        return summary;
    }

    private static SolutionCheckBatchItemSummary CloneItem(
        SolutionCheckBatchItemSummary source)
    {
        return new SolutionCheckBatchItemSummary
        {
            CandidateKey =
                source.CandidateKey,
            CandidateName =
                source.CandidateName,
            Summary =
                CloneSummary(source.Summary)
        };
    }

    private static SolutionCheckSummary CloneSummary(
        SolutionCheckSummary source)
    {
        var clone =
            new SolutionCheckSummary
            {
                Level =
                    source.Level,
                IsValid =
                    source.IsValid,
                StructuralStatus =
                    source.StructuralStatus,
                VariableDomainStatus =
                    source.VariableDomainStatus,
                FeasibilityStatus =
                    source.FeasibilityStatus,
                ObjectiveStatus =
                    source.ObjectiveStatus,
                IssueCount =
                    source.IssueCount,
                InformationCount =
                    source.InformationCount,
                WarningCount =
                    source.WarningCount,
                ErrorCount =
                    source.ErrorCount,
                ViolatedConstraintCount =
                    source.ViolatedConstraintCount,
                MaximumConstraintViolation =
                    source.MaximumConstraintViolation,
                TotalConstraintViolation =
                    source.TotalConstraintViolation,
                ReportedObjectiveValue =
                    source.ReportedObjectiveValue,
                RecomputedObjectiveValue =
                    source.RecomputedObjectiveValue,
                ObjectiveDifference =
                    source.ObjectiveDifference,
                ObjectiveRelativeDifference =
                    source.ObjectiveRelativeDifference,
                ObjectiveComparisonTolerance =
                    source.ObjectiveComparisonTolerance
            };

        foreach (SolutionCheckIssueCount issueCount in
                 source.IssueCountsByKind)
        {
            clone.IssueCountsByKind.Add(
                new SolutionCheckIssueCount
                {
                    Kind =
                        issueCount.Kind,
                    Count =
                        issueCount.Count
                });
        }

        return clone;
    }
}
