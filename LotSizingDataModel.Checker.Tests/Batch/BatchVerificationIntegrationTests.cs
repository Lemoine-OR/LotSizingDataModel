using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Batch;

public sealed class BatchVerificationIntegrationTests
{
    [Fact]
    public async Task MixedBatch_PreservesInputOrder_AndCountsValidity()
    {
        ReferenceFixtureData valid =
            ReferenceFixture.Load();

        ReferenceFixtureData invalid =
            ReferenceFixture.Load();

        ReflectionMutation.SetScalarProperty(
            invalid.Solution.Evaluation,
            "ObjectiveValue",
            999999.0);

        SolutionVerificationBatchCandidate[] candidates =
        [
            SolutionVerificationBatchCandidate.ForSolution(
                "valid",
                valid.Instance,
                valid.Solution,
                "Reference valid solution"),

            SolutionVerificationBatchCandidate.ForSolution(
                "invalid-objective",
                invalid.Instance,
                invalid.Solution,
                "Reference solution with wrong objective")
        ];

        var service =
            new LotSizingSolutionBatchVerificationService();

        var options =
            new SolutionVerificationBatchOptions
            {
                MaxDegreeOfParallelism = 2,
                VerificationOptions =
                    new SolutionVerificationOptions
                    {
                        ApplyToSolutionEvaluation = false,
                        UpdateKnownResultFeasibility = false,
                        PromoteFullyVerifiedKnownResult = false,
                        CheckOptions =
                            new SolutionCheckOptions
                            {
                                Level = SolutionCheckLevel.Full
                            }
                    }
            };

        SolutionVerificationBatchResult result =
            await service.VerifyAsync(
                candidates,
                options);

        Assert.Equal(2, result.CandidateCount);
        Assert.Equal(1, result.ValidCandidateCount);
        Assert.Equal(1, result.InvalidCandidateCount);
        Assert.Equal(0, result.ExecutionFailureCount);
        Assert.Equal("valid", result.Items[0].CandidateKey);
        Assert.Equal("invalid-objective", result.Items[1].CandidateKey);
        Assert.True(result.Items[0].IsValid);
        Assert.False(result.Items[1].IsValid);
    }

    [Fact]
    public async Task CancelledBatch_PropagatesCancellation()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        SolutionVerificationBatchCandidate[] candidates =
        [
            SolutionVerificationBatchCandidate.ForSolution(
                "candidate",
                data.Instance,
                data.Solution)
        ];

        using var cancellationSource =
            new CancellationTokenSource();

        cancellationSource.Cancel();

        var service =
            new LotSizingSolutionBatchVerificationService();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () =>
                service.VerifyAsync(
                    candidates,
                    cancellationToken:
                        cancellationSource.Token));
    }
}
