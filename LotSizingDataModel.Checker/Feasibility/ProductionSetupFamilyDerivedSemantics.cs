using System;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Feasibility;

public static class ProductionSetupFamilyDerivedSemantics
{
    public static bool IsFamilySetupActivated(
        SupplyChain supplyChain,
        LotSizingSolution solution,
        ProductionSetupFamily family,
        int period)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(family);

        int[] routingIds =
            supplyChain.ProductionRoutings
                .Where(routing =>
                    family.MemberItemIds.Contains(routing.ItemId) &&
                    routing.WorkCenters.Any(reference =>
                        reference.PlantId ==
                            family.WorkCenter.PlantId &&
                        reference.WorkCenterId ==
                            family.WorkCenter.WorkCenterId))
                .Select(routing => routing.Id)
                .ToArray();

        return solution.ProductionDecisions.Any(
            decision =>
                routingIds.Contains(decision.RoutingId) &&
                decision.IsSetupActivated(period));
    }

    public static double GetFamilySetupCapacityConsumption(
        SupplyChain supplyChain,
        LotSizingSolution solution,
        ProductionSetupFamily family,
        int period)
    {
        return IsFamilySetupActivated(
                supplyChain,
                solution,
                family,
                period)
            ? family.SetupTime?[period] ?? 0.0
            : 0.0;
    }
}
