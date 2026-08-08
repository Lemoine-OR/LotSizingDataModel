using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Defines mathematical-feasibility validation of a candidate
/// lot-sizing solution against a mathematical model.
/// </summary>
public interface IMathematicalFeasibilityChecker
{
    /// <summary>
    /// Projects the candidate solution onto the mathematical model,
    /// evaluates all enabled constraints, and reports every violation.
    /// </summary>
    /// <param name="model">Mathematical model defining feasibility.</param>
    /// <param name="solution">Candidate business solution.</param>
    /// <param name="options">Checker options and tolerances.</param>
    /// <returns>Detailed feasibility checking result.</returns>
    SolutionCheckResult Check(
        MathematicalModel model,
        LotSizingSolution solution,
        SolutionCheckOptions options);
}
