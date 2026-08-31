using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspChangeoverCostObjectiveTermBuilder : StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId=>"glspChangeoverCost";
    protected override ValueTask BuildTermsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,LinearExpressionBuilder expressionBuilder,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);
        foreach(var m in profile.EnumerateMicroPeriods())foreach(var from in routings)foreach(var to in routings){if(from.ItemId==to.ItemId)continue;string key=GlspFormulationVariableKeyFactory.CreateChangeoverKey(plantId,wc.Id,from.ItemId,to.ItemId,m);if(!context.VariableRegistry.TryGet(key,out MathematicalVariable? variable)||variable is null)continue;double cost=GlspSchedulingData.FindChangeover(profile,from.ItemId,to.ItemId)?.ChangeoverCost?[m.MacroPeriod]??0.0;if(cost!=0)AddCostTerm(context,expressionBuilder,key,cost);}
        return ValueTask.CompletedTask;
    }
}
