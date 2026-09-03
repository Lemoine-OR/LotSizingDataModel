using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Prevents the standard formulation from silently ignoring
/// setup-transition semantics that have no executable scheduling profile.
/// </summary>
public static class SetupTransitionExecutionGuard
{
    public static bool HasUnsupportedSemantics(
        SupplyChain supplyChain)
    {
        return supplyChain.Plants
            .SelectMany(plant => plant.WorkCenters)
            .Any(workCenter =>
                workCenter.SetupTransitionProfile is not null &&
                workCenter.SchedulingProfile is null &&
                (
                    workCenter.SetupTransitionProfile.CarryOverPolicy ==
                        SetupCarryOverPolicy.Allowed ||
                    workCenter.SetupTransitionProfile.Changeovers.Count > 0
                ));
    }
}
