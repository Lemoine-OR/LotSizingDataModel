using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Defines a component that converts a generic mathematical
/// solver result into a normalized lot-sizing solution.
/// </summary>
/// <remarks>
/// Implementations use mathematical-variable domain keys to map
/// solver values back to business decision objects. They must not
/// depend on any native solver API.
/// </remarks>
public interface IMathematicalSolutionMapper
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
    /// <returns>
    /// Normalized lot-sizing solution.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="model"/>, or
    /// <paramref name="solveResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when solver values cannot be mapped consistently
    /// to the lot-sizing domain model.
    /// </exception>
    LotSizingSolution Map(
        LotSizingInstance instance,
        MathematicalModel model,
        Execution.MathematicalModelSolveResult solveResult);
}
