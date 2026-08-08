using System;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical procurement-variable values to purchase
/// decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Supported canonical domain-key formats are:
/// <code>
/// procurement|supplier=&lt;id&gt;|item=&lt;id&gt;|destinationWarehouse=&lt;id&gt;|period=&lt;index&gt;
/// procurement|supplier=&lt;id&gt;|item=&lt;id&gt;|destinationPlant=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Period numbers are one-based.
/// </remarks>
public sealed class ProcurementDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.Procurement;

    /// <summary>
    /// Maps one non-zero purchased quantity.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed procurement domain key.
    /// </param>
    /// <param name="variableValue">
    /// Purchased quantity returned by the solver.
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

        int supplierId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Supplier);

        int itemId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        WarehouseReference destinationWarehouse =
            ResolveDestinationWarehouse(
                domainKey);

        PurchaseDecision? decision =
            solution.PurchaseDecisions
                .FirstOrDefault(
                    existing =>
                        existing.Matches(
                            supplierId,
                            itemId,
                            destinationWarehouse));

        if (decision is null)
        {
            if (solution.PlanningHorizon <= 0)
            {
                throw new InvalidOperationException(
                    "The target lot-sizing solution must have a " +
                    "strictly positive planning horizon before " +
                    "purchase decisions are mapped.");
            }

            decision =
                new PurchaseDecision(
                    supplierId,
                    itemId,
                    destinationWarehouse,
                    solution.PlanningHorizon);

            solution.AddPurchaseDecision(
                decision);
        }

        decision.SetPurchasedQuantity(
            period,
            variableValue.Value);
    }

    private static WarehouseReference ResolveDestinationWarehouse(
        MathematicalDomainKey domainKey)
    {
        bool hasStandaloneWarehouse =
            domainKey.TryGetInt32(
                MathematicalDomainKeySegment.DestinationWarehouse,
                out int warehouseId);

        bool hasPlantWarehouse =
            domainKey.TryGetInt32(
                MathematicalDomainKeySegment.DestinationPlant,
                out int plantId);

        if (hasStandaloneWarehouse ==
            hasPlantWarehouse)
        {
            throw new InvalidOperationException(
                "A procurement domain key must identify exactly " +
                "one destination warehouse using either " +
                "'destinationWarehouse' or 'destinationPlant'.");
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
