using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

public static class SchedulingVariableKeyFactory
{
    public static string SetupState(
        int plantId,
        int workCenterId,
        int itemId,
        int period,
        int microPeriod)
    {
        return new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.MicroPeriodSetupState)
            .Add(MathematicalDomainKeySegment.Plant, plantId)
            .Add(MathematicalDomainKeySegment.WorkCenter, workCenterId)
            .Add(MathematicalDomainKeySegment.Item, itemId)
            .Add(MathematicalDomainKeySegment.Period, period)
            .Add(MathematicalDomainKeySegment.MicroPeriod, microPeriod)
            .Build();
    }

    public static string Changeover(
        int plantId,
        int workCenterId,
        int fromItemId,
        int toItemId,
        int period,
        int microPeriod)
    {
        return new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover)
            .Add(MathematicalDomainKeySegment.Plant, plantId)
            .Add(MathematicalDomainKeySegment.WorkCenter, workCenterId)
            .Add(MathematicalDomainKeySegment.FromItem, fromItemId)
            .Add(MathematicalDomainKeySegment.ToItem, toItemId)
            .Add(MathematicalDomainKeySegment.Period, period)
            .Add(MathematicalDomainKeySegment.MicroPeriod, microPeriod)
            .Build();
    }
}
