using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds delivered-quantity variables from configured
/// distribution-center sourcing relationships.
/// </summary>
public sealed class DeliveryVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the delivery variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Delivery;

    /// <summary>
    /// Determines whether delivery variables are required.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        return
            instance.SupplyChain
                .DistributionCenterSourcings.Count > 0;
    }

    /// <summary>
    /// Builds delivered-quantity variables.
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
            DistributionCenterSourcing sourcing
            in instance.SupplyChain.DistributionCenterSourcings)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (
                int period = 1;
                period <= planningHorizon;
                period++)
            {
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

                string domainKey =
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"D_c{sourcing.DistributionCenterId}" +
                    $"_i{sourcing.ItemId}_w" +
                    $"{sourcing.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    double.PositiveInfinity,
                    $"Delivered quantity to distribution center " +
                    $"{sourcing.DistributionCenterId} for item " +
                    $"{sourcing.ItemId} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
