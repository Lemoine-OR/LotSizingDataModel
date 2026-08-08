using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds item-specific inventory setup variables.
/// </summary>
public sealed class InventorySetupVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the inventory-setup variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.InventorySetup;

    /// <summary>
    /// Determines whether inventory setup variables are enabled.
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
            instance.SupplyChain.Inventories.Any(
                inventory =>
                    inventory.FixedSetupCost is not null ||
                    inventory.SetupTime is not null);
    }

    /// <summary>
    /// Builds binary inventory setup variables.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            Inventory inventory
            in instance.SupplyChain.Inventories)
        {
            if (inventory.FixedSetupCost is null &&
                inventory.SetupTime is null)
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
                        MathematicalDecisionCategory.InventorySetup)
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

                AddBinaryVariable(
                    context,
                    $"YI_i{inventory.ItemId}_w" +
                    $"{inventory.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    $"Inventory setup for item {inventory.ItemId} " +
                    $"at warehouse " +
                    $"{inventory.Warehouse.ReferenceId}, " +
                    $"period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
