using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspMacroCapacityConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspMacroCapacity";
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);IReadOnlyList<ProductionRouting> routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        for(int t=1;t<=instance.PlanningHorizon;t++)
        {
            var e=new LinearExpressionBuilder();
            for(int i=0;i<ordered.Length;i++)
            {
                var m=ordered[i];if(m.MacroPeriod!=t)continue;int fixedFrom=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);bool reset=i>0&&GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],m);
                foreach(var r in routings){var c=GlspSchedulingData.GetCharacteristic(instance,r,plantId,wc.Id);e.Add(GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroProductionKey(plantId,wc.Id,r.Id,r.ItemId,m)),c.UnitCapacityConsumption![t]);double st=c.SetupTime?[t]??0.0;if(st>0)e.Add(GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(plantId,wc.Id,r.Id,r.ItemId,m,fixedFrom,reset)),st);}
                foreach(var from in routings)foreach(var to in routings){if(from.ItemId==to.ItemId)continue;string key=GlspFormulationVariableKeyFactory.CreateChangeoverKey(plantId,wc.Id,from.ItemId,to.ItemId,m);if(!context.VariableRegistry.TryGet(key,out MathematicalVariable? z)||z is null)continue;double time=GlspSchedulingData.FindChangeover(profile,from.ItemId,to.ItemId)?.ChangeoverTime?[t]??0.0;if(time>0)e.Add(z,time);}
            }
            string additionalKey=new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.WorkCenterAdditionalCapacity).Add(MathematicalDomainKeySegment.Plant,plantId).Add(MathematicalDomainKeySegment.WorkCenter,wc.Id).Add(MathematicalDomainKeySegment.Period,t).Build();if(context.VariableRegistry.TryGet(additionalKey,out MathematicalVariable? additional)&&additional is not null)e.Subtract(additional);
            AddConstraint(context,$"glspMacroCapacity_p{plantId}_w{wc.Id}_t{t}",e.Build(),MathematicalConstraintSense.LessThanOrEqual,wc.CapacityConstraint![t]);
        }
        return ValueTask.CompletedTask;
    }
}
