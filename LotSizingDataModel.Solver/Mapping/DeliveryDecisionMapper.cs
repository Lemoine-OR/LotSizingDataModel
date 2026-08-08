using System;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical delivery-variable values to distribution
/// decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Supported canonical domain-key formats are:
/// <code>
/// delivery|distributionCenter=&lt;id&gt;|item=&lt;id&gt;|warehouse=&lt;id&gt;|period=&lt;index&gt;
/// delivery|distributionCenter=&lt;id&gt;|item=&lt;id&gt;|plant=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// The first format identifies a standalone warehouse. The
/// second identifies the warehouse attached to a plant.
/// Period numbers are one-based.
/// </remarks>
public sealed class DeliveryDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.Delivery;

    /// <summary>
    /// Maps one non-zero delivered quantity.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed delivery domain key.
    /// </param>
    /// <param name="variableValue">
    /// Delivered quantity returned by the solver.
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

        int distributionCenterId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.DistributionCenter);

        int itemId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        WarehouseReference warehouse =
            ResolveWarehouse(
                domainKey);

        DistributionDecision? decision =
            solution.DistributionDecisions
                .FirstOrDefault(
                    existing =>
                        existing.Matches(
                            distributionCenterId,
                            itemId,
                            warehouse));

        if (decision is null)
        {
            if (solution.PlanningHorizon <= 0)
            {
                throw new InvalidOperationException(
                    "The target lot-sizing solution must have a " +
                    "strictly positive planning horizon before " +
                    "distribution decisions are mapped.");
            }

            decision =
                new DistributionDecision(
                    distributionCenterId,
                    itemId,
                    warehouse,
                    solution.PlanningHorizon);

            solution.AddDistributionDecision(
                decision);
        }

        decision.SetDeliveredQuantity(
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
                "A delivery domain key must identify exactly one " +
                "warehouse using either 'warehouse' or 'plant'.");
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
