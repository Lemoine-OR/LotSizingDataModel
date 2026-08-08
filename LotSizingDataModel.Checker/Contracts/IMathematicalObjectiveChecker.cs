using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Defines independent objective-value validation of a candidate
/// lot-sizing solution against a mathematical model.
/// </summary>
public interface IMathematicalObjectiveChecker
{
    /// <summary>
    /// Projects the candidate solution onto the mathematical model,
    /// independently evaluates the model objective, and compares the
    /// recomputed value with the objective value stored in the solution.
    /// </summary>
    /// <param name="model">Mathematical model defining the objective.</param>
    /// <param name="solution">Candidate business solution.</param>
    /// <param name="options">Checker options and tolerances.</param>
    /// <returns>Detailed objective checking result.</returns>
    SolutionCheckResult Check(
        MathematicalModel model,
        LotSizingSolution solution,
        SolutionCheckOptions options);
}
