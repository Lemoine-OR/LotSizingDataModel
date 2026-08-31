using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspSetupStartCostObjectiveTermBuilder : StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId=>"glspSetupStartCost";
    protected override ValueTask BuildTermsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,LinearExpressionBuilder expressionBuilder,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken){var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();for(int i=0;i<ordered.Length;i++){int from=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);bool reset=i>0&&GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],ordered[i]);foreach(var r in routings){var c=GlspSchedulingData.GetCharacteristic(instance,r,plantId,wc.Id);double cost=c.FixedSetupCost?[ordered[i].MacroPeriod]??0.0;if(cost!=0)AddCostTerm(context,expressionBuilder,GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(plantId,wc.Id,r.Id,r.ItemId,ordered[i],from,reset),cost);}}return ValueTask.CompletedTask;}
}
