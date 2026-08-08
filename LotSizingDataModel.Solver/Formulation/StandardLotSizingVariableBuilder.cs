using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Aggregates the variable-family builders used by the standard
/// lot-sizing formulation.
/// </summary>
/// <remarks>
/// This class deliberately contains no direct knowledge of the
/// physical supply-chain classes. Each variable family is built
/// by a dedicated <see cref="IStandardLotSizingVariableFamilyBuilder"/>
/// implementation. This keeps the orchestration stable while
/// individual decision families evolve.
/// </remarks>
public sealed class StandardLotSizingVariableBuilder :
    IStandardLotSizingVariableBuilder
{
    private readonly IReadOnlyList<IStandardLotSizingVariableFamilyBuilder>
        _familyBuilders;

    /// <summary>
    /// Initializes the standard variable builder.
    /// </summary>
    /// <param name="familyBuilders">
    /// Ordered variable-family builders.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="familyBuilders"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection contains a
    /// <see langword="null"/> builder or duplicate family
    /// identifiers.
    /// </exception>
    public StandardLotSizingVariableBuilder(
        IEnumerable<IStandardLotSizingVariableFamilyBuilder> familyBuilders)
    {
        ArgumentNullException.ThrowIfNull(
            familyBuilders);

        IStandardLotSizingVariableFamilyBuilder[] builders =
            familyBuilders.ToArray();

        if (builders.Any(
                builder =>
                    builder is null))
        {
            throw new InvalidOperationException(
                "The standard variable-family builder collection " +
                "cannot contain a null entry.");
        }

        string[] duplicateFamilyIds =
            builders
                .GroupBy(
                    builder =>
                        builder.FamilyId,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .ToArray();

        if (duplicateFamilyIds.Length > 0)
        {
            throw new InvalidOperationException(
                "Duplicate standard variable-family identifiers " +
                "were registered: " +
                string.Join(
                    ", ",
                    duplicateFamilyIds));
        }

        _familyBuilders =
            builders;
    }

    /// <summary>
    /// Gets the ordered variable-family builders.
    /// </summary>
    public IReadOnlyList<IStandardLotSizingVariableFamilyBuilder>
        FamilyBuilders =>
            _familyBuilders;

    /// <summary>
    /// Creates and registers all mathematical variables required
    /// by the standard lot-sizing formulation.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context receiving the generated
    /// variables.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel variable construction.
    /// </param>
    /// <returns>
    /// Task representing variable construction.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/>,
    /// <paramref name="context"/>, or
    /// <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
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

        options.EnsureValid();

        foreach (
            IStandardLotSizingVariableFamilyBuilder familyBuilder
            in _familyBuilders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!familyBuilder.IsEnabled(
                    instance,
                    options))
            {
                continue;
            }

            await familyBuilder.BuildAsync(
                instance,
                context,
                options,
                cancellationToken);
        }
    }
}

/// <summary>
/// Defines one decision-variable family of the standard
/// lot-sizing formulation.
/// </summary>
/// <remarks>
/// Examples of variable families are production, setup,
/// inventory, backlog, transport, procurement, and resource
/// capacity variables.
/// </remarks>
public interface IStandardLotSizingVariableFamilyBuilder
{
    /// <summary>
    /// Gets the unique identifier of the variable family.
    /// </summary>
    string FamilyId
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
    bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options);

    /// <summary>
    /// Creates and registers all variables belonging to this
    /// family.
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
    ValueTask BuildAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken = default);
}
