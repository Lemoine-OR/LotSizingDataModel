using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines a solver-independent mathematical formulation for a
/// lot-sizing instance.
/// </summary>
/// <remarks>
/// An implementation creates the common mathematical model only.
/// Solver-specific translation is performed later by the selected
/// solver adapter.
/// </remarks>
public interface IMathematicalModelFormulation
{
    /// <summary>
    /// Gets the unique formulation identifier.
    /// </summary>
    string FormulationId
    {
        get;
    }

    /// <summary>
    /// Gets the human-readable formulation name.
    /// </summary>
    string Name
    {
        get;
    }

    /// <summary>
    /// Gets the formulation description.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Determines whether the formulation supports a
    /// lot-sizing instance.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to evaluate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the formulation can represent
    /// the instance; otherwise, <see langword="false"/>.
    /// </returns>
    bool CanBuild(
        LotSizingInstance instance);

    /// <summary>
    /// Builds the solver-independent mathematical model.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to formulate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel model construction.
    /// </param>
    /// <returns>
    /// Task returning the complete mathematical model.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the formulation cannot represent the supplied
    /// instance or when model construction fails.
    /// </exception>
    ValueTask<MathematicalModel> BuildAsync(
        LotSizingInstance instance,
        CancellationToken cancellationToken = default);
}
