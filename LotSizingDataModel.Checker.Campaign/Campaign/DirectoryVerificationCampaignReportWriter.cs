using System.Globalization;
using System.Text;
using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Reporting;

namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Writes deterministic human-readable and tab-separated reports for a
/// completed directory verification campaign.
/// </summary>
public sealed class DirectoryVerificationCampaignReportWriter
{
    private static readonly CultureInfo ReportCulture =
        CultureInfo.InvariantCulture;

    private static readonly Encoding ReportEncoding =
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false);

    private readonly SolutionCheckTextReportFormatter _formatter;

    /// <summary>
    /// Initializes the writer with the standard checker report formatter.
    /// </summary>
    public DirectoryVerificationCampaignReportWriter()
        : this(
            new SolutionCheckTextReportFormatter())
    {
    }

    /// <summary>
    /// Initializes the writer with an explicit checker report formatter.
    /// </summary>
    /// <param name="formatter">Formatter used for checker result sections.</param>
    public DirectoryVerificationCampaignReportWriter(
        SolutionCheckTextReportFormatter formatter)
    {
        _formatter =
            formatter ??
            throw new ArgumentNullException(nameof(formatter));
    }

    /// <summary>
    /// Writes the global summary, tab-separated manifest and individual
    /// candidate reports.
    /// </summary>
    /// <param name="result">Completed campaign result.</param>
    /// <param name="options">Campaign reporting policy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paths of the files written by this operation.</returns>
    public async Task<DirectoryVerificationCampaignReportFiles> WriteAsync(
        DirectoryVerificationCampaignResult result,
        DirectoryVerificationCampaignOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(result.OutputDirectory);

        string candidateDirectory =
            Path.Combine(
                result.OutputDirectory,
                "candidates");

        Directory.CreateDirectory(candidateDirectory);

        string summaryPath =
            Path.Combine(
                result.OutputDirectory,
                "campaign-summary.txt");

        string manifestPath =
            Path.Combine(
                result.OutputDirectory,
                "campaign-items.tsv");

        string globalValidationPath =
            Path.Combine(
                result.OutputDirectory,
                "campaign-validation.txt");

        await WriteTextAsync(
                summaryPath,
                FormatCampaignSummary(
                    result,
                    options.ReportOptions),
                options.OverwriteExistingReports,
                cancellationToken)
            .ConfigureAwait(false);

        await WriteTextAsync(
                manifestPath,
                FormatManifest(result),
                options.OverwriteExistingReports,
                cancellationToken)
            .ConfigureAwait(false);

        await WriteTextAsync(
                globalValidationPath,
                FormatGlobalValidationReport(result),
                options.OverwriteExistingReports,
                cancellationToken)
            .ConfigureAwait(false);

        var candidateReportPaths =
            new List<string>(result.Items.Count);

        for (int index = 0;
             index < result.Items.Count;
             index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DirectoryVerificationCampaignItem item =
                result.Items[index];

            string candidateReportPath =
                Path.Combine(
                    candidateDirectory,
                    $"candidate-{index + 1:D6}.txt");

            await WriteTextAsync(
                    candidateReportPath,
                    FormatCandidateReport(
                        item,
                        options.ReportOptions),
                    options.OverwriteExistingReports,
                    cancellationToken)
                .ConfigureAwait(false);

            candidateReportPaths.Add(candidateReportPath);
        }

        return new DirectoryVerificationCampaignReportFiles
        {
            SummaryReportPath =
                summaryPath,
            ManifestPath =
                manifestPath,
            GlobalValidationReportPath =
                globalValidationPath,
            CandidateReportPaths =
                candidateReportPaths.AsReadOnly()
        };
    }

    private string FormatCampaignSummary(
        DirectoryVerificationCampaignResult result,
        SolutionCheckReportOptions reportOptions)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine("Lot-sizing directory verification campaign");
        builder.AppendLine(new string('=', 60));
        builder.Append("Input directory  : ");
        builder.AppendLine(result.InputDirectory);
        builder.Append("Output directory : ");
        builder.AppendLine(result.OutputDirectory);
        builder.Append("Campaign result  : ");
        builder.AppendLine(
            result.IsValid
                ? "VALID"
                : "INVALID");
        builder.AppendLine();

        builder.AppendLine("Input discovery");
        builder.AppendLine("---------------");
        AppendCount(
            builder,
            "Matching XML files",
            result.DiscoveredXmlFileCount);
        AppendCount(
            builder,
            "Loaded instances",
            result.LoadedInstanceCount);
        AppendCount(
            builder,
            "Ignored non-instance XML",
            result.IgnoredNonInstanceXmlFileCount);
        AppendCount(
            builder,
            "File load failures",
            result.FileLoadFailureCount);
        AppendCount(
            builder,
            "Known results",
            result.KnownResultCount);
        AppendCount(
            builder,
            "Predicate filtered",
            result.PredicateFilteredKnownResultCount);
        AppendCount(
            builder,
            "Without detailed solution",
            result.KnownResultWithoutDetailedSolutionCount);
        AppendCount(
            builder,
            "Verification candidates",
            result.CandidateCount);

        builder.AppendLine();
        builder.AppendLine("Batch execution");
        builder.AppendLine("---------------");
        AppendCount(
            builder,
            "Completed candidates",
            result.BatchResult.CompletedCandidateCount);
        AppendCount(
            builder,
            "Execution failures",
            result.BatchResult.ExecutionFailureCount);
        AppendCount(
            builder,
            "Valid candidates",
            result.BatchResult.ValidCandidateCount);
        AppendCount(
            builder,
            "Invalid candidates",
            result.BatchResult.InvalidCandidateCount);

        if (result.FileLoadFailures.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("File load failures");
            builder.AppendLine("------------------");

            foreach (InstanceFileLoadFailure failure in
                     result.FileLoadFailures)
            {
                builder.Append("  ");
                builder.Append(failure.RelativeFilePath);
                builder.Append(" | ");
                builder.Append(failure.ExceptionType);
                builder.Append(" | ");
                builder.AppendLine(
                    NormalizeSingleLine(failure.Message));
            }
        }

        if (result.BatchResult.ExecutionFailureCount > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Candidate execution failures");
            builder.AppendLine("----------------------------");

            foreach (DirectoryVerificationCampaignItem item in
                     result.Items.Where(
                         item =>
                             !item.Verification.ExecutionSucceeded))
            {
                builder.Append("  ");
                builder.Append(item.Source.CandidateKey);
                builder.Append(" | ");
                builder.Append(
                    item.Verification.FailureExceptionType ??
                    "unknown exception");
                builder.Append(" | ");
                builder.AppendLine(
                    NormalizeSingleLine(
                        item.Verification.FailureMessage ??
                        string.Empty));
            }
        }

        builder.AppendLine();
        builder.AppendLine(
            _formatter.FormatBatch(
                result.BatchResult.CheckSummary,
                reportOptions));

        return builder
            .ToString()
            .TrimEnd();
    }

    private string FormatCandidateReport(
        DirectoryVerificationCampaignItem item,
        SolutionCheckReportOptions reportOptions)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine("Directory campaign candidate");
        builder.AppendLine(new string('=', 60));
        builder.Append("Candidate key : ");
        builder.AppendLine(item.Source.CandidateKey);
        builder.Append("Source file   : ");
        builder.AppendLine(item.Source.RelativeSourceFilePath);
        builder.Append("Instance ID   : ");
        builder.AppendLine(item.Source.InstanceId);
        builder.Append("KnownResult ID: ");
        builder.AppendLine(item.Source.KnownResultId);

        if (!string.IsNullOrWhiteSpace(item.Source.KnownResultName))
        {
            builder.Append("KnownResult   : ");
            builder.AppendLine(item.Source.KnownResultName);
        }

        builder.AppendLine();

        if (!item.Verification.ExecutionSucceeded)
        {
            builder.AppendLine("Verification execution failed");
            builder.AppendLine("-----------------------------");
            builder.Append("Exception: ");
            builder.AppendLine(
                item.Verification.FailureExceptionType ??
                "unknown exception");
            builder.Append("Message  : ");
            builder.AppendLine(
                item.Verification.FailureMessage ??
                string.Empty);

            return builder
                .ToString()
                .TrimEnd();
        }

        if (item.Verification.VerificationResult is null)
        {
            builder.AppendLine(
                "Verification completed without a detailed result.");

            return builder
                .ToString()
                .TrimEnd();
        }

        builder.AppendLine(
            _formatter.Format(
                item.Verification.VerificationResult.CheckResult,
                BuildCandidateDisplayName(item.Source),
                reportOptions));

        return builder
            .ToString()
            .TrimEnd();
    }

    private static string FormatGlobalValidationReport(
        DirectoryVerificationCampaignResult result)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine("Lot-sizing global solution validation report");
        builder.AppendLine(new string('=', 72));
        builder.Append("Overall status      : ");
        builder.AppendLine(result.IsValid ? "VALID" : "INVALID");
        AppendCount(builder, "Candidates", result.CandidateCount);
        AppendCount(builder, "Valid", result.BatchResult.ValidCandidateCount);
        AppendCount(builder, "Invalid", result.BatchResult.InvalidCandidateCount);
        AppendCount(builder, "Execution failures", result.BatchResult.ExecutionFailureCount);
        AppendCount(builder, "File load failures", result.FileLoadFailureCount);

        builder.AppendLine();
        builder.AppendLine("Candidate validation matrix");
        builder.AppendLine("---------------------------");
        builder.AppendLine(
            "# | Instance | KnownResult | Exec | Valid | Structure | Domain | Feasibility | Objective | Viol. | Obj diff");

        foreach (DirectoryVerificationCampaignItem item in result.Items)
        {
            SolutionVerificationBatchItemResult verification =
                item.Verification;

            SolutionCheckSummary? summary =
                verification.Summary;

            builder.Append((verification.Index + 1).ToString(ReportCulture));
            builder.Append(" | ");
            builder.Append(item.Source.InstanceId);
            builder.Append(" | ");
            builder.Append(item.Source.KnownResultId);
            builder.Append(" | ");
            builder.Append(verification.ExecutionSucceeded ? "OK" : "FAIL");
            builder.Append(" | ");
            builder.Append(verification.IsValid ? "YES" : "NO");
            builder.Append(" | ");
            builder.Append(summary?.StructuralStatus.ToString() ?? "-");
            builder.Append(" | ");
            builder.Append(summary?.VariableDomainStatus.ToString() ?? "-");
            builder.Append(" | ");
            builder.Append(summary?.FeasibilityStatus.ToString() ?? "-");
            builder.Append(" | ");
            builder.Append(summary?.ObjectiveStatus.ToString() ?? "-");
            builder.Append(" | ");
            builder.Append(
                summary?.ViolatedConstraintCount.ToString(ReportCulture) ?? "-");
            builder.Append(" | ");
            builder.AppendLine(FormatNullableDouble(summary?.ObjectiveDifference));
        }

        var issueCounts =
            result.Items
                .Where(item => item.Verification.Summary is not null)
                .SelectMany(item => item.Verification.Summary!.IssueCountsByKind)
                .GroupBy(count => count.Kind)
                .Select(
                    group => new
                    {
                        Kind = group.Key,
                        Count = group.Sum(value => value.Count)
                    })
                .Where(value => value.Count > 0)
                .OrderBy(value => value.Kind.ToString(), StringComparer.Ordinal)
                .ToArray();

        if (issueCounts.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Aggregated diagnostics by kind");
            builder.AppendLine("------------------------------");

            foreach (var issueCount in issueCounts)
            {
                builder.Append("  ");
                builder.Append(issueCount.Kind);
                builder.Append(" : ");
                builder.AppendLine(issueCount.Count.ToString(ReportCulture));
            }
        }

        return builder
            .ToString()
            .TrimEnd();
    }

    private static string FormatManifest(
        DirectoryVerificationCampaignResult result)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine(
            "index\tcandidate_key\tsource_file\tinstance_id\tknown_result_id" +
            "\texecution\tvalidation\terrors\twarnings\tviolated_constraints" +
            "\treported_objective\trecomputed_objective\tfailure_type" +
            "\tfailure_message");

        foreach (DirectoryVerificationCampaignItem item in result.Items)
        {
            SolutionVerificationBatchItemResult verification =
                item.Verification;

            SolutionCheckSummary? summary =
                verification.Summary;

            AppendTsv(builder, verification.Index.ToString(ReportCulture));
            AppendTsv(builder, item.Source.CandidateKey);
            AppendTsv(builder, item.Source.RelativeSourceFilePath);
            AppendTsv(builder, item.Source.InstanceId);
            AppendTsv(builder, item.Source.KnownResultId);
            AppendTsv(
                builder,
                verification.ExecutionSucceeded
                    ? "completed"
                    : "failed");
            AppendTsv(
                builder,
                verification.ExecutionSucceeded
                    ? verification.IsValid
                        ? "valid"
                        : "invalid"
                    : string.Empty);
            AppendTsv(
                builder,
                summary?.ErrorCount.ToString(ReportCulture) ??
                string.Empty);
            AppendTsv(
                builder,
                summary?.WarningCount.ToString(ReportCulture) ??
                string.Empty);
            AppendTsv(
                builder,
                summary?.ViolatedConstraintCount.ToString(ReportCulture) ??
                string.Empty);
            AppendTsv(
                builder,
                FormatNullableDouble(summary?.ReportedObjectiveValue));
            AppendTsv(
                builder,
                FormatNullableDouble(summary?.RecomputedObjectiveValue));
            AppendTsv(
                builder,
                verification.FailureExceptionType ??
                string.Empty);
            AppendTsv(
                builder,
                verification.FailureMessage ??
                string.Empty,
                endOfLine: true);
        }

        return builder
            .ToString()
            .TrimEnd();
    }

    private static void AppendTsv(
        StringBuilder builder,
        string value,
        bool endOfLine = false)
    {
        builder.Append(
            NormalizeSingleLine(value)
                .Replace('\t', ' '));

        if (endOfLine)
        {
            builder.AppendLine();
        }
        else
        {
            builder.Append('\t');
        }
    }

    private static void AppendCount(
        StringBuilder builder,
        string label,
        int value)
    {
        builder.Append("  ");
        builder.Append(label.PadRight(28));
        builder.Append(" : ");
        builder.AppendLine(value.ToString(ReportCulture));
    }

    private static string BuildCandidateDisplayName(
        DirectoryVerificationCandidateSource source)
    {
        string knownResultLabel =
            string.IsNullOrWhiteSpace(source.KnownResultName)
                ? source.KnownResultId
                : source.KnownResultName;

        return
            source.InstanceId +
            " / " +
            knownResultLabel;
    }

    private static string FormatNullableDouble(
        double? value)
    {
        return value.HasValue
            ? value.Value.ToString("G17", ReportCulture)
            : string.Empty;
    }

    private static string NormalizeSingleLine(
        string value)
    {
        return value
            .Replace('\r', ' ')
            .Replace('\n', ' ');
    }

    private static async Task WriteTextAsync(
        string path,
        string content,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        FileMode mode =
            overwrite
                ? FileMode.Create
                : FileMode.CreateNew;

        await using var stream =
            new FileStream(
                path,
                mode,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 64 * 1024,
                useAsync: true);

        await using var writer =
            new StreamWriter(
                stream,
                ReportEncoding);

        await writer.WriteAsync(
                content.AsMemory(),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
