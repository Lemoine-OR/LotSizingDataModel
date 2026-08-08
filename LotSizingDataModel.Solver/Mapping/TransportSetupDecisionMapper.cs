using System;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical item-specific transport setup values to
/// transport decisions in a lot-sizing solution.
/// </summary>
/// <remarks>
/// Expected canonical domain-key format:
/// <code>
/// transportSetup|item=&lt;id&gt;|transportResource=&lt;id&gt;|originWarehouse=&lt;id&gt;|destinationWarehouse=&lt;id&gt;|period=&lt;index&gt;
/// </code>
/// Origin and destination may alternatively be identified by
/// <c>originPlant</c> and <c>destinationPlant</c>.
/// Period numbers are one-based.
/// </remarks>
public sealed class TransportSetupDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical domain-key category handled by this
    /// mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.TransportSetup;

    /// <summary>
    /// Maps one non-zero transport setup value.
    /// </summary>
    /// <param name="context">
    /// Mathematical-solution mapping context.
    /// </param>
    /// <param name="solution">
    /// Target lot-sizing solution.
    /// </param>
    /// <param name="domainKey">
    /// Parsed transport-setup domain key.
    /// </param>
    /// <param name="variableValue">
    /// Setup value returned by the solver.
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
                        existing.Matches(
                            itemId,
                            transportResourceId,
                            origin,
                            destination));

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

        decision.SetSetupActivated(
            period,
            variableValue.Value >
                ZeroTolerance);
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
                $"A transport-setup domain key must identify " +
                $"exactly one {role} warehouse using either " +
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
