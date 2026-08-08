using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds all constraint families of the standard lot-sizing
/// formulation.
/// </summary>
public sealed class StandardLotSizingConstraintBuilder :
    IStandardLotSizingConstraintBuilder
{
    private readonly IReadOnlyList<IStandardLotSizingConstraintFamilyBuilder>
        _familyBuilders;

    /// <summary>
    /// Initializes the standard constraint builder.
    /// </summary>
    /// <param name="familyBuilders">
    /// Ordered constraint-family builders.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="familyBuilders"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection contains a null builder or
    /// duplicate family identifiers.
    /// </exception>
    public StandardLotSizingConstraintBuilder(
        IEnumerable<IStandardLotSizingConstraintFamilyBuilder> familyBuilders)
    {
        ArgumentNullException.ThrowIfNull(
            familyBuilders);

        IStandardLotSizingConstraintFamilyBuilder[] builders =
            familyBuilders.ToArray();

        if (builders.Any(
                builder =>
                    builder is null))
        {
            throw new InvalidOperationException(
                "The standard constraint-family builder " +
                "collection cannot contain a null entry.");
        }

        string[] duplicateIds =
            builders
                .GroupBy(
                    builder =>
                        builder.ConstraintFamilyId,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new InvalidOperationException(
                "Duplicate standard constraint-family " +
                "identifiers were registered: " +
                string.Join(
                    ", ",
                    duplicateIds));
        }

        _familyBuilders =
            builders;
    }

    /// <summary>
    /// Gets the ordered constraint-family builders.
    /// </summary>
    public IReadOnlyList<IStandardLotSizingConstraintFamilyBuilder>
        FamilyBuilders =>
            _familyBuilders;

    /// <summary>
    /// Builds all enabled standard constraint families.
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

        options.EnsureValid();

        foreach (
            IStandardLotSizingConstraintFamilyBuilder familyBuilder
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
