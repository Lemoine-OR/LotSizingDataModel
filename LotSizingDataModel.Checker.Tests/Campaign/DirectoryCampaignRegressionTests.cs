using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Campaign;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Campaign;

/// <summary>
/// Regression tests for directory campaigns over several serialized
/// lot-sizing results.
/// </summary>
public sealed class DirectoryCampaignRegressionTests
{
    [Fact]
    public async Task MultipleReferenceFiles_AreValidated_AndGlobalReportIsWritten()
    {
        string root =
            CreateTemporaryDirectory();

        try
        {
            CopyReferenceFixture(root, "instance-a.xml");
            CopyReferenceFixture(root, "instance-b.xml");
            CopyReferenceFixture(root, "instance-c.xml");

            await File.WriteAllTextAsync(
                Path.Combine(root, "other.xml"),
                "<other />");

            var service =
                new DirectoryVerificationCampaignService();

            DirectoryVerificationCampaignResult result =
                await service.RunAsync(
                    root,
                    options: CreateOptions(),
                    cancellationToken: CancellationToken.None);

            Assert.True(result.IsValid);
            Assert.Equal(4, result.DiscoveredXmlFileCount);
            Assert.Equal(3, result.LoadedInstanceCount);
            Assert.Equal(1, result.IgnoredNonInstanceXmlFileCount);
            Assert.Equal(3, result.CandidateCount);
            Assert.Equal(3, result.BatchResult.ValidCandidateCount);
            Assert.Equal(0, result.BatchResult.InvalidCandidateCount);
            Assert.Equal(0, result.BatchResult.ExecutionFailureCount);

            string validationReport =
                Assert.IsType<string>(
                    result.ReportFiles.GlobalValidationReportPath);

            Assert.True(File.Exists(validationReport));

            string text =
                await File.ReadAllTextAsync(validationReport);

            Assert.Contains("Overall status", text);
            Assert.Contains("VALID", text);
            Assert.Contains("Candidate validation matrix", text);
            Assert.Contains("DJ-45", text);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ReRun_DoesNotScanGeneratedReportDirectory()
    {
        string root =
            CreateTemporaryDirectory();

        try
        {
            CopyReferenceFixture(root, "instance-a.xml");
            CopyReferenceFixture(root, "instance-b.xml");

            var service =
                new DirectoryVerificationCampaignService();

            DirectoryVerificationCampaignOptions options =
                CreateOptions();

            DirectoryVerificationCampaignResult first =
                await service.RunAsync(
                    root,
                    options: options,
                    cancellationToken: CancellationToken.None);

            DirectoryVerificationCampaignResult second =
                await service.RunAsync(
                    root,
                    options: options,
                    cancellationToken: CancellationToken.None);

            Assert.Equal(2, first.DiscoveredXmlFileCount);
            Assert.Equal(2, second.DiscoveredXmlFileCount);
            Assert.Equal(2, second.CandidateCount);
            Assert.True(second.IsValid);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task GlobalValidationReport_IsDeterministicAcrossEquivalentRuns()
    {
        string root =
            CreateTemporaryDirectory();

        try
        {
            CopyReferenceFixture(root, "001.xml");
            CopyReferenceFixture(root, "002.xml");
            CopyReferenceFixture(root, "003.xml");

            var service =
                new DirectoryVerificationCampaignService();

            DirectoryVerificationCampaignOptions options =
                CreateOptions();

            DirectoryVerificationCampaignResult first =
                await service.RunAsync(
                    root,
                    outputDirectory: "reports-a",
                    options: options,
                    cancellationToken: CancellationToken.None);

            DirectoryVerificationCampaignResult second =
                await service.RunAsync(
                    root,
                    outputDirectory: "reports-b",
                    options: options,
                    cancellationToken: CancellationToken.None);

            string firstText =
                await File.ReadAllTextAsync(
                    Assert.IsType<string>(
                        first.ReportFiles.GlobalValidationReportPath));

            string secondText =
                await File.ReadAllTextAsync(
                    Assert.IsType<string>(
                        second.ReportFiles.GlobalValidationReportPath));

            Assert.Equal(firstText, secondText);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static DirectoryVerificationCampaignOptions CreateOptions()
    {
        return new DirectoryVerificationCampaignOptions
        {
            WriteReports = true,
            SearchSubdirectories = true,
            IgnoreNonLotSizingInstanceXml = true,
            BatchOptions =
                new SolutionVerificationBatchOptions
                {
                    MaxDegreeOfParallelism = 2,
                    VerificationOptions =
                        new SolutionVerificationOptions
                        {
                            ApplyToSolutionEvaluation = false,
                            UpdateKnownResultFeasibility = false,
                            PromoteFullyVerifiedKnownResult = false
                        }
                }
        };
    }

    private static void CopyReferenceFixture(
        string directory,
        string fileName)
    {
        File.Copy(
            ReferenceFixture.GetPath(),
            Path.Combine(directory, fileName));
    }

    private static string CreateTemporaryDirectory()
    {
        string path =
            Path.Combine(
                Path.GetTempPath(),
                "LotSizingDataModel.Checker.Tests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
