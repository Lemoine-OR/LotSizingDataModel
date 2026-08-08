using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Defines projection of a normalized lot-sizing solution onto
/// the variables of a solver-independent mathematical model.
/// </summary>
public interface IMathematicalSolutionValueProjector
{
    /// <summary>
    /// Projects candidate business-decision values onto all
    /// mathematical variables that have supported domain keys.
    /// </summary>
    /// <param name="model">Mathematical model to populate.</param>
    /// <param name="solution">Candidate lot-sizing solution.</param>
    /// <returns>Projected values and diagnostics.</returns>
    MathematicalSolutionProjectionResult Project(
        MathematicalModel model,
        LotSizingSolution solution);
}
