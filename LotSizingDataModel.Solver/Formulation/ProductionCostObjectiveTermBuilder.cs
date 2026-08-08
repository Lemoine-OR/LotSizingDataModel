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
/// Adds variable production costs to the standard objective.
/// </summary>
public sealed class ProductionCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "productionCost";

    /// <summary>
    /// Builds variable production-cost terms.
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
                        characteristic.UnitUsageCost?[period] ??
                        0.0;
                }

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
