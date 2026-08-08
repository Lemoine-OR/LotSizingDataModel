using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines the constraint-construction component used by the
/// standard solver-independent lot-sizing formulation.
/// </summary>
/// <remarks>
/// Implementations create all linear constraints required by
/// the standard formulation from the normalized
/// <see cref="LotSizingInstance"/> and from the mathematical
/// variables previously registered in the supplied
/// <see cref="MathematicalModelBuildContext"/>.
/// </remarks>
public interface IStandardLotSizingConstraintBuilder
{
    /// <summary>
    /// Builds and adds all constraints of the standard
    /// lot-sizing formulation.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context containing the variables
    /// created by the standard variable builder.
    /// </param>
    /// <param name="options">
    /// Standard formulation options controlling optional
    /// constraint families.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel constraint construction.
    /// </param>
    /// <returns>
    /// Task representing constraint construction.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="context"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when a required mathematical variable cannot be
    /// resolved from the build context or when the source data
    /// cannot be represented consistently by the standard
    /// formulation.
    /// </exception>
    ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default);
}
