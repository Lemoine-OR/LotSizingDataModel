using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Defines a mapper responsible for one family of lot-sizing
/// decisions identified by a mathematical domain-key category.
/// </summary>
/// <remarks>
/// Implementations translate mathematical-variable values into
/// the corresponding decision objects of a
/// <see cref="LotSizingSolution"/>. This contract allows each
/// decision family to be mapped independently while sharing the
/// same solver-independent mapping context.
/// </remarks>
public interface IMathematicalDecisionMapper
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    string Category
    {
        get;
    }

    /// <summary>
    /// Maps the decision values handled by this mapper into a
    /// lot-sizing solution.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="context"/> or
    /// <paramref name="solution"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when one or more mathematical values cannot be
    /// mapped consistently to the target decision family.
    /// </exception>
    void Map(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution);
}
