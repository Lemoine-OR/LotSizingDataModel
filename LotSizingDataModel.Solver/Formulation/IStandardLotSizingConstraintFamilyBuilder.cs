using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines one constraint family of the standard lot-sizing
/// formulation.
/// </summary>
public interface IStandardLotSizingConstraintFamilyBuilder
{
    /// <summary>
    /// Gets the unique identifier of the constraint family.
    /// </summary>
    string ConstraintFamilyId
    {
        get;
    }

    /// <summary>
    /// Determines whether this constraint family is enabled.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when this family must be built;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options);

    /// <summary>
    /// Builds all constraints belonging to this family.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel constraint construction.
    /// </param>
    /// <returns>
    /// Task representing constraint-family construction.
    /// </returns>
    ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default);
}
