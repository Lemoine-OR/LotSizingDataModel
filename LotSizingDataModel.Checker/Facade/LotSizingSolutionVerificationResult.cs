using LotSizingDataModel.Checker.Integration;
using LotSizingDataModel.Checker.Results;

namespace LotSizingDataModel.Checker.Facade;

/// <summary>
/// Contains both the independent checker result and information about the
/// application of that result to domain objects.
/// </summary>
public sealed class LotSizingSolutionVerificationResult
{
    /// <summary>
    /// Gets the independent checker result.
    /// </summary>
    public required SolutionCheckResult CheckResult
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the result of applying the checker result to domain objects.
    /// </summary>
    public required SolutionCheckApplicationResult ApplicationResult
    {
        get;
        init;
    }

    /// <summary>
    /// Gets whether the requested independent checks all succeeded.
    /// </summary>
    public bool IsValid =>
        CheckResult.IsValid;
}
