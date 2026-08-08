using System.Text.Json;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Regression;

/// <summary>
/// Locks the validated Dellaert-Jeunet Small96 campaign as a stable checker
/// regression baseline.
/// </summary>
public sealed class CampaignBaselineTests
{
    [Fact]
    public async Task CapturedSmall96Baseline_HasExpectedCorpusAndCounts()
    {
        CampaignBaselineDefinition baseline =
            await LoadBaselineDefinitionAsync();

        CampaignValidationSnapshot snapshot =
            await LoadCapturedSnapshotAsync();

        Assert.Equal(
            baseline.ExpectedOverallStatus,
            snapshot.OverallStatus);

        Assert.Equal(
            baseline.ExpectedCandidateCount,
            snapshot.CandidateCount);

        Assert.Equal(
            baseline.ExpectedValidCandidateCount,
            snapshot.ValidCandidateCount);

        Assert.Equal(
            baseline.ExpectedInvalidCandidateCount,
            snapshot.InvalidCandidateCount);

        Assert.Equal(
            baseline.ExpectedExecutionFailureCount,
            snapshot.ExecutionFailureCount);

        Assert.Equal(
            baseline.ExpectedFileLoadFailureCount,
            snapshot.FileLoadFailureCount);

        Assert.Equal(
            baseline.ExpectedCandidateCount,
            snapshot.Candidates.Count);

        string[] expectedInstances =
            baseline.ExpectedInstances
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        string[] actualInstances =
            snapshot.Candidates
                .Select(row => row.Instance)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(expectedInstances, actualInstances);
        Assert.Equal(
            actualInstances.Length,
            actualInstances.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task CapturedSmall96Baseline_AllIndependentCheckerStagesPass()
    {
        CampaignBaselineDefinition baseline =
            await LoadBaselineDefinitionAsync();

        CampaignValidationSnapshot snapshot =
            await LoadCapturedSnapshotAsync();

        foreach (CampaignValidationCandidateRow row in snapshot.Candidates)
        {
            Assert.Equal("OK", row.ExecutionStatus);
            Assert.Equal("YES", row.ValidStatus);
            Assert.Equal("Passed", row.StructuralStatus);
            Assert.Equal("Passed", row.DomainStatus);
            Assert.Equal("Passed", row.FeasibilityStatus);
            Assert.Equal("Passed", row.ObjectiveStatus);
            Assert.InRange(
                row.ViolatedConstraintCount,
                0,
                baseline.MaximumViolatedConstraintCount);
        }
    }

    [Fact]
    public async Task CapturedSmall96Baseline_ObjectiveDifferencesStayInsideNumericalEnvelope()
    {
        CampaignBaselineDefinition baseline =
            await LoadBaselineDefinitionAsync();

        CampaignValidationSnapshot snapshot =
            await LoadCapturedSnapshotAsync();

        Assert.All(
            snapshot.Candidates,
            row =>
            {
                Assert.True(
                    double.IsFinite(row.ObjectiveDifference),
                    $"Objective difference for {row.Instance} is not finite.");

                Assert.InRange(
                    Math.Abs(row.ObjectiveDifference),
                    0.0,
                    baseline.MaximumObjectiveAbsoluteDifference);
            });
    }

    private static async Task<CampaignBaselineDefinition>
        LoadBaselineDefinitionAsync()
    {
        string path =
            Path.Combine(
                AppContext.BaseDirectory,
                "Baselines",
                "DellaertJeunet.Small96.baseline.json");

        string json =
            await File.ReadAllTextAsync(path);

        CampaignBaselineDefinition? baseline =
            JsonSerializer.Deserialize<CampaignBaselineDefinition>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        return Assert.IsType<CampaignBaselineDefinition>(baseline);
    }

    private static async Task<CampaignValidationSnapshot>
        LoadCapturedSnapshotAsync()
    {
        string path =
            Path.Combine(
                AppContext.BaseDirectory,
                "Baselines",
                "DellaertJeunet.Small96.campaign-validation.txt");

        string text =
            await File.ReadAllTextAsync(path);

        return CampaignValidationReportParser.Parse(text);
    }

    private sealed class CampaignBaselineDefinition
    {
        public string ExpectedOverallStatus { get; set; } = string.Empty;

        public int ExpectedCandidateCount { get; set; }

        public int ExpectedValidCandidateCount { get; set; }

        public int ExpectedInvalidCandidateCount { get; set; }

        public int ExpectedExecutionFailureCount { get; set; }

        public int ExpectedFileLoadFailureCount { get; set; }

        public int MaximumViolatedConstraintCount { get; set; }

        public double MaximumObjectiveAbsoluteDifference { get; set; }

        public string[] ExpectedInstances { get; set; } = [];
    }
}
