using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Provides common validation and variable-registration helpers
/// for one variable family of the standard lot-sizing
/// formulation.
/// </summary>
/// <remarks>
/// Derived classes only need to implement the family identifier,
/// the family-enablement rule, and the actual family-specific
/// variable construction.
/// </remarks>
public abstract class StandardLotSizingVariableFamilyBuilderBase :
    IStandardLotSizingVariableFamilyBuilder
{
    /// <summary>
    /// Gets the unique identifier of the variable family.
    /// </summary>
    public abstract string FamilyId
    {
        get;
    }

    /// <summary>
    /// Determines whether this variable family is enabled for
    /// the supplied instance and formulation options.
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
    /// Validates the common arguments and builds the
    /// family-specific variables.
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
    /// Token used to cancel family construction.
    /// </param>
    /// <returns>
    /// Task representing variable-family construction.
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
                FamilyId))
        {
            throw new InvalidOperationException(
                "A standard variable-family builder must expose " +
                "a non-empty family identifier.");
        }

        options.EnsureValid();

        cancellationToken.ThrowIfCancellationRequested();

        await BuildFamilyAsync(
            instance,
            context,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Builds the variables belonging to the concrete family.
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
    /// Token used to cancel family construction.
    /// </param>
    /// <returns>
    /// Task representing concrete family construction.
    /// </returns>
    protected abstract ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates and registers one continuous non-negative
    /// variable.
    /// </summary>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="name">
    /// Mathematical variable name.
    /// </param>
    /// <param name="domainKey">
    /// Canonical business-domain key.
    /// </param>
    /// <param name="upperBound">
    /// Variable upper bound.
    /// </param>
    /// <param name="description">
    /// Optional variable description.
    /// </param>
    /// <returns>
    /// Created mathematical variable.
    /// </returns>
    protected static MathematicalVariable AddNonNegativeContinuousVariable(
        MathematicalModelBuildContext context,
        string name,
        string domainKey,
        double upperBound = double.PositiveInfinity,
        string description = "")
    {
        ArgumentNullException.ThrowIfNull(
            context);

        return context.AddVariable(
            name,
            domainKey,
            MathematicalVariableType.Continuous,
            0.0,
            upperBound,
            description);
    }

    /// <summary>
    /// Creates and registers one binary variable.
    /// </summary>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="name">
    /// Mathematical variable name.
    /// </param>
    /// <param name="domainKey">
    /// Canonical business-domain key.
    /// </param>
    /// <param name="description">
    /// Optional variable description.
    /// </param>
    /// <returns>
    /// Created mathematical variable.
    /// </returns>
    protected static MathematicalVariable AddBinaryVariable(
        MathematicalModelBuildContext context,
        string name,
        string domainKey,
        string description = "")
    {
        ArgumentNullException.ThrowIfNull(
            context);

        return context.AddVariable(
            name,
            domainKey,
            MathematicalVariableType.Binary,
            0.0,
            1.0,
            description);
    }

    /// <summary>
    /// Determines whether a finite non-negative upper bound is
    /// structurally equal to zero according to the formulation
    /// options.
    /// </summary>
    /// <param name="upperBound">
    /// Candidate upper bound.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the variable may be omitted
    /// as structurally zero; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    protected static bool IsStructurallyZero(
        double upperBound,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        if (!options.RemoveStructurallyZeroVariables)
        {
            return false;
        }

        return
            double.IsFinite(
                upperBound) &&
            upperBound >= 0.0 &&
            upperBound <=
                options.StructuralZeroTolerance;
    }
}
