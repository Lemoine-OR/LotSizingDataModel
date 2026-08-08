using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Provides a reusable base implementation for
/// solver-independent lot-sizing formulations.
/// </summary>
public abstract class MathematicalModelFormulationBase :
    IMathematicalModelFormulation
{
    /// <summary>
    /// Gets the unique formulation identifier.
    /// </summary>
    public abstract string FormulationId
    {
        get;
    }

    /// <summary>
    /// Gets the human-readable formulation name.
    /// </summary>
    public abstract string Name
    {
        get;
    }

    /// <summary>
    /// Gets the formulation description.
    /// </summary>
    public virtual string Description =>
        string.Empty;

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
    public abstract bool CanBuild(
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the formulation metadata is invalid, the
    /// instance is unsupported, or the generated model is
    /// invalid.
    /// </exception>
    public async ValueTask<MathematicalModel> BuildAsync(
        LotSizingInstance instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        EnsureMetadataIsValid();

        cancellationToken.ThrowIfCancellationRequested();

        if (!CanBuild(
                instance))
        {
            throw new InvalidOperationException(
                $"Formulation '{FormulationId}' does not support " +
                "the supplied lot-sizing instance.");
        }

        var context =
            new MathematicalModelBuildContext(
                CreateModelName(
                    instance));

        context.SetDescription(
            Description);

        await BuildVariablesAsync(
            instance,
            context,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        await BuildObjectiveAsync(
            instance,
            context,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        await BuildConstraintsAsync(
            instance,
            context,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        MathematicalModel model =
            context.Build(
                clone:
                    false);

        ValidateBuiltModel(
            instance,
            model);

        return model;
    }

    /// <summary>
    /// Creates the mathematical model name.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance being formulated.
    /// </param>
    /// <returns>
    /// Mathematical model name.
    /// </returns>
    protected virtual string CreateModelName(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        return FormulationId;
    }

    /// <summary>
    /// Builds and registers all mathematical variables.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance being formulated.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel model construction.
    /// </param>
    /// <returns>
    /// Task representing variable construction.
    /// </returns>
    protected abstract ValueTask BuildVariablesAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Builds the mathematical objective.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance being formulated.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel model construction.
    /// </param>
    /// <returns>
    /// Task representing objective construction.
    /// </returns>
    protected abstract ValueTask BuildObjectiveAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Builds all mathematical constraints.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance being formulated.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel model construction.
    /// </param>
    /// <returns>
    /// Task representing constraint construction.
    /// </returns>
    protected abstract ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Performs formulation-specific validation after the model
    /// has been built.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="model">
    /// Generated mathematical model.
    /// </param>
    protected virtual void ValidateBuiltModel(
        LotSizingInstance instance,
        MathematicalModel model)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            model);

        model.EnsureValid();
    }

    private void EnsureMetadataIsValid()
    {
        if (string.IsNullOrWhiteSpace(
                FormulationId))
        {
            throw new InvalidOperationException(
                "A formulation identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(
                Name))
        {
            throw new InvalidOperationException(
                "A formulation name is required.");
        }
    }
}
