using LotSizingDataModel.Core.DecisionModel.Scheduling;
namespace LotSizingDataModel.Solver.Formulation;
internal static class GlspSequenceSemantics
{
    public static bool IsResetBoundary(ProductionSchedulingProfile profile,ProductionMicroPeriodReference previous,ProductionMicroPeriodReference current)
    {
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(previous);ArgumentNullException.ThrowIfNull(current);
        return profile.SetupCarryOverPolicy==SetupCarryOverPolicy.Forbidden && previous.MacroPeriod!=current.MacroPeriod;
    }
    public static int GetFixedPredecessorItemId(ProductionSchedulingProfile profile,int index)
    {
        ArgumentNullException.ThrowIfNull(profile);return index==0&&profile.HasInitialSetupState?profile.InitialSetupItemId:0;
    }
}
