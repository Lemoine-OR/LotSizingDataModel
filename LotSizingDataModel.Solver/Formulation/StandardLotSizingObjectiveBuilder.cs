using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds the complete linear objective of the standard
/// lot-sizing formulation from registered objective-term
/// families.
/// </summary>
public sealed class StandardLotSizingObjectiveBuilder :
    IStandardLotSizingObjectiveBuilder
{
    private readonly IReadOnlyList<IStandardLotSizingObjectiveTermBuilder>
        _termBuilders;

    /// <summary>
    /// Initializes the standard objective builder.
    /// </summary>
    /// <param name="termBuilders">
    /// Ordered objective-term builders.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="termBuilders"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection contains a null builder or
    /// duplicate term-family identifiers.
    /// </exception>
    public StandardLotSizingObjectiveBuilder(
        IEnumerable<IStandardLotSizingObjectiveTermBuilder> termBuilders)
    {
        ArgumentNullException.ThrowIfNull(
            termBuilders);

        IStandardLotSizingObjectiveTermBuilder[] builders =
            termBuilders.ToArray();

        if (builders.Any(
                builder =>
                    builder is null))
        {
            throw new InvalidOperationException(
                "The standard objective-term builder collection " +
                "cannot contain a null entry.");
        }

        string[] duplicateIds =
            builders
                .GroupBy(
                    builder =>
                        builder.TermFamilyId,
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
                "Duplicate standard objective-term identifiers " +
                "were registered: " +
                string.Join(
                    ", ",
                    duplicateIds));
        }

        _termBuilders =
            builders;
    }

    /// <summary>
    /// Gets the ordered objective-term builders.
    /// </summary>
    public IReadOnlyList<IStandardLotSizingObjectiveTermBuilder>
        TermBuilders =>
            _termBuilders;

    /// <summary>
    /// Builds and assigns the complete minimization objective.
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
    /// Token used to cancel objective construction.
    /// </param>
    /// <returns>
    /// Task representing objective construction.
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

        var expressionBuilder =
            new LinearExpressionBuilder();

        foreach (
            IStandardLotSizingObjectiveTermBuilder termBuilder
            in _termBuilders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!termBuilder.IsEnabled(
                    instance,
                    options))
            {
                continue;
            }

            await termBuilder.BuildAsync(
                instance,
                context,
                expressionBuilder,
                options,
                cancellationToken);
        }

        var objective =
            new MathematicalObjective(
                "totalCost",
                ObjectiveSense.Minimize,
                expressionBuilder.Build())
            {
                Description =
                    "Total cost of the standard lot-sizing " +
                    "formulation."
            };

        context.SetObjective(
            objective);
    }
}
