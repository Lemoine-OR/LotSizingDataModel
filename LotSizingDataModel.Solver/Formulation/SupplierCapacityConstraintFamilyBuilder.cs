using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds procurement-capacity constraints for supplier-delivery relations.
/// </summary>
/// <remarks>
/// alpha.22 capacity is scoped to supplier + item + destination warehouse.
/// It is not an aggregate supplier-wide shared resource.
/// </remarks>
public sealed class SupplierCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "supplierCapacity";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeProcurement &&
            instance.SupplyChain.SupplierDeliveries.Any(
                delivery =>
                    delivery.CapacityConstraint is not null);
    }

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            SupplierDelivery delivery
            in instance.SupplyChain.SupplierDeliveries)
        {
            if (delivery.CapacityConstraint is null)
            {
                continue;
            }

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

                StandardFormulationDomainKeyFactory
                    .AddDestinationWarehouse(
                        keyBuilder,
                        delivery.Warehouse);

                MathematicalVariable procurement =
                    context.GetVariable(
                        keyBuilder
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build());

                double capacity =
                    delivery.CapacityConstraint
                        .GetMaximumCapacity(period);

                AddConstraint(
                    context,
                    $"supplierCapacity_f{delivery.SupplierId}" +
                    $"_i{delivery.ItemId}" +
                    $"_w{delivery.Warehouse.ReferenceId}" +
                    $"_t{period}",
                    new LinearExpressionBuilder()
                        .Add(procurement)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    capacity,
                    description:
                        "Maximum procurement quantity for one supplier-" +
                        "item-destination relationship.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
