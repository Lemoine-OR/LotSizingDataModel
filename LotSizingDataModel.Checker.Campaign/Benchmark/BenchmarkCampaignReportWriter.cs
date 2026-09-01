using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LotSizingDataModel.Checker.Campaign.Benchmark;

/// <summary>
/// Writes deterministic JSON and CSV campaign artifacts plus a
/// SHA-256 manifest.
/// </summary>
public sealed class BenchmarkCampaignReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented =
                true,
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase
        };

    public BenchmarkCampaignReportFiles Write(
        BenchmarkCampaignReport report,
        string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(
            report);

        report.EnsureValid();

        if (string.IsNullOrWhiteSpace(
                outputDirectory))
        {
            throw new ArgumentException(
                "An output directory is required.",
                nameof(outputDirectory));
        }

        Directory.CreateDirectory(
            outputDirectory);

        string safeCampaignId =
            SanitizeFileName(
                report.CampaignId);

        string jsonPath =
            Path.Combine(
                outputDirectory,
                safeCampaignId +
                ".benchmark.json");

        string csvPath =
            Path.Combine(
                outputDirectory,
                safeCampaignId +
                ".benchmark.csv");

        string shaPath =
            Path.Combine(
                outputDirectory,
                safeCampaignId +
                ".benchmark.sha256");

        BenchmarkCampaignRunRecord[] orderedRuns =
            report.Runs
                .OrderBy(
                    run =>
                        run.InstanceId,
                    StringComparer.Ordinal)
                .ThenBy(
                    run =>
                        run.Provenance.MethodId,
                    StringComparer.Ordinal)
                .ThenBy(
                    run =>
                        run.Provenance.Seed)
                .ToArray();

        var normalizedReport =
            new BenchmarkCampaignReport
            {
                CampaignId =
                    report.CampaignId,

                GeneratedAtUtc =
                    report.GeneratedAtUtc,

                Runs =
                    orderedRuns
            };

        string json =
            JsonSerializer.Serialize(
                normalizedReport,
                JsonOptions);

        File.WriteAllText(
            jsonPath,
            NormalizeLf(
                json) +
            "\n",
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier:
                    false));

        File.WriteAllText(
            csvPath,
            BuildCsv(
                orderedRuns),
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier:
                    false));

        string manifest =
            BuildManifest(
                jsonPath,
                csvPath);

        File.WriteAllText(
            shaPath,
            manifest,
            Encoding.ASCII);

        return new BenchmarkCampaignReportFiles(
            jsonPath,
            csvPath,
            shaPath);
    }

    private static string BuildCsv(
        IReadOnlyList<BenchmarkCampaignRunRecord> runs)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine(
            "instanceId,instanceFingerprint,formulationId,methodId,methodVersion,backendId,backendVersion,isStochastic,seed,parameters,objectiveValue,hasFeasibleSolution,isOptimal,elapsedMilliseconds,bksResultId,bksObjectiveValue,bksVerificationStatus,relativeGapToBks,historicalFamily,historicalExactMatch,declaredButNotDetected,detectedButNotDeclared");

        foreach (BenchmarkCampaignRunRecord run
                 in runs)
        {
            string parameters =
                string.Join(
                    ";",
                    run.Provenance.Parameters
                        .Select(
                            pair =>
                                pair.Key +
                                "=" +
                                pair.Value));

            string declaredOnly =
                run.HistoricalAudit is null
                    ? string.Empty
                    : string.Join(
                        ";",
                        run.HistoricalAudit
                            .DeclaredButNotDetected);

            string detectedOnly =
                run.HistoricalAudit is null
                    ? string.Empty
                    : string.Join(
                        ";",
                        run.HistoricalAudit
                            .DetectedButNotDeclared);

            string[] values =
            [
                run.InstanceId,
                run.InstanceFingerprint,
                run.Provenance.FormulationId,
                run.Provenance.MethodId,
                run.Provenance.MethodVersion,
                run.Provenance.BackendId,
                run.Provenance.BackendVersion,
                run.Provenance.IsStochastic
                    ? "true"
                    : "false",
                run.Provenance.Seed?.ToString(
                    CultureInfo.InvariantCulture) ??
                    string.Empty,
                parameters,
                FormatNullableDouble(
                    run.ObjectiveValue),
                run.HasFeasibleSolution
                    ? "true"
                    : "false",
                run.IsOptimal
                    ? "true"
                    : "false",
                run.ElapsedMilliseconds.ToString(
                    "R",
                    CultureInfo.InvariantCulture),
                run.BksResultId,
                FormatNullableDouble(
                    run.BksObjectiveValue),
                run.BksVerificationStatus?.ToString() ??
                    string.Empty,
                FormatNullableDouble(
                    run.RelativeGapToBks),
                run.HistoricalAudit?.Family ??
                    string.Empty,
                run.HistoricalAudit is null
                    ? string.Empty
                    : (
                        run.HistoricalAudit.IsExactMatch
                            ? "true"
                            : "false"
                    ),
                declaredOnly,
                detectedOnly
            ];

            builder.AppendLine(
                string.Join(
                    ",",
                    values.Select(
                        EscapeCsv)));
        }

        return NormalizeLf(
                   builder.ToString())
               .TrimEnd('\n') +
               "\n";
    }

    private static string BuildManifest(
        params string[] paths)
    {
        var lines =
            new List<string>();

        foreach (string path
                 in paths.OrderBy(
                     value =>
                         Path.GetFileName(value),
                     StringComparer.Ordinal))
        {
            string hash =
                Convert.ToHexString(
                        SHA256.HashData(
                            File.ReadAllBytes(
                                path)))
                    .ToLowerInvariant();

            lines.Add(
                hash +
                "  " +
                Path.GetFileName(
                    path));
        }

        return string.Join(
                   "\n",
                   lines) +
               "\n";
    }

    private static string FormatNullableDouble(
        double? value)
    {
        return value?.ToString(
                   "R",
                   CultureInfo.InvariantCulture) ??
               string.Empty;
    }

    private static string EscapeCsv(
        string? value)
    {
        string normalized =
            value ??
            string.Empty;

        if (!normalized.Contains(',') &&
            !normalized.Contains('"') &&
            !normalized.Contains('\n') &&
            !normalized.Contains('\r'))
        {
            return normalized;
        }

        return "\"" +
               normalized.Replace(
                   "\"",
                   "\"\"",
                   StringComparison.Ordinal) +
               "\"";
    }

    private static string SanitizeFileName(
        string value)
    {
        char[] invalid =
            Path.GetInvalidFileNameChars();

        string normalized =
            new(
                value
                    .Select(
                        character =>
                            invalid.Contains(
                                character)
                                ? '_'
                                : character)
                    .ToArray());

        return normalized.Length == 0
            ? "benchmark-campaign"
            : normalized;
    }

    private static string NormalizeLf(
        string value)
    {
        return value
            .Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
            .Replace(
                "\r",
                "\n",
                StringComparison.Ordinal);
    }
}
