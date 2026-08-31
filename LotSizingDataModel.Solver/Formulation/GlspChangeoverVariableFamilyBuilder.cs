using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspChangeoverVariableFamilyBuilder : StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId=>"glspChangeover";
    protected override ValueTask BuildFamilyAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);IReadOnlyList<ProductionRouting> routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        for(int i=0;i<ordered.Length;i++)
        {
            var current=ordered[i];int fixedFrom=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);
            if(fixedFrom>0){foreach(var to in routings){if(to.ItemId==fixedFrom)continue;AddBinaryVariable(context,$"glspZ_i{fixedFrom}_j{to.ItemId}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}",GlspFormulationVariableKeyFactory.CreateChangeoverKey(plantId,wc.Id,fixedFrom,to.ItemId,current),"GLSP initial-state changeover.");}continue;}
            if(i==0||GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],current))continue;
            foreach(var from in routings)foreach(var to in routings){if(from.ItemId==to.ItemId)continue;AddBinaryVariable(context,$"glspZ_i{from.ItemId}_j{to.ItemId}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}",GlspFormulationVariableKeyFactory.CreateChangeoverKey(plantId,wc.Id,from.ItemId,to.ItemId,current),"GLSP adjacent-state changeover.");}
        }
        return ValueTask.CompletedTask;
    }
}
