using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspChangeoverDefinitionConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspChangeoverDefinition";
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);IReadOnlyList<ProductionRouting> routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        for(int i=0;i<ordered.Length;i++)
        {
            var current=ordered[i];int fixedFrom=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);
            if(fixedFrom>0){foreach(var to in routings){if(to.ItemId==fixedFrom)continue;var z=GetVariable(context,GlspFormulationVariableKeyFactory.CreateChangeoverKey(plantId,wc.Id,fixedFrom,to.ItemId,current));var y=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(plantId,wc.Id,to.Id,to.ItemId,current));AddConstraint(context,$"glspInitialChangeover_i{fixedFrom}_j{to.ItemId}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}",new LinearExpressionBuilder().Add(z).Subtract(y).Build(),MathematicalConstraintSense.Equal,0.0);}continue;}
            if(i==0||GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],current))continue;
            foreach(var from in routings)foreach(var to in routings)
            {
                if(from.ItemId==to.ItemId)continue;var z=GetVariable(context,GlspFormulationVariableKeyFactory.CreateChangeoverKey(plantId,wc.Id,from.ItemId,to.ItemId,current));var yp=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(plantId,wc.Id,from.Id,from.ItemId,ordered[i-1]));var yc=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(plantId,wc.Id,to.Id,to.ItemId,current));string suffix=$"_i{from.ItemId}_j{to.ItemId}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}";
                AddConstraint(context,"glspChangeoverLower"+suffix,new LinearExpressionBuilder().Add(z).Subtract(yp).Subtract(yc).Build(),MathematicalConstraintSense.GreaterThanOrEqual,-1.0);
                AddConstraint(context,"glspChangeoverUpperPrevious"+suffix,new LinearExpressionBuilder().Add(z).Subtract(yp).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);
                AddConstraint(context,"glspChangeoverUpperCurrent"+suffix,new LinearExpressionBuilder().Add(z).Subtract(yc).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);
            }
        }
        return ValueTask.CompletedTask;
    }
}
