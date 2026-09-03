using System;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Feasibility;

public static class SchedulingTransitionChecker
{
    public static bool IsTransitionSemanticallyValid(
        SupplyChain supplyChain,
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(solution);

        foreach (var plant in supplyChain.Plants)
        {
            foreach (var workCenter in plant.WorkCenters)
            {
                ProductionSetupTransitionProfile? transitions =
                    workCenter.SetupTransitionProfile;

                if (transitions is null)
                {
                    continue;
                }

                var schedule =
                    solution.WorkCenterSchedulingDecisions
                        .FirstOrDefault(decision =>
                            decision.WorkCenter.PlantId == plant.Id &&
                            decision.WorkCenter.WorkCenterId ==
                                workCenter.Id);

                if (schedule is null)
                {
                    continue;
                }

                if (transitions.CarryOverPolicy ==
                    SetupCarryOverPolicy.Forbidden)
                {
                    continue;
                }

                if (!schedule.IsInternallyValid)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
