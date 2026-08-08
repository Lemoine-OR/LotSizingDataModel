using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Provides common validation and helper methods for one
/// constraint family of the standard lot-sizing formulation.
/// </summary>
public abstract class StandardLotSizingConstraintFamilyBuilderBase :
    IStandardLotSizingConstraintFamilyBuilder
{
    /// <summary>
    /// Gets the unique identifier of the constraint family.
    /// </summary>
    public abstract string ConstraintFamilyId
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
    /// <see langword="true"/> by default.
    /// </returns>
    public virtual bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        return true;
    }

    /// <summary>
    /// Validates common arguments and builds the concrete
    /// constraint family.
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
    /// Task representing constraint construction.
    /// </returns>
    public async ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            options);

        if (string.IsNullOrWhiteSpace(
                ConstraintFamilyId))
        {
            throw new InvalidOperationException(
                "A standard constraint-family builder must " +
                "expose a non-empty family identifier.");
        }

        options.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        await BuildConstraintsAsync(
            instance,
            context,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Builds the concrete constraint family.
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
    /// Task representing constraint construction.
    /// </returns>
    protected abstract ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a required variable from its canonical domain
    /// key.
    /// </summary>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="domainKey">
    /// Canonical variable domain key.
    /// </param>
    /// <returns>
    /// Registered mathematical variable.
    /// </returns>
    protected static MathematicalVariable GetVariable(
        MathematicalModelBuildContext context,
        string domainKey)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        if (string.IsNullOrWhiteSpace(
                domainKey))
        {
            throw new ArgumentException(
                "A variable domain key is required.",
                nameof(domainKey));
        }

        return context.GetVariable(
            domainKey);
    }

    /// <summary>
    /// Adds one linear constraint to the mathematical model.
    /// </summary>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="name">
    /// Constraint name.
    /// </param>
    /// <param name="expression">
    /// Left-hand-side expression.
    /// </param>
    /// <param name="sense">
    /// Constraint relational sense.
    /// </param>
    /// <param name="rightHandSide">
    /// Right-hand-side value.
    /// </param>
    /// <param name="domainKey">
    /// Optional business-domain key.
    /// </param>
    /// <param name="description">
    /// Optional constraint description.
    /// </param>
    protected static void AddConstraint(
        MathematicalModelBuildContext context,
        string name,
        LinearExpression expression,
        MathematicalConstraintSense sense,
        double rightHandSide,
        string domainKey = "",
        string description = "")
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            expression);

        context.AddConstraint(
            name,
            expression,
            sense,
            rightHandSide,
            domainKey,
            description);
    }
}
