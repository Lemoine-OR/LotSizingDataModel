using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Provides common validation and helper methods for one
/// objective-term family of the standard lot-sizing
/// formulation.
/// </summary>
public abstract class StandardLotSizingObjectiveTermBuilderBase :
    IStandardLotSizingObjectiveTermBuilder
{
    /// <summary>
    /// Gets the unique identifier of the objective-term family.
    /// </summary>
    public abstract string TermFamilyId
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
    /// objective-term family.
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
    /// Task representing term construction.
    /// </returns>
    public async ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            expressionBuilder);

        ArgumentNullException.ThrowIfNull(
            options);

        if (string.IsNullOrWhiteSpace(
                TermFamilyId))
        {
            throw new InvalidOperationException(
                "A standard objective-term builder must expose " +
                "a non-empty term-family identifier.");
        }

        options.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        await BuildTermsAsync(
            instance,
            context,
            expressionBuilder,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Builds the concrete objective-term family.
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
    /// Task representing term construction.
    /// </returns>
    protected abstract ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds one variable cost term to the shared objective.
    /// </summary>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="expressionBuilder">
    /// Shared objective-expression builder.
    /// </param>
    /// <param name="domainKey">
    /// Canonical variable domain key.
    /// </param>
    /// <param name="coefficient">
    /// Linear cost coefficient.
    /// </param>
    protected static void AddCostTerm(
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        string domainKey,
        double coefficient)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            expressionBuilder);

        if (string.IsNullOrWhiteSpace(
                domainKey))
        {
            throw new ArgumentException(
                "A variable domain key is required.",
                nameof(domainKey));
        }

        if (!double.IsFinite(
                coefficient))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coefficient),
                coefficient,
                "An objective coefficient must be finite.");
        }

        MathematicalVariable variable =
            context.GetVariable(
                domainKey);

        expressionBuilder.Add(
            variable,
            coefficient);
    }
}
