using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspMacroProductionActivationVariableFamilyBuilder : StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId=>"glspMacroProductionActivation";
    public override bool IsEnabled(LotSizingInstance instance,StandardLotSizingFormulationOptions options){ArgumentNullException.ThrowIfNull(instance);ArgumentNullException.ThrowIfNull(options);return instance.SupplyChain.WorkCenters.Any(w=>w.SchedulingProfile?.MaximumProducedItemCount is not null);}
    protected override ValueTask BuildFamilyAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken){var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);foreach(var r in GlspSchedulingData.GetRoutings(instance,plantId,wc.Id))for(int t=1;t<=instance.PlanningHorizon;t++)AddBinaryVariable(context,$"glspMacroProductionActive_r{r.Id}_t{t}",GlspFormulationVariableKeyFactory.CreateMacroProductionActivationKey(r.Id,t),"GLSP positive macro production flag.");return ValueTask.CompletedTask;}
}
