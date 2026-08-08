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
/// Builds soft safety-stock violation variables.
/// </summary>
public sealed class InventorySafetyStockViolationVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the safety-stock violation variable-family
    /// identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.InventorySafetyStockViolation;

    /// <summary>
    /// Determines whether safety-stock violation variables are
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
            options.IncludeSafetyStock &&
            options.IncludeSafetyStockViolation &&
            instance.SupplyChain.Inventories.Any(
                inventory =>
                    inventory.SafetyStock is not null &&
                    inventory.SafetyStockViolationCost is not null);
    }

    /// <summary>
    /// Builds non-negative safety-stock violation variables.
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
            if (inventory.SafetyStock is null ||
                inventory.SafetyStockViolationCost is null)
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
                        MathematicalDecisionCategory
                            .InventorySafetyStockViolation)
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
                    $"VSS_i{inventory.ItemId}_w" +
                    $"{inventory.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    double.PositiveInfinity,
                    $"Safety-stock violation for item " +
                    $"{inventory.ItemId} at warehouse " +
                    $"{inventory.Warehouse.ReferenceId}, " +
                    $"period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
