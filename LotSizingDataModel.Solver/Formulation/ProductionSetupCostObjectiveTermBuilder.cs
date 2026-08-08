using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds fixed production setup costs to the standard objective.
/// </summary>
public sealed class ProductionSetupCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "productionSetupCost";

    /// <summary>
    /// Determines whether production setup costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeProductionSetups;
    }

    /// <summary>
    /// Builds fixed production setup-cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var queries =
            new SupplyChainQueries(
                instance.SupplyChain);

        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double coefficient =
                    0.0;

                foreach (
                    var workCenter
                    in routing.WorkCenters)
                {
                    ProductionCharacteristic characteristic =
                        queries.GetRequiredProductionCharacteristic(
                            routing.ItemId,
                            workCenter);

                    coefficient +=
                        characteristic.FixedSetupCost?[period] ??
                        0.0;
                }

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

                AddCostTerm(
                    context,
                    expressionBuilder,
                    domainKey,
                    coefficient);
            }
        }

        return ValueTask.CompletedTask;
    }
}
