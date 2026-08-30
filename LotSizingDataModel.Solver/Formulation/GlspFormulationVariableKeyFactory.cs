using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

internal static class GlspFormulationVariableKeyFactory
{
    public static string CreateMicroProductionKey(
        int plantId,
        int workCenterId,
        int routingId,
        int itemId,
        ProductionMicroPeriodReference microPeriod)
    {
        ArgumentNullException.ThrowIfNull(microPeriod);
        return BaseMicroKey(MathematicalDecisionCategory.MicroPeriodProduction, plantId, workCenterId, microPeriod)
            .Add(MathematicalDomainKeySegment.Routing, routingId)
            .Add(MathematicalDomainKeySegment.Item, itemId)
            .Build();
    }

    public static string CreateMicroSetupStateKey(
        int plantId,
        int workCenterId,
        int routingId,
        int itemId,
        ProductionMicroPeriodReference microPeriod)
    {
        ArgumentNullException.ThrowIfNull(microPeriod);
        return BaseMicroKey(MathematicalDecisionCategory.MicroPeriodSetupState, plantId, workCenterId, microPeriod)
            .Add(MathematicalDomainKeySegment.Routing, routingId)
            .Add(MathematicalDomainKeySegment.Item, itemId)
            .Build();
    }

    public static string CreateChangeoverKey(
        int plantId,
        int workCenterId,
        int fromItemId,
        int toItemId,
        ProductionMicroPeriodReference microPeriod)
    {
        ArgumentNullException.ThrowIfNull(microPeriod);
        return BaseMicroKey(MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover, plantId, workCenterId, microPeriod)
            .Add(MathematicalDomainKeySegment.FromItem, fromItemId)
            .Add(MathematicalDomainKeySegment.ToItem, toItemId)
            .Build();
    }

    private static MathematicalDomainKeyBuilder BaseMicroKey(
        string category,
        int plantId,
        int workCenterId,
        ProductionMicroPeriodReference microPeriod) =>
            new MathematicalDomainKeyBuilder(category)
                .Add(MathematicalDomainKeySegment.Plant, plantId)
                .Add(MathematicalDomainKeySegment.WorkCenter, workCenterId)
                .Add(MathematicalDomainKeySegment.Period, microPeriod.MacroPeriod)
                .Add(MathematicalDomainKeySegment.MicroPeriod, microPeriod.MicroPeriodIndex);
}
