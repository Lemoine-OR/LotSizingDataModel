using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds the production-setup variable family of the standard
/// lot-sizing formulation.
/// </summary>
/// <remarks>
/// One binary setup variable is created for every production
/// routing and planning period when production setups are
/// enabled.
/// The canonical domain key is:
/// <code>
/// setup|routing=&lt;routingId&gt;|period=&lt;period&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class SetupVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique identifier of the production-setup
    /// variable family.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Setup;

    /// <summary>
    /// Determines whether production-setup variables are enabled
    /// for the supplied instance.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when production setups are enabled
    /// and at least one production routing exists; otherwise,
    /// <see langword="false"/>.
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
            options.IncludeProductionSetups &&
            instance.SupplyChain.ProductionRoutings.Count > 0;
    }

    /// <summary>
    /// Builds the production-setup variables.
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
    /// Task representing setup-variable construction.
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
                "Production-setup variables cannot be built for " +
                "an instance with a non-positive planning " +
                "horizon.");
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
                        MathematicalDecisionCategory.Setup)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                string variableName =
                    $"y_r{routing.Id}_t{period}";

                AddBinaryVariable(
                    context,
                    variableName,
                    domainKey,
                    $"Production setup activation for routing " +
                    $"{routing.Id} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
