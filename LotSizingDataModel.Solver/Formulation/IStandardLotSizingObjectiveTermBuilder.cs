using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines one cost-term family of the standard lot-sizing
/// objective function.
/// </summary>
public interface IStandardLotSizingObjectiveTermBuilder
{
    /// <summary>
    /// Gets the unique identifier of the objective-term family.
    /// </summary>
    string TermFamilyId
    {
        get;
    }

    /// <summary>
    /// Determines whether this objective-term family is enabled.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when this term family must be
    /// included; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options);

    /// <summary>
    /// Adds all linear objective terms belonging to this family.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="expressionBuilder">
    /// Shared objective-expression builder.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel term construction.
    /// </param>
    /// <returns>
    /// Task representing objective-term construction.
    /// </returns>
    ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default);
}
