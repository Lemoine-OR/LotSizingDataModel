using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspSetupStartVariableFamilyBuilder : StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId=>"glspSetupStart";
    protected override ValueTask BuildFamilyAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);IReadOnlyList<ProductionRouting> routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);var ordered=profile.EnumerateMicroPeriods().ToArray();
        for(int i=0;i<ordered.Length;i++){int from=GlspSequenceSemantics.GetFixedPredecessorItemId(profile,i);bool reset=i>0&&GlspSequenceSemantics.IsResetBoundary(profile,ordered[i-1],ordered[i]);foreach(var routing in routings)AddBinaryVariable(context,$"glspU_r{routing.Id}_t{ordered[i].MacroPeriod}_s{ordered[i].MicroPeriodIndex}",GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(plantId,wc.Id,routing.Id,routing.ItemId,ordered[i],from,reset),"GLSP setup-start occurrence.");}
        return ValueTask.CompletedTask;
    }
}
