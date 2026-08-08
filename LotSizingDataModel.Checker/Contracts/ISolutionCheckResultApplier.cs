using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Integration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance.Results;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Applies a solver-independent checker result to the serializable solution
/// evaluation and, optionally, to a known-result record.
/// </summary>
public interface ISolutionCheckResultApplier
{
    /// <summary>
    /// Applies one checker result.
    /// </summary>
    /// <param name="solution">Candidate solution to update.</param>
    /// <param name="checkResult">Independent checker result.</param>
    /// <param name="options">Verification and persistence options.</param>
    /// <param name="evaluatedAtUtc">UTC instant associated with the evaluation.</param>
    /// <param name="knownResult">Optional known-result record to update.</param>
    /// <returns>Information describing the domain-object mutations performed.</returns>
    SolutionCheckApplicationResult Apply(
        LotSizingSolution solution,
        SolutionCheckResult checkResult,
        SolutionVerificationOptions options,
        DateTime evaluatedAtUtc,
        KnownResult? knownResult = null);
}
