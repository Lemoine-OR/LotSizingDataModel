using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines the objective-construction component used by the
/// standard solver-independent lot-sizing formulation.
/// </summary>
/// <remarks>
/// Implementations build the linear objective from the
/// normalized <see cref="LotSizingInstance"/> and from the
/// mathematical variables previously registered in the
/// supplied <see cref="MathematicalModelBuildContext"/>.
/// </remarks>
public interface IStandardLotSizingObjectiveBuilder
{
    /// <summary>
    /// Builds and assigns the objective function of the standard
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
    /// decision and cost families.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel objective construction.
    /// </param>
    /// <returns>
    /// Task representing objective construction.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="context"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when a required mathematical variable cannot be
    /// resolved from the build context or when the source cost
    /// data are inconsistent.
    /// </exception>
    ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default);
}
