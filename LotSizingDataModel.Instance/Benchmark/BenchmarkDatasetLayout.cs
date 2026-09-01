namespace LotSizingDataModel.Instance.Benchmark;

/// <summary>
/// Canonical repository layout for benchmark artifacts.
/// </summary>
public static class BenchmarkDatasetLayout
{
    public const string RootDirectory =
        "benchmarks";

    public const string RawDirectory =
        "raw";

    public const string AnnotatedDirectory =
        "annotated";

    public const string SolutionsDirectory =
        "solutions";

    public const string CampaignsDirectory =
        "campaigns";

    public static string GetDirectory(
        BenchmarkDatasetArtifactKind kind)
    {
        return kind switch
        {
            BenchmarkDatasetArtifactKind.RawInstance =>
                $"{RootDirectory}/{RawDirectory}",

            BenchmarkDatasetArtifactKind.AnnotatedInstance =>
                $"{RootDirectory}/{AnnotatedDirectory}",

            BenchmarkDatasetArtifactKind.Solution =>
                $"{RootDirectory}/{SolutionsDirectory}",

            BenchmarkDatasetArtifactKind.CampaignReport =>
                $"{RootDirectory}/{CampaignsDirectory}",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    kind,
                    "Unknown benchmark artifact kind.")
        };
    }

    public static string CreateRelativePath(
        BenchmarkDatasetArtifactKind kind,
        string family,
        string fileName)
    {
        string normalizedFamily =
            NormalizeSegment(
                family,
                nameof(family));

        string normalizedFile =
            NormalizeSegment(
                fileName,
                nameof(fileName));

        return
            $"{GetDirectory(kind)}/{normalizedFamily}/{normalizedFile}";
    }

    public static void EnsureCanonicalRelativePath(
        string relativePath)
    {
        if (string.IsNullOrWhiteSpace(
                relativePath))
        {
            throw new ArgumentException(
                "A benchmark relative path is required.",
                nameof(relativePath));
        }

        string normalized =
            relativePath
                .Replace('\\', '/')
                .Trim();

        if (normalized.StartsWith(
                "/",
                StringComparison.Ordinal) ||
            normalized.Contains(
                "../",
                StringComparison.Ordinal) ||
            normalized.Contains(
                "/..",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Benchmark artifact paths must remain relative and cannot traverse directories.");
        }

        string[] allowedPrefixes =
        [
            $"{RootDirectory}/{RawDirectory}/",
            $"{RootDirectory}/{AnnotatedDirectory}/",
            $"{RootDirectory}/{SolutionsDirectory}/",
            $"{RootDirectory}/{CampaignsDirectory}/"
        ];

        if (!allowedPrefixes.Any(
                prefix =>
                    normalized.StartsWith(
                        prefix,
                        StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Benchmark path '{normalized}' is outside the canonical raw/annotated/solutions/campaigns layout.");
        }
    }

    private static string NormalizeSegment(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            throw new ArgumentException(
                "A non-empty benchmark path segment is required.",
                parameterName);
        }

        string normalized =
            value.Trim();

        if (normalized.Contains('/') ||
            normalized.Contains('\\') ||
            normalized is "." or "..")
        {
            throw new ArgumentException(
                "Benchmark path segments cannot contain directory separators or traversal tokens.",
                parameterName);
        }

        return normalized;
    }
}
