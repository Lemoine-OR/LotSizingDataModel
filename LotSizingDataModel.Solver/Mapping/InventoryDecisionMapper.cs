using System;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical inventory-variable values to inventory
/// decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Supported canonical domain-key formats are:
/// <code>
/// inventory|item=&lt;id&gt;|warehouse=&lt;id&gt;|period=&lt;index&gt;
/// inventory|item=&lt;id&gt;|plant=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class InventoryDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.Inventory;

    /// <summary>
    /// Maps one non-zero inventory value.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed inventory domain key.
    /// </param>
    /// <param name="variableValue">
    /// Inventory value returned by the solver.
    /// </param>
    protected override void MapValue(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        MathematicalDomainKey domainKey,
        MathematicalVariableValue variableValue)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            solution);

        ArgumentNullException.ThrowIfNull(
            domainKey);

        ArgumentNullException.ThrowIfNull(
            variableValue);

        int itemId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        WarehouseReference warehouse =
            ResolveWarehouse(
                domainKey);

        InventoryDecision? decision =
            solution.InventoryDecisions
                .FirstOrDefault(
                    existing =>
                        existing.ItemId ==
                            itemId &&
                        existing.Warehouse.Kind ==
                            warehouse.Kind &&
                        existing.Warehouse.ReferenceId ==
                            warehouse.ReferenceId);

        if (decision is null)
        {
            decision =
                new InventoryDecision(
                    itemId,
                    warehouse,
                    solution.PlanningHorizon);

            solution.AddInventoryDecision(
                decision);
        }

        decision.SetLevel(
            period,
            variableValue.Value);
    }

    private static WarehouseReference ResolveWarehouse(
        MathematicalDomainKey domainKey)
    {
        bool hasStandaloneWarehouse =
            domainKey.TryGetInt32(
                MathematicalDomainKeySegment.Warehouse,
                out int warehouseId);

        bool hasPlantWarehouse =
            domainKey.TryGetInt32(
                MathematicalDomainKeySegment.Plant,
                out int plantId);

        if (hasStandaloneWarehouse ==
            hasPlantWarehouse)
        {
            throw new InvalidOperationException(
                "An inventory domain key must identify exactly " +
                "one warehouse using either 'warehouse' or " +
                "'plant'.");
        }

        if (hasStandaloneWarehouse)
        {
            return WarehouseReference
                .ForStandaloneWarehouse(
                    warehouseId);
        }

        return WarehouseReference
            .ForPlantWarehouse(
                plantId);
    }
}
