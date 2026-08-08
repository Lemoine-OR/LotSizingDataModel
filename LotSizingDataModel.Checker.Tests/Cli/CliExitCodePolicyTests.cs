using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Campaign;
using LotSizingDataModel.Checker.Cli;
using LotSizingDataModel.Checker.Reporting;

namespace LotSizingDataModel.Checker.Tests.Cli;

public sealed class CliExitCodePolicyTests
{
    [Fact]
    public void SuccessfulCandidate_MapsToSuccess()
    {
        DirectoryVerificationCampaignResult campaign =
            CreateCampaign(
                new SolutionVerificationBatchItemResult
                {
                    Index = 0,
                    CandidateKey = "valid",
                    ExecutionSucceeded = true,
                    Summary =
                        new SolutionCheckSummary
                        {
                            IsValid = true
                        }
                });

        Assert.Equal(
            CliExitCode.Success,
            CliExitCodePolicy.Determine(campaign));
    }

    [Fact]
    public void InvalidCandidate_MapsToValidationFailed()
    {
        DirectoryVerificationCampaignResult campaign =
            CreateCampaign(
                new SolutionVerificationBatchItemResult
                {
                    Index = 0,
                    CandidateKey = "invalid",
                    ExecutionSucceeded = true,
                    Summary =
                        new SolutionCheckSummary
                        {
                            IsValid = false
                        }
                });

        Assert.Equal(
            CliExitCode.ValidationFailed,
            CliExitCodePolicy.Determine(campaign));
    }

    [Fact]
    public void ExecutionFailure_TakesPrecedenceOverInvalidCandidate()
    {
        DirectoryVerificationCampaignResult campaign =
            CreateCampaign(
                new SolutionVerificationBatchItemResult
                {
                    Index = 0,
                    CandidateKey = "invalid",
                    ExecutionSucceeded = true,
                    Summary =
                        new SolutionCheckSummary
                        {
                            IsValid = false
                        }
                },
                new SolutionVerificationBatchItemResult
                {
                    Index = 1,
                    CandidateKey = "failed",
                    ExecutionSucceeded = false,
                    FailureExceptionType =
                        typeof(InvalidOperationException).FullName,
                    FailureMessage =
                        "Synthetic failure"
                });

        Assert.Equal(
            CliExitCode.ExecutionFailure,
            CliExitCodePolicy.Determine(campaign));
    }

    [Fact]
    public void FileLoadFailure_MapsToExecutionFailure()
    {
        DirectoryVerificationCampaignResult campaign =
            new()
            {
                BatchResult =
                    new SolutionVerificationBatchResult(),
                FileLoadFailures =
                [
                    new InstanceFileLoadFailure
                    {
                        FilePath = "broken.xml",
                        RelativeFilePath = "broken.xml",
                        ExceptionType =
                            typeof(InvalidOperationException).FullName ??
                            nameof(InvalidOperationException),
                        Message = "Synthetic XML failure"
                    }
                ]
            };

        Assert.Equal(
            CliExitCode.ExecutionFailure,
            CliExitCodePolicy.Determine(campaign));
    }

    private static DirectoryVerificationCampaignResult CreateCampaign(
        params SolutionVerificationBatchItemResult[] items)
    {
        return new DirectoryVerificationCampaignResult
        {
            BatchResult =
                new SolutionVerificationBatchResult
                {
                    Items =
                        Array.AsReadOnly(items)
                }
        };
    }
}
