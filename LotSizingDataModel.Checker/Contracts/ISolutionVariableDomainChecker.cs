using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Defines numerical-domain validation for lot-sizing solution decisions.
/// </summary>
public interface ISolutionVariableDomainChecker
{
    /// <summary>
    /// Checks all numerical decision values contained in a candidate solution.
    /// </summary>
    /// <param name="solution">Candidate solution to inspect.</param>
    /// <param name="options">Checker options and numerical tolerances.</param>
    /// <returns>Detailed variable-domain checking result.</returns>
    SolutionCheckResult Check(
        LotSizingSolution solution,
        SolutionCheckOptions options);
}
