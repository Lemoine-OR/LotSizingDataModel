using LotSizingDataModel.Checker.Campaign;

namespace LotSizingDataModel.Checker.Cli;

/// <summary>
/// Determines the process exit code corresponding to one completed checker
/// campaign.
/// </summary>
internal static class CliExitCodePolicy
{
    /// <summary>
    /// Maps a completed campaign result to a stable CLI exit code.
    /// </summary>
    /// <param name="result">Completed campaign result.</param>
    /// <returns>The exit code to return from the checker process.</returns>
    public static CliExitCode Determine(
        DirectoryVerificationCampaignResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.FileLoadFailureCount > 0 ||
            result.BatchResult.ExecutionFailureCount > 0)
        {
            return CliExitCode.ExecutionFailure;
        }

        if (result.BatchResult.InvalidCandidateCount > 0)
        {
            return CliExitCode.ValidationFailed;
        }

        return CliExitCode.Success;
    }
}
