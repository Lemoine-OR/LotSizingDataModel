using LotSizingDataModel.Checker.Reporting;
using LotSizingDataModel.Checker.Results;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Formats independent solution-check results for human consumption.
/// </summary>
public interface ISolutionCheckReportFormatter
{
    /// <summary>
    /// Formats one checker result.
    /// </summary>
    /// <param name="result">Checker result to format.</param>
    /// <param name="candidateName">Optional candidate display name.</param>
    /// <param name="options">Optional report formatting options.</param>
    /// <returns>A deterministic human-readable report.</returns>
    string Format(
        SolutionCheckResult result,
        string? candidateName = null,
        SolutionCheckReportOptions? options = null);

    /// <summary>
    /// Formats an aggregated batch summary.
    /// </summary>
    /// <param name="summary">Batch summary to format.</param>
    /// <param name="options">Optional report formatting options.</param>
    /// <returns>A deterministic human-readable batch report.</returns>
    string FormatBatch(
        SolutionCheckBatchSummary summary,
        SolutionCheckReportOptions? options = null);
}
