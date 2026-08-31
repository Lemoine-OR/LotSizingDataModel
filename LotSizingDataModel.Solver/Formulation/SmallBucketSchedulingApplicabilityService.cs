using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketSchedulingApplicabilityService
{
    public bool CanBuild(LotSizingInstance instance,SmallBucketSchedulingFormulationKind kind)
    {
        if(instance is null || instance.PlanningHorizon<=0) return false;
        var entries=instance.SupplyChain.Plants.SelectMany(p=>p.WorkCenters.Select(w=>(PlantId:p.Id,WorkCenter:w))).Where(e=>e.WorkCenter.SchedulingProfile is not null).Take(2).ToArray();
        if(entries.Length!=1) return false;
        int plantId=entries[0].PlantId; WorkCenter wc=entries[0].WorkCenter; ProductionSchedulingProfile profile=wc.SchedulingProfile!;
        if(profile.BucketMode!=SchedulingBucketMode.SmallBucket || profile.MaximumProducedItemCount is null || profile.MaximumProducedItemCount.PlanningHorizon!=instance.PlanningHorizon || profile.HasSequenceDependentChangeoverTimes || profile.HasSequenceDependentChangeoverCosts || profile.Changeovers.Count>0) return false;
        if(kind==SmallBucketSchedulingFormulationKind.Plsp && profile.SetupCarryOverPolicy==SetupCarryOverPolicy.Forbidden) return false;
        if(!ValidCore(profile,instance.PlanningHorizon,kind)) return false;
        if(wc.CapacityConstraint is null || wc.CapacityConstraint.PlanningHorizon!=instance.PlanningHorizon) return false;
        if(wc.AdditionalCapacity is not null && (kind==SmallBucketSchedulingFormulationKind.Dlsp || wc.AdditionalCapacity.PlanningHorizon!=instance.PlanningHorizon)) return false;
        LotSizingProblemFeatures features; try{features=LotSizingProblemFeatureExtractor.Extract(instance.SupplyChain);}catch(InvalidOperationException){return false;}
        if(features.HasStartUpCosts || features.HasStartUpTimes || features.HasMinimumLotSizes || features.HasMaximumLotSizes || features.HasLotSizeMultiples || features.IsMultiSite) return false;
        if(kind==SmallBucketSchedulingFormulationKind.Dlsp && (features.HasSetupTimes || features.HasAdditionalProductionCapacity)) return false;
        var routings=instance.SupplyChain.ProductionRoutings.ToArray(); if(routings.Length==0 || routings.GroupBy(r=>r.ItemId).Any(g=>g.Count()!=1)) return false;
        var itemIds=routings.Select(r=>r.ItemId).ToHashSet(); if(profile.HasInitialSetupState && !itemIds.Contains(profile.InitialSetupItemId)) return false;
        if(profile.MaximumSetupCount is not null && profile.MaximumSetupCount.PlanningHorizon!=instance.PlanningHorizon) return false;
        foreach(ProductionRouting routing in routings)
        {
            if(routing.WorkCenters.Count!=1 || routing.WorkCenters[0].PlantId!=plantId || routing.WorkCenters[0].WorkCenterId!=wc.Id) return false;
            if(routing.GroupingConstraint is not null && routing.GroupingConstraint.PlanningHorizon!=instance.PlanningHorizon) return false;
            var matches=instance.SupplyChain.ProductionCharacteristics.Where(c=>c.ItemId==routing.ItemId && c.WorkCenter.PlantId==plantId && c.WorkCenter.WorkCenterId==wc.Id).Take(2).ToArray();
            if(matches.Length!=1 || matches[0].UnitCapacityConsumption is null || matches[0].UnitCapacityConsumption.PlanningHorizon!=instance.PlanningHorizon) return false;
            var c=matches[0]; if(c.SetupTime is not null && c.SetupTime.PlanningHorizon!=instance.PlanningHorizon) return false; if(c.FixedSetupCost is not null && c.FixedSetupCost.PlanningHorizon!=instance.PlanningHorizon) return false;
            for(int t=1;t<=instance.PlanningHorizon;t++){double cap=wc.CapacityConstraint[t], cons=c.UnitCapacityConsumption[t];if(!double.IsFinite(cap)||cap<0||!double.IsFinite(cons)||cons<=0)return false;}
        }
        return true;
    }
    private static bool ValidCore(ProductionSchedulingProfile profile,int horizon,SmallBucketSchedulingFormulationKind kind)
    {
        var required=kind==SmallBucketSchedulingFormulationKind.Dlsp?SmallBucketProductionMode.AllOrNothing:SmallBucketProductionMode.Continuous; if(profile.SmallBucketProductionMode!=required)return false;
        int max=kind==SmallBucketSchedulingFormulationKind.Plsp?2:1;
        for(int t=1;t<=horizon;t++){int c=profile.MaximumProducedItemCount!.GetCount(t);if(c<0||c>max)return false;}
        if(kind!=SmallBucketSchedulingFormulationKind.Plsp)return true;
        if(profile.MaximumSetupCount is null)return false;
        for(int t=1;t<=horizon;t++){int c=profile.MaximumSetupCount.GetCount(t);if(c<0||c>1)return false;if(t==1&&c==0&&!profile.HasInitialSetupState)return false;}
        return true;
    }
}
