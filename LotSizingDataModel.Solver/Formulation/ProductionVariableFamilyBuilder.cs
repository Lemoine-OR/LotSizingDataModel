using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds the production-quantity variable family of the
/// standard lot-sizing formulation.
/// </summary>
/// <remarks>
/// One non-negative continuous production variable is created
/// for every production routing and planning period.
/// The canonical domain key is:
/// <code>
/// production|routing=&lt;routingId&gt;|period=&lt;period&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class ProductionVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique identifier of the production variable
    /// family.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Production;

    /// <summary>
    /// Determines whether production variables are required for
    /// the supplied instance.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when at least one production
    /// routing exists; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        return
            instance.SupplyChain.ProductionRoutings.Count > 0;
    }

    /// <summary>
    /// Builds the production-quantity variables.
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
    /// Token used to cancel variable construction.
    /// </param>
    /// <returns>
    /// Task representing production-variable construction.
    /// </returns>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        int planningHorizon =
            instance.PlanningHorizon;

        if (planningHorizon <= 0)
        {
            throw new InvalidOperationException(
                "Production variables cannot be built for an " +
                "instance with a non-positive planning horizon.");
        }

        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (routing.Id <= 0)
            {
                throw new InvalidOperationException(
                    "Every production routing must have a " +
                    "strictly positive identifier before the " +
                    "mathematical formulation is built.");
            }

            for (
                int period = 1;
                period <= planningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Production)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                string variableName =
                    $"x_r{routing.Id}_t{period}";

                AddNonNegativeContinuousVariable(
                    context,
                    variableName,
                    domainKey,
                    double.PositiveInfinity,
                    $"Production quantity for routing " +
                    $"{routing.Id} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
