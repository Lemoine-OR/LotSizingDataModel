using LotSizingDataModel.Checker.Campaign.Benchmark;
using LotSizingDataModel.Instance.Benchmark;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Historical;
using LotSizingDataModel.Instance.Results;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Regression;

public sealed class BenchmarkCampaignConsolidationTests
{
    [Fact]
    public void BestKnownSelection_RejectsDisputedBetterValue()
    {
        var disputed =
            new KnownResult(
                "disputed",
                90.0)
            {
                VerificationStatus =
                    KnownResultVerificationStatus.Disputed,

                SourceReference =
                    "published source"
            };

        var verified =
            new KnownResult(
                "verified",
                100.0)
            {
                VerificationStatus =
                    KnownResultVerificationStatus.IndependentlyVerified,

                SourceReference =
                    "independent verification"
            };

        BenchmarkBestKnownResultSelection selection =
            new BenchmarkBestKnownResultSelectionService()
                .Select(
                    new[]
                    {
                        disputed,
                        verified
                    },
                    BenchmarkObjectiveDirection.Minimize);

        Assert.NotNull(
            selection.SelectedResult);

        Assert.Equal(
            "verified",
            selection.SelectedResult.ResultId);

        BenchmarkKnownResultAuditRecord disputedAudit =
            selection.Audits.Single(
                audit =>
                    audit.ResultId ==
                    "disputed");

        Assert.False(
            disputedAudit.IsReferenceEligible);

        Assert.Contains(
            "BKS-DISPUTED",
            disputedAudit.Diagnostics);
    }

    [Fact]
    public void StochasticRun_RequiresExplicitSeed()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new BenchmarkRunProvenance(
                    "standard",
                    "ga",
                    "1.0",
                    "native",
                    "1.0",
                    isStochastic:
                        true,
                    seed:
                        null));
    }

    [Fact]
    public void DatasetLayout_SeparatesRawAnnotatedSolutionsAndCampaigns()
    {
        Assert.Equal(
            "benchmarks/raw/jeunet/i01.txt",
            BenchmarkDatasetLayout.CreateRelativePath(
                BenchmarkDatasetArtifactKind.RawInstance,
                "jeunet",
                "i01.txt"));

        Assert.Equal(
            "benchmarks/annotated/jeunet/i01.xml",
            BenchmarkDatasetLayout.CreateRelativePath(
                BenchmarkDatasetArtifactKind.AnnotatedInstance,
                "jeunet",
                "i01.xml"));

        Assert.Equal(
            "benchmarks/solutions/jeunet/i01.solution.xml",
            BenchmarkDatasetLayout.CreateRelativePath(
                BenchmarkDatasetArtifactKind.Solution,
                "jeunet",
                "i01.solution.xml"));

        Assert.Equal(
            "benchmarks/campaigns/jeunet/c01.json",
            BenchmarkDatasetLayout.CreateRelativePath(
                BenchmarkDatasetArtifactKind.CampaignReport,
                "jeunet",
                "c01.json"));

        Assert.Throws<InvalidOperationException>(
            () =>
                BenchmarkDatasetLayout
                    .EnsureCanonicalRelativePath(
                        "../outside/file.xml"));
    }

    [Fact]
    public void CampaignWriter_ProducesDeterministicJsonCsvAndHashes()
    {
        var provenance =
            new BenchmarkRunProvenance(
                "standard",
                "wagner-whitin-classical",
                "1.1.0",
                "ULSAlgorithms",
                "1.1.0",
                isStochastic:
                    false,
                seed:
                    null,
                parameters:
                    new Dictionary<string, string>
                    {
                        ["zeta"] = "2",
                        ["alpha"] = "1"
                    });

        var bks =
            new KnownResult(
                "bks",
                100.0)
            {
                VerificationStatus =
                    KnownResultVerificationStatus.AutomaticallyVerified,

                SourceReference =
                    "checker campaign"
            };

        HistoricalMappingAuditResult historical =
            new HistoricalMappingAuditService()
                .Audit(
                    HistoricalClassificationFamily.Wolsey,
                    declaredTokens:
                        new[]
                        {
                            "CAP=U"
                        },
                    detectedTokens:
                        new[]
                        {
                            "CAP=U"
                        });

        BenchmarkCampaignRunRecord run =
            new BenchmarkCampaignRunRecordFactory()
                .Create(
                    "instance-01",
                    "sha256:abc",
                    provenance,
                    objectiveValue:
                        110.0,
                    hasFeasibleSolution:
                        true,
                    isOptimal:
                        false,
                    elapsed:
                        TimeSpan.FromMilliseconds(
                            12.5),
                    direction:
                        BenchmarkObjectiveDirection.Minimize,
                    bks:
                        bks,
                    historicalAudit:
                        new BenchmarkHistoricalAuditSnapshot(
                            historical));

        var report =
            new BenchmarkCampaignReport
            {
                CampaignId =
                    "campaign-a",

                GeneratedAtUtc =
                    new DateTime(
                        2026,
                        9,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc),

                Runs =
                    new[]
                    {
                        run
                    }
            };

        string directory =
            Path.Combine(
                Path.GetTempPath(),
                "lsdm-alpha42-" +
                Guid.NewGuid().ToString("N"));

        try
        {
            BenchmarkCampaignReportFiles files =
                new BenchmarkCampaignReportWriter()
                    .Write(
                        report,
                        directory);

            Assert.True(
                File.Exists(
                    files.JsonPath));

            Assert.True(
                File.Exists(
                    files.CsvPath));

            Assert.True(
                File.Exists(
                    files.Sha256Path));

            string csv =
                File.ReadAllText(
                    files.CsvPath);

            Assert.Contains(
                "alpha=1;zeta=2",
                csv,
                StringComparison.Ordinal);

            Assert.Contains(
                "0.1",
                csv,
                StringComparison.Ordinal);

            string manifest =
                File.ReadAllText(
                    files.Sha256Path);

            Assert.Contains(
                "campaign-a.benchmark.json",
                manifest,
                StringComparison.Ordinal);

            Assert.Contains(
                "campaign-a.benchmark.csv",
                manifest,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(
                    directory))
            {
                Directory.Delete(
                    directory,
                    recursive:
                        true);
            }
        }
    }

    [Fact]
    public void HistoricalAuditSnapshot_PreservesDeclaredDetectedDifference()
    {
        HistoricalMappingAuditResult audit =
            new HistoricalMappingAuditService()
                .Audit(
                    HistoricalClassificationFamily.Wolsey,
                    declaredTokens:
                        new[]
                        {
                            "CAP=U",
                            "IM"
                        },
                    detectedTokens:
                        new[]
                        {
                            "CAP=U",
                            "VAR=B"
                        });

        var snapshot =
            new BenchmarkHistoricalAuditSnapshot(
                audit);

        Assert.False(
            snapshot.IsExactMatch);

        Assert.Equal(
            "IM",
            Assert.Single(
                snapshot.DeclaredButNotDetected));

        Assert.Equal(
            "VAR=B",
            Assert.Single(
                snapshot.DetectedButNotDeclared));
    }
}
