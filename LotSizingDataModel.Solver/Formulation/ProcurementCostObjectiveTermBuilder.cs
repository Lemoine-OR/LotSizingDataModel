using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds supplier purchase costs to the standard objective.
/// </summary>
public sealed class ProcurementCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "procurementCost";

    /// <summary>
    /// Determines whether procurement costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeProcurement;
    }

    /// <summary>
    /// Builds supplier purchase-cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            SupplierDelivery delivery
            in instance.SupplyChain.SupplierDeliveries)
        {
            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Procurement)
                        .Add(
                            MathematicalDomainKeySegment.Supplier,
                            delivery.SupplierId)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            delivery.ItemId);

                StandardFormulationDomainKeyFactory.AddDestinationWarehouse(
                    keyBuilder,
                    delivery.Warehouse);

                AddCostTerm(
                    context,
                    expressionBuilder,
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                    delivery.PurchasePrice?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
