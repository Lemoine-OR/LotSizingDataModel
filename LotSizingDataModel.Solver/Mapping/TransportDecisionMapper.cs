using System;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical transport-variable values to transport
/// decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Expected canonical domain-key format:
/// <code>
/// transport|item=&lt;id&gt;|transportResource=&lt;id&gt;|originWarehouse=&lt;id&gt;|destinationWarehouse=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Origin and destination may alternatively be identified by
/// <c>originPlant</c> and <c>destinationPlant</c> when the
/// corresponding warehouses are attached to plants.
/// </remarks>
public sealed class TransportDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.Transport;

    /// <summary>
    /// Maps one non-zero transport quantity.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed transport domain key.
    /// </param>
    /// <param name="variableValue">
    /// Transport quantity returned by the solver.
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

        int transportResourceId =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.TransportResource);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        WarehouseReference origin =
            ResolveWarehouse(
                domainKey,
                MathematicalDomainKeySegment.OriginWarehouse,
                MathematicalDomainKeySegment.OriginPlant,
                "origin");

        WarehouseReference destination =
            ResolveWarehouse(
                domainKey,
                MathematicalDomainKeySegment.DestinationWarehouse,
                MathematicalDomainKeySegment.DestinationPlant,
                "destination");

        TransportDecision? decision =
            solution.TransportDecisions
                .FirstOrDefault(
                    existing =>
                        existing.ItemId ==
                            itemId &&
                        existing.TransportResourceId ==
                            transportResourceId &&
                        existing.Origin.Kind ==
                            origin.Kind &&
                        existing.Origin.ReferenceId ==
                            origin.ReferenceId &&
                        existing.Destination.Kind ==
                            destination.Kind &&
                        existing.Destination.ReferenceId ==
                            destination.ReferenceId);

        if (decision is null)
        {
            if (solution.PlanningHorizon <= 0)
            {
                throw new InvalidOperationException(
                    "The target lot-sizing solution must have a " +
                    "strictly positive planning horizon before " +
                    "transport decisions are mapped.");
            }

            decision =
                new TransportDecision(
                    itemId,
                    transportResourceId,
                    origin,
                    destination,
                    solution.PlanningHorizon);

            solution.AddTransportDecision(
                decision);
        }

        decision.SetTransportedQuantity(
            period,
            variableValue.Value);
    }

    private static WarehouseReference ResolveWarehouse(
        MathematicalDomainKey domainKey,
        string warehouseSegment,
        string plantSegment,
        string role)
    {
        bool hasStandaloneWarehouse =
            domainKey.TryGetInt32(
                warehouseSegment,
                out int warehouseId);

        bool hasPlantWarehouse =
            domainKey.TryGetInt32(
                plantSegment,
                out int plantId);

        if (hasStandaloneWarehouse ==
            hasPlantWarehouse)
        {
            throw new InvalidOperationException(
                $"A transport domain key must identify exactly " +
                $"one {role} warehouse using either " +
                $"'{warehouseSegment}' or '{plantSegment}'.");
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
