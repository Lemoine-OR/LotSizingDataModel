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
/// Builds item-specific additional inventory-capacity
/// variables.
/// </summary>
public sealed class InventoryAdditionalCapacityVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the inventory additional-capacity variable-family
    /// identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.InventoryAdditionalCapacity;

    /// <summary>
    /// Determines whether additional inventory capacity is
    /// enabled.
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
            options.IncludeAdditionalCapacity &&
            instance.SupplyChain.Inventories.Any(
                inventory =>
                    inventory.AdditionalCapacity is not null);
    }

    /// <summary>
    /// Builds item-specific additional inventory-capacity
    /// variables.
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
            if (inventory.AdditionalCapacity is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double upperBound =
                    inventory.AdditionalCapacity[period];

                if (IsStructurallyZero(
                        upperBound,
                        options))
                {
                    continue;
                }

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .InventoryAdditionalCapacity)
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
                    $"OI_i{inventory.ItemId}_w" +
                    $"{inventory.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    upperBound,
                    $"Additional inventory capacity for item " +
                    $"{inventory.ItemId} at warehouse " +
                    $"{inventory.Warehouse.ReferenceId}, " +
                    $"period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
