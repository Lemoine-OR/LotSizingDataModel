using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Provides the standard solver-independent mixed-integer
/// lot-sizing formulation.
/// </summary>
/// <remarks>
/// The formulation delegates variable, objective, and
/// constraint construction to dedicated components. This keeps
/// the formulation orchestration independent from any native
/// solver API and allows the three construction stages to evolve
/// independently.
/// </remarks>
public sealed class StandardLotSizingFormulation :
    MathematicalModelFormulationBase
{
    /// <summary>
    /// Canonical identifier of the standard lot-sizing
    /// formulation.
    /// </summary>
    public const string StandardFormulationId =
        "standard";

    private readonly IStandardLotSizingVariableBuilder
        _variableBuilder;

    private readonly IStandardLotSizingObjectiveBuilder
        _objectiveBuilder;

    private readonly IStandardLotSizingConstraintBuilder
        _constraintBuilder;

    private readonly StandardLotSizingFormulationOptions
        _options;

    /// <summary>
    /// Initializes the standard lot-sizing formulation.
    /// </summary>
    /// <param name="variableBuilder">
    /// Component responsible for mathematical variable
    /// construction.
    /// </param>
    /// <param name="objectiveBuilder">
    /// Component responsible for objective construction.
    /// </param>
    /// <param name="constraintBuilder">
    /// Component responsible for constraint construction.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the supplied dependencies is
    /// <see langword="null"/>.
    /// </exception>
    public StandardLotSizingFormulation(
        IStandardLotSizingVariableBuilder variableBuilder,
        IStandardLotSizingObjectiveBuilder objectiveBuilder,
        IStandardLotSizingConstraintBuilder constraintBuilder,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            variableBuilder);

        ArgumentNullException.ThrowIfNull(
            objectiveBuilder);

        ArgumentNullException.ThrowIfNull(
            constraintBuilder);

        ArgumentNullException.ThrowIfNull(
            options);

        StandardLotSizingFormulationOptions normalizedOptions =
            options.Clone();

        normalizedOptions.EnsureValid();

        _variableBuilder =
            variableBuilder;

        _objectiveBuilder =
            objectiveBuilder;

        _constraintBuilder =
            constraintBuilder;

        _options =
            normalizedOptions;
    }

    /// <summary>
    /// Gets the unique formulation identifier.
    /// </summary>
    public override string FormulationId =>
        StandardFormulationId;

    /// <summary>
    /// Gets the human-readable formulation name.
    /// </summary>
    public override string Name =>
        "Standard lot-sizing MILP formulation";

    /// <summary>
    /// Gets the formulation description.
    /// </summary>
    public override string Description =>
        "Solver-independent mixed-integer linear formulation " +
        "for normalized lot-sizing instances.";

    /// <summary>
    /// Gets an independent copy of the options used by this
    /// formulation.
    /// </summary>
    public StandardLotSizingFormulationOptions Options =>
        _options.Clone();

    /// <summary>
    /// Determines whether the standard formulation can build a
    /// mathematical model for the supplied instance.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to evaluate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the instance has a strictly
    /// positive planning horizon; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool CanBuild(
        LotSizingInstance instance)
    {
        if (instance is null)
        {
            return false;
        }

        return instance.PlanningHorizon > 0;
    }

    /// <summary>
    /// Creates the mathematical model name.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance being formulated.
    /// </param>
    /// <returns>
    /// Human-readable mathematical model name.
    /// </returns>
    protected override string CreateModelName(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        if (!string.IsNullOrWhiteSpace(
                instance.Name))
        {
            return
                $"{instance.Name.Trim()} - {FormulationId}";
        }

        if (!string.IsNullOrWhiteSpace(
                instance.InstanceId))
        {
            return
                $"{instance.InstanceId.Trim()} - {FormulationId}";
        }

        return base.CreateModelName(
            instance);
    }

    /// <summary>
    /// Builds and registers all mathematical variables.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel variable construction.
    /// </param>
    /// <returns>
    /// Task representing variable construction.
    /// </returns>
    protected override ValueTask BuildVariablesAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken)
    {
        return _variableBuilder.BuildAsync(
            instance,
            context,
            _options,
            cancellationToken);
    }

    /// <summary>
    /// Builds the mathematical objective.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel objective construction.
    /// </param>
    /// <returns>
    /// Task representing objective construction.
    /// </returns>
    protected override ValueTask BuildObjectiveAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken)
    {
        return _objectiveBuilder.BuildAsync(
            instance,
            context,
            _options,
            cancellationToken);
    }

    /// <summary>
    /// Builds all mathematical constraints.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel constraint construction.
    /// </param>
    /// <returns>
    /// Task representing constraint construction.
    /// </returns>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken)
    {
        return _constraintBuilder.BuildAsync(
            instance,
            context,
            _options,
            cancellationToken);
    }
}
