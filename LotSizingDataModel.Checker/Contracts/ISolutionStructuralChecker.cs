using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Defines structural validation of a lot-sizing solution against
/// a lot-sizing instance.
/// </summary>
public interface ISolutionStructuralChecker
{
    /// <summary>
    /// Checks solution structure, decision keys, planning horizons,
    /// duplicate decisions, entity references, and completeness of
    /// the expected decision structure.
    /// </summary>
    /// <param name="instance">Problem instance.</param>
    /// <param name="solution">Candidate solution.</param>
    /// <param name="options">Checker options.</param>
    /// <returns>A structural checking result.</returns>
    SolutionCheckResult Check(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionCheckOptions options);
}
