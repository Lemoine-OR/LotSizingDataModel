using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Campaign;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Campaign;

/// <summary>
/// Non-brittle smoke tests ensuring that a small regression campaign can be
/// processed with bounded parallelism without execution failures.
/// </summary>
public sealed class CampaignPerformanceSmokeTests
{
    [Fact]
    [Trait("Category", "PerformanceSmoke")]
    public async Task EightCandidates_CompleteWithoutExecutionFailure()
    {
        string root =
            Path.Combine(
                Path.GetTempPath(),
                "LotSizingDataModel.Checker.Tests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            for (int index = 0; index < 8; index++)
            {
                File.Copy(
                    ReferenceFixture.GetPath(),
                    Path.Combine(root, $"instance-{index + 1:D2}.xml"));
            }

            var options =
                new DirectoryVerificationCampaignOptions
                {
                    WriteReports = false,
                    BatchOptions =
                        new SolutionVerificationBatchOptions
                        {
                            MaxDegreeOfParallelism = 4,
                            VerificationOptions =
                                new SolutionVerificationOptions
                                {
                                    ApplyToSolutionEvaluation = false,
                                    UpdateKnownResultFeasibility = false,
                                    PromoteFullyVerifiedKnownResult = false
                                }
                        }
                };

            var service =
                new DirectoryVerificationCampaignService();

            DirectoryVerificationCampaignResult result =
                await service.RunAsync(
                    root,
                    options: options,
                    cancellationToken: CancellationToken.None);

            Assert.Equal(8, result.CandidateCount);
            Assert.Equal(8, result.BatchResult.CompletedCandidateCount);
            Assert.Equal(8, result.BatchResult.ValidCandidateCount);
            Assert.Equal(0, result.BatchResult.ExecutionFailureCount);
            Assert.True(result.IsValid);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
