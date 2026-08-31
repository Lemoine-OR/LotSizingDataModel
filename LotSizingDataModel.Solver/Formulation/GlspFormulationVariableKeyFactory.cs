using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
namespace LotSizingDataModel.Solver.Formulation;
internal static class GlspFormulationVariableKeyFactory
{
    public static string CreateMicroProductionKey(int plantId,int workCenterId,int routingId,int itemId,ProductionMicroPeriodReference microPeriod)=>Base(MathematicalDecisionCategory.MicroPeriodProduction,plantId,workCenterId,microPeriod).Add(MathematicalDomainKeySegment.Routing,routingId).Add(MathematicalDomainKeySegment.Item,itemId).Build();
    public static string CreateMicroSetupStateKey(int plantId,int workCenterId,int routingId,int itemId,ProductionMicroPeriodReference microPeriod)=>Base(MathematicalDecisionCategory.MicroPeriodSetupState,plantId,workCenterId,microPeriod).Add(MathematicalDomainKeySegment.Routing,routingId).Add(MathematicalDomainKeySegment.Item,itemId).Build();
    public static string CreateMicroSetupStartKey(int plantId,int workCenterId,int routingId,int itemId,ProductionMicroPeriodReference microPeriod,int fixedPredecessorItemId,bool resetBoundary)
    {
        var b=Base(MathematicalDecisionCategory.AuxiliaryMicroPeriodSetupStart,plantId,workCenterId,microPeriod).Add(MathematicalDomainKeySegment.Routing,routingId).Add(MathematicalDomainKeySegment.Item,itemId);
        if(fixedPredecessorItemId>0)b.Add(MathematicalDomainKeySegment.FromItem,fixedPredecessorItemId);if(resetBoundary)b.Add(MathematicalDomainKeySegment.SetupReset,1);return b.Build();
    }
    public static string CreateChangeoverKey(int plantId,int workCenterId,int fromItemId,int toItemId,ProductionMicroPeriodReference microPeriod)=>Base(MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover,plantId,workCenterId,microPeriod).Add(MathematicalDomainKeySegment.FromItem,fromItemId).Add(MathematicalDomainKeySegment.ToItem,toItemId).Build();
    public static string CreateMacroProductionActivationKey(int routingId,int period)=>new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliaryMacroProductionActivation).Add(MathematicalDomainKeySegment.Routing,routingId).Add(MathematicalDomainKeySegment.Period,period).Build();
    private static MathematicalDomainKeyBuilder Base(string category,int plantId,int workCenterId,ProductionMicroPeriodReference micro)=>new MathematicalDomainKeyBuilder(category).Add(MathematicalDomainKeySegment.Plant,plantId).Add(MathematicalDomainKeySegment.WorkCenter,workCenterId).Add(MathematicalDomainKeySegment.Period,micro.MacroPeriod).Add(MathematicalDomainKeySegment.MicroPeriod,micro.MicroPeriodIndex);
}
