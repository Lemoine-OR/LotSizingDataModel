using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines the variable-construction component used by the
/// standard solver-independent lot-sizing formulation.
/// </summary>
/// <remarks>
/// Implementations inspect the normalized
/// <see cref="LotSizingInstance"/> and create the mathematical
/// decision variables required by the standard formulation.
/// Every created variable must be registered through the
/// supplied <see cref="MathematicalModelBuildContext"/> so that
/// objective and constraint builders can resolve it later from
/// its canonical domain key.
/// </remarks>
public interface IStandardLotSizingVariableBuilder
{
    /// <summary>
    /// Creates and registers all mathematical variables required
    /// by the standard lot-sizing formulation.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context receiving the generated
    /// variables.
    /// </param>
    /// <param name="options">
    /// Standard formulation options controlling optional
    /// decision families.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel variable construction.
    /// </param>
    /// <returns>
    /// Task representing variable construction.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="context"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the source instance contains data that cannot
    /// be represented consistently by the standard formulation.
    /// </exception>
    ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default);
}
