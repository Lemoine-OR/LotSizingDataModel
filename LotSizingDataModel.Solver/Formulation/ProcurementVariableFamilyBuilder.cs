using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds supplier-procurement quantity variables.
/// </summary>
public sealed class ProcurementVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the procurement variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Procurement;

    /// <summary>
    /// Determines whether procurement variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeProcurement &&
            instance.SupplyChain.SupplierDeliveries.Count > 0;
    }

    /// <summary>
    /// Builds procurement variables for each supplier delivery
    /// relationship and period.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        int planningHorizon =
            instance.PlanningHorizon;

        foreach (
            SupplierDelivery delivery
            in instance.SupplyChain.SupplierDeliveries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (delivery.SupplierId <= 0 ||
                delivery.ItemId <= 0)
            {
                throw new InvalidOperationException(
                    "Supplier deliveries must identify strictly " +
                    "positive supplier and item identifiers.");
            }

            for (
                int period = 1;
                period <= planningHorizon;
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

                string domainKey =
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"P_f{delivery.SupplierId}_i{delivery.ItemId}" +
                    $"_w{delivery.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    double.PositiveInfinity,
                    $"Purchased quantity from supplier " +
                    $"{delivery.SupplierId} for item " +
                    $"{delivery.ItemId} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
