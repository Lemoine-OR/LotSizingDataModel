using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspSetupCountConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspMaximumSetupCount";
    public override bool IsEnabled(LotSizingInstance instance,StandardLotSizingFormulationOptions options){ArgumentNullException.ThrowIfNull(instance);ArgumentNullException.ThrowIfNull(options);return instance.SupplyChain.WorkCenters.Any(w=>w.SchedulingProfile?.MaximumSetupCount is not null);}
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);MaximumSetupCount limit=profile.MaximumSetupCount??throw new InvalidOperationException("MaximumSetupCount required.");IReadOnlyList<ProductionRouting> routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        for(int t=1;t<=instance.PlanningHorizon;t++){var e=new LinearExpressionBuilder();for(int i=0;i<ordered.Length;i++){if(ordered[i].MacroPeriod!=t)continue;int from=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);bool reset=i>0&&GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],ordered[i]);foreach(var r in routings)e.Add(GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(plantId,wc.Id,r.Id,r.ItemId,ordered[i],from,reset)));}AddConstraint(context,$"glspMaximumSetupCount_t{t}",e.Build(),MathematicalConstraintSense.LessThanOrEqual,limit.GetCount(t));}
        return ValueTask.CompletedTask;
    }
}
