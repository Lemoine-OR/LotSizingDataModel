using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps a mathematical period-zero inventory value to the normalized solution.
/// </summary>
public sealed class InitialInventoryDecisionMapper :
    MathematicalDecisionMapperBase
{
    public override string Category =>
        MathematicalDecisionCategory.InitialInventory;

    protected override void MapValue(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        MathematicalDomainKey domainKey,
        MathematicalVariableValue variableValue)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(domainKey);
        ArgumentNullException.ThrowIfNull(variableValue);

        int itemId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        WarehouseReference warehouse =
            ResolveWarehouse(domainKey);

        InventoryDecision? decision =
            solution.InventoryDecisions.FirstOrDefault(
                existing =>
                    existing.ItemId == itemId &&
                    existing.Warehouse.Kind == warehouse.Kind &&
                    existing.Warehouse.ReferenceId == warehouse.ReferenceId);

        if (decision is null)
        {
            decision =
                new InventoryDecision(
                    itemId,
                    warehouse,
                    solution.PlanningHorizon);

            solution.AddInventoryDecision(decision);
        }

        decision.InitialInventoryLevel =
            variableValue.Value;
    }

    private static WarehouseReference ResolveWarehouse(
        MathematicalDomainKey domainKey)
    {
        bool hasStandalone =
            domainKey.TryGetInt32(
                MathematicalDomainKeySegment.Warehouse,
                out int warehouseId);

        bool hasPlant =
            domainKey.TryGetInt32(
                MathematicalDomainKeySegment.Plant,
                out int plantId);

        if (hasStandalone == hasPlant)
        {
            throw new InvalidOperationException(
                "An initial-inventory key must identify exactly one warehouse.");
        }

        return hasStandalone
            ? WarehouseReference.ForStandaloneWarehouse(warehouseId)
            : WarehouseReference.ForPlantWarehouse(plantId);
    }
}
