using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspSetupStartDefinitionConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspSetupStartDefinition";
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);IReadOnlyList<ProductionRouting> routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        for(int i=0;i<ordered.Length;i++)
        {
            var current=ordered[i];int fixedFrom=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);bool reset=i>0&&GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],current);
            foreach(var routing in routings)
            {
                var y=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(plantId,wc.Id,routing.Id,routing.ItemId,current));var u=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(plantId,wc.Id,routing.Id,routing.ItemId,current,fixedFrom,reset));string suffix=$"_r{routing.Id}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}";
                if(fixedFrom>0){if(fixedFrom==routing.ItemId)AddConstraint(context,"glspSetupStartInitialSame"+suffix,new LinearExpressionBuilder().Add(u).Build(),MathematicalConstraintSense.Equal,0.0);else AddConstraint(context,"glspSetupStartInitial"+suffix,new LinearExpressionBuilder().Add(u).Subtract(y).Build(),MathematicalConstraintSense.Equal,0.0);continue;}
                if(i==0||reset){AddConstraint(context,"glspSetupStartReset"+suffix,new LinearExpressionBuilder().Add(u).Subtract(y).Build(),MathematicalConstraintSense.Equal,0.0);continue;}
                var prev=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(plantId,wc.Id,routing.Id,routing.ItemId,ordered[i-1]));
                AddConstraint(context,"glspSetupStartLower"+suffix,new LinearExpressionBuilder().Add(u).Subtract(y).Add(prev).Build(),MathematicalConstraintSense.GreaterThanOrEqual,0.0);
                AddConstraint(context,"glspSetupStartUpperCurrent"+suffix,new LinearExpressionBuilder().Add(u).Subtract(y).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);
                AddConstraint(context,"glspSetupStartUpperPrevious"+suffix,new LinearExpressionBuilder().Add(u).Add(prev).Build(),MathematicalConstraintSense.LessThanOrEqual,1.0);
            }
        }
        return ValueTask.CompletedTask;
    }
}
