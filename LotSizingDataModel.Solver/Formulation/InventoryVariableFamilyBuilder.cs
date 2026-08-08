using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds inventory-level variables for every configured
/// item-warehouse inventory relationship.
/// </summary>
public sealed class InventoryVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the inventory variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Inventory;

    /// <summary>
    /// Determines whether inventory variables are required.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        return instance.SupplyChain.Inventories.Count > 0;
    }

    /// <summary>
    /// Builds inventory-level variables.
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
            Inventory inventory
            in instance.SupplyChain.Inventories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (inventory.ItemId <= 0)
            {
                throw new InvalidOperationException(
                    "Every inventory relationship must identify " +
                    "a strictly positive item identifier.");
            }

            for (
                int period = 1;
                period <= planningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Inventory)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            inventory.ItemId);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    inventory.Warehouse);

                string domainKey =
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"I_i{inventory.ItemId}_w" +
                    $"{inventory.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    double.PositiveInfinity,
                    $"Inventory level for item {inventory.ItemId} " +
                    $"at warehouse {inventory.Warehouse.ReferenceId} " +
                    $"in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
