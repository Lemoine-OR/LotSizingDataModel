using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspGroupingConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspSetupGrouping";
    public override bool IsEnabled(LotSizingInstance instance,StandardLotSizingFormulationOptions options){ArgumentNullException.ThrowIfNull(instance);ArgumentNullException.ThrowIfNull(options);return instance.SupplyChain.ProductionRoutings.Any(r=>r.GroupingConstraint is not null);}
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        foreach(var r in routings){if(r.GroupingConstraint is null)continue;for(int t=1;t<=instance.PlanningHorizon;t++){int g=r.GroupingConstraint.GetGroupingPeriodCount(t);int last=Math.Min(instance.PlanningHorizon,t+g-1);for(int k=t+1;k<=last;k++){var first=ordered.Select((m,i)=>(m,i)).Where(x=>x.m.MacroPeriod==t).ToArray();var later=ordered.Select((m,i)=>(m,i)).Where(x=>x.m.MacroPeriod==k).ToArray();foreach(var a in first)foreach(var b in later)AddConstraint(context,$"glspGrouping_r{r.Id}_t{t}_s{a.m.MicroPeriodIndex}_k{k}_v{b.m.MicroPeriodIndex}",new LinearExpressionBuilder().Add(Start(context,profile,ordered,a.i,r,plantId,wc.Id)).Add(Start(context,profile,ordered,b.i,r,plantId,wc.Id)).Build(),MathematicalConstraintSense.LessThanOrEqual,1.0);}}}
        return ValueTask.CompletedTask;
    }
    private static MathematicalVariable Start(MathematicalModelBuildContext context,ProductionSchedulingProfile profile,ProductionMicroPeriodReference[] ordered,int i,ProductionRouting r,int plantId,int wcId){int from=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);bool reset=i>0&&GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],ordered[i]);return context.GetVariable(GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(plantId,wcId,r.Id,r.ItemId,ordered[i],from,reset));}
}
