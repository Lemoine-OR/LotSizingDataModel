using System.Globalization;

namespace LotSizingDataModel.Checker.Tests.Infrastructure;

/// <summary>
/// Parses the deterministic global campaign-validation report produced by
/// <c>LotSizingDataModel.Checker.Campaign</c>.
/// </summary>
internal static class CampaignValidationReportParser
{
    /// <summary>
    /// Parses a complete campaign-validation report.
    /// </summary>
    /// <param name="text">Report text.</param>
    /// <returns>Parsed report snapshot.</returns>
    public static CampaignValidationSnapshot Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string[] lines =
            text.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Split('\n');

        string overallStatus =
            ReadSummaryValue(lines, "Overall status");

        int candidates =
            ParseInt(ReadSummaryValue(lines, "Candidates"), "Candidates");

        int valid =
            ParseInt(ReadSummaryValue(lines, "Valid"), "Valid");

        int invalid =
            ParseInt(ReadSummaryValue(lines, "Invalid"), "Invalid");

        int executionFailures =
            ParseInt(
                ReadSummaryValue(lines, "Execution failures"),
                "Execution failures");

        int fileLoadFailures =
            ParseInt(
                ReadSummaryValue(lines, "File load failures"),
                "File load failures");

        var rows =
            new List<CampaignValidationCandidateRow>();

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.Length == 0 ||
                !char.IsDigit(trimmed[0]) ||
                !trimmed.Contains('|', StringComparison.Ordinal))
            {
                continue;
            }

            string[] columns =
                trimmed.Split('|')
                    .Select(column => column.Trim())
                    .ToArray();

            if (columns.Length != 11 ||
                !int.TryParse(
                    columns[0],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int index))
            {
                continue;
            }

            if (!int.TryParse(
                    columns[9],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int violatedConstraintCount))
            {
                throw new FormatException(
                    $"Invalid violated-constraint count in campaign row {index}.");
            }

            if (!double.TryParse(
                    columns[10],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double objectiveDifference))
            {
                throw new FormatException(
                    $"Invalid objective difference in campaign row {index}.");
            }

            rows.Add(
                new CampaignValidationCandidateRow(
                    index,
                    columns[1],
                    columns[2],
                    columns[3],
                    columns[4],
                    columns[5],
                    columns[6],
                    columns[7],
                    columns[8],
                    violatedConstraintCount,
                    objectiveDifference));
        }

        return new CampaignValidationSnapshot(
            overallStatus,
            candidates,
            valid,
            invalid,
            executionFailures,
            fileLoadFailures,
            rows);
    }

    private static string ReadSummaryValue(
        IEnumerable<string> lines,
        string label)
    {
        string? line =
            lines.FirstOrDefault(
                candidate =>
                    candidate.TrimStart()
                        .StartsWith(
                            label,
                            StringComparison.OrdinalIgnoreCase));

        if (line is null)
        {
            throw new FormatException(
                $"Campaign-validation report does not contain '{label}'.");
        }

        int separatorIndex =
            line.IndexOf(':');

        if (separatorIndex < 0)
        {
            throw new FormatException(
                $"Campaign-validation summary line '{label}' has no separator.");
        }

        return line[(separatorIndex + 1)..].Trim();
    }

    private static int ParseInt(string value, string label)
    {
        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsed))
        {
            throw new FormatException(
                $"Campaign-validation summary value '{label}' is not an integer.");
        }

        return parsed;
    }
}

/// <summary>
/// Immutable snapshot of the global campaign-validation summary and rows.
/// </summary>
internal sealed record CampaignValidationSnapshot(
    string OverallStatus,
    int CandidateCount,
    int ValidCandidateCount,
    int InvalidCandidateCount,
    int ExecutionFailureCount,
    int FileLoadFailureCount,
    IReadOnlyList<CampaignValidationCandidateRow> Candidates);

/// <summary>
/// Immutable row extracted from the candidate validation matrix.
/// </summary>
internal sealed record CampaignValidationCandidateRow(
    int Index,
    string Instance,
    string KnownResult,
    string ExecutionStatus,
    string ValidStatus,
    string StructuralStatus,
    string DomainStatus,
    string FeasibilityStatus,
    string ObjectiveStatus,
    int ViolatedConstraintCount,
    double ObjectiveDifference);
