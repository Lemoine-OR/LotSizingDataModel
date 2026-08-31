using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspMicroProductionLinkConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspMicroProductionLink";
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken){var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);foreach(var m in profile.EnumerateMicroPeriods()){double cap=wc.CapacityConstraint![m.MacroPeriod]+(wc.AdditionalCapacity?[m.MacroPeriod]??0.0);foreach(var r in routings){var c=GlspSchedulingData.GetCharacteristic(instance,r,plantId,wc.Id);AddConstraint(context,$"glspProductionLink_r{r.Id}_t{m.MacroPeriod}_s{m.MicroPeriodIndex}",new LinearExpressionBuilder().Add(GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroProductionKey(plantId,wc.Id,r.Id,r.ItemId,m)),c.UnitCapacityConsumption![m.MacroPeriod]).Subtract(GetVariable(context,GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(plantId,wc.Id,r.Id,r.ItemId,m)),cap).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);}}return ValueTask.CompletedTask;}
}
