using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines a service that selects a formulation and builds a
/// solver-independent mathematical model.
/// </summary>
public interface IMathematicalModelBuildService
{
    /// <summary>
    /// Selects a compatible formulation and builds the
    /// mathematical model.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to formulate.
    /// </param>
    /// <param name="registry">
    /// Registry containing the available formulations.
    /// </param>
    /// <param name="options">
    /// Mathematical-model build options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel formulation selection and model
    /// construction.
    /// </param>
    /// <returns>
    /// Task returning the complete mathematical-model build
    /// result.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="registry"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    ValueTask<MathematicalModelBuildResult> BuildAsync(
        LotSizingInstance instance,
        MathematicalModelFormulationRegistry registry,
        MathematicalModelBuildOptions options,
        CancellationToken cancellationToken = default);
}
