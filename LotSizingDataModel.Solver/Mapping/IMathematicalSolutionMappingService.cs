using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Defines a service that maps a generic mathematical solver
/// result to a normalized lot-sizing solution.
/// </summary>
public interface IMathematicalSolutionMappingService
{
    /// <summary>
    /// Maps a mathematical-model solve result to a normalized
    /// lot-sizing solution.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="model">
    /// Solver-independent mathematical model that was solved.
    /// </param>
    /// <param name="solveResult">
    /// Generic mathematical-model solve result.
    /// </param>
    /// <param name="options">
    /// Mathematical-solution mapping options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the mapping operation.
    /// </param>
    /// <returns>
    /// Task returning the complete mapping result.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="model"/>,
    /// <paramref name="solveResult"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    ValueTask<MathematicalSolutionMappingResult> MapAsync(
        LotSizingInstance instance,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        MathematicalSolutionMappingOptions options,
        CancellationToken cancellationToken = default);
}
