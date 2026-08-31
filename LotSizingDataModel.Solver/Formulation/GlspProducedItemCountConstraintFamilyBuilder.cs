using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspProducedItemCountConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId=>"glspProducedItemCount";
    public override bool IsEnabled(LotSizingInstance instance,StandardLotSizingFormulationOptions options){ArgumentNullException.ThrowIfNull(instance);ArgumentNullException.ThrowIfNull(options);return instance.SupplyChain.WorkCenters.Any(w=>w.SchedulingProfile?.MaximumProducedItemCount is not null);}
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var (plantId,wc,profile)=GlspSchedulingData.GetSchedulingWorkCenter(instance);MaximumProducedItemCount limit=profile.MaximumProducedItemCount??throw new InvalidOperationException("MaximumProducedItemCount required.");var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);
        for(int t=1;t<=instance.PlanningHorizon;t++){var count=new LinearExpressionBuilder();foreach(var r in routings){var c=GlspSchedulingData.GetCharacteristic(instance,r,plantId,wc.Id);double m=wc.CapacityConstraint![t]+(wc.AdditionalCapacity?[t]??0.0);var q=GetVariable(context,GlspFormulationVariableKeyFactory.CreateMacroProductionActivationKey(r.Id,t));AddConstraint(context,$"glspMacroProductionActivation_r{r.Id}_t{t}",new LinearExpressionBuilder().Add(GetVariable(context,StandardFormulationVariableKeyFactory.CreateProductionKey(r.Id,t)),c.UnitCapacityConsumption![t]).Subtract(q,m).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);count.Add(q);}AddConstraint(context,$"glspProducedItemCount_t{t}",count.Build(),MathematicalConstraintSense.LessThanOrEqual,limit.GetCount(t));}
        return ValueTask.CompletedTask;
    }
}
