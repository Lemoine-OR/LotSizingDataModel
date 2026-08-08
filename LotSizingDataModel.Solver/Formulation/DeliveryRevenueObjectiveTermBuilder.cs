using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Subtracts sales revenue from the minimized standard
/// objective.
/// </summary>
public sealed class DeliveryRevenueObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "deliveryRevenue";

    /// <summary>
    /// Builds negative delivered-quantity revenue terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            DistributionCenterSourcing sourcing
            in instance.SupplyChain.DistributionCenterSourcings)
        {
            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Delivery)
                        .Add(
                            MathematicalDomainKeySegment.DistributionCenter,
                            sourcing.DistributionCenterId)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            sourcing.ItemId);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    sourcing.Warehouse);

                AddCostTerm(
                    context,
                    expressionBuilder,
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                    -(sourcing.SellingPrice?[period] ??
                        0.0));
            }
        }

        return ValueTask.CompletedTask;
    }
}
