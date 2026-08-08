using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Contracts;

/// <summary>
/// Defines a generic, solver-independent checker for lot-sizing solutions.
/// </summary>
public interface ILotSizingSolutionChecker
{
    /// <summary>
    /// Checks a candidate solution against its lot-sizing instance.
    /// </summary>
    /// <param name="instance">
    /// Instance defining the problem to satisfy.
    /// </param>
    /// <param name="solution">
    /// Candidate solution to validate.
    /// </param>
    /// <param name="options">
    /// Optional checking configuration.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Detailed structural, feasibility, and objective diagnostics.
    /// </returns>
    Task<SolutionCheckResult> CheckAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionCheckOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks a candidate solution against its lot-sizing instance while
    /// reusing an already constructed mathematical model.
    /// </summary>
    /// <remarks>
    /// This overload is intended for solver pipelines that already generated
    /// the mathematical model used to obtain the candidate solution. It avoids
    /// rebuilding the same formulation solely for independent checking.
    /// </remarks>
    /// <param name="instance">
    /// Instance defining the problem to satisfy.
    /// </param>
    /// <param name="solution">
    /// Candidate solution to validate.
    /// </param>
    /// <param name="mathematicalModel">
    /// Mathematical model generated from <paramref name="instance"/>.
    /// </param>
    /// <param name="options">
    /// Optional checking configuration.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Detailed structural, feasibility, and objective diagnostics.
    /// </returns>
    Task<SolutionCheckResult> CheckAsync(
        LotSizingInstance instance,
        LotSizingSolution solution,
        MathematicalModel mathematicalModel,
        SolutionCheckOptions? options = null,
        CancellationToken cancellationToken = default);
}
