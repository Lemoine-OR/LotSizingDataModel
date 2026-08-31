using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification;
namespace LotSizingDataModel.Solver.Formulation;
public sealed class GlspSchedulingApplicabilityService
{
    public bool CanBuild(LotSizingInstance instance)
    {
        if(instance is null||instance.PlanningHorizon<=0)return false;
        if(!GlspSchedulingData.TryGetSchedulingWorkCenter(instance,out int plantId,out var wc,out ProductionSchedulingProfile? profile)||wc is null||profile is null)return false;
        if(profile.BucketMode!=SchedulingBucketMode.MacroMicro||!profile.HasExplicitMicroPeriodGrid||profile.MicroPeriodLengthMode!=MicroPeriodLengthMode.Variable||profile.MicroPeriodAssignmentMode!=MicroPeriodAssignmentMode.SingleItem||profile.MicroPeriodCount is null||profile.MicroPeriodCount.PlanningHorizon!=instance.PlanningHorizon)return false;
        for(int t=1;t<=instance.PlanningHorizon;t++)if(profile.MicroPeriodCount.GetCount(t)<=0)return false;
        if(profile.MaximumSetupCount is not null&&profile.MaximumSetupCount.PlanningHorizon!=instance.PlanningHorizon)return false;if(profile.MaximumProducedItemCount is not null&&profile.MaximumProducedItemCount.PlanningHorizon!=instance.PlanningHorizon)return false;
        if(wc.CapacityConstraint is null||wc.CapacityConstraint.PlanningHorizon!=instance.PlanningHorizon)return false;if(wc.AdditionalCapacity is not null&&wc.AdditionalCapacity.PlanningHorizon!=instance.PlanningHorizon)return false;
        LotSizingProblemFeatures features;try{features=LotSizingProblemFeatureExtractor.Extract(instance.SupplyChain);}catch(InvalidOperationException){return false;}
        if(features.HasStartUpCosts||features.HasStartUpTimes||features.HasMinimumLotSizes||features.HasMaximumLotSizes||features.HasLotSizeMultiples||features.IsMultiSite)return false;
        if(instance.SupplyChain.PeriodicOperatingExpenditureBudget is not null)return false;if(instance.SupplyChain.ObjectivePolicy is not null&&(instance.SupplyChain.ObjectivePolicy.AggregationMode!=ObjectiveAggregationMode.Single||instance.SupplyChain.ObjectivePolicy.PrimaryObjectiveKind!=OptimizationObjectiveKind.Economic))return false;
        var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);if(routings.Count<2||routings.GroupBy(r=>r.ItemId).Any(g=>g.Count()!=1))return false;var itemIds=routings.Select(r=>r.ItemId).ToHashSet();if(profile.HasInitialSetupState&&!itemIds.Contains(profile.InitialSetupItemId))return false;
        foreach(ProductionRouting r in routings){if(r.WorkCenters.Count!=1||r.WorkCenters[0].PlantId!=plantId||r.WorkCenters[0].WorkCenterId!=wc.Id)return false;if(r.GroupingConstraint is not null&&r.GroupingConstraint.PlanningHorizon!=instance.PlanningHorizon)return false;if(!GlspSchedulingData.TryGetCharacteristic(instance,r,plantId,wc.Id,out ProductionCharacteristic? c)||c is null||c.UnitCapacityConsumption is null||c.UnitCapacityConsumption.PlanningHorizon!=instance.PlanningHorizon)return false;if(c.SetupTime is not null&&c.SetupTime.PlanningHorizon!=instance.PlanningHorizon)return false;if(c.FixedSetupCost is not null&&c.FixedSetupCost.PlanningHorizon!=instance.PlanningHorizon)return false;for(int t=1;t<=instance.PlanningHorizon;t++){double cap=wc.CapacityConstraint[t],cons=c.UnitCapacityConsumption[t];if(!double.IsFinite(cap)||cap<0||!double.IsFinite(cons)||cons<=0)return false;}}
        if(profile.Changeovers.Any(ch=>ch.FromItemId==ch.ToItemId||!itemIds.Contains(ch.FromItemId)||!itemIds.Contains(ch.ToItemId)||(ch.ChangeoverTime is not null&&ch.ChangeoverTime.PlanningHorizon!=instance.PlanningHorizon)||(ch.ChangeoverCost is not null&&ch.ChangeoverCost.PlanningHorizon!=instance.PlanningHorizon)))return false;if(profile.Changeovers.GroupBy(ch=>(ch.FromItemId,ch.ToItemId)).Any(g=>g.Count()>1))return false;return true;
    }
}
