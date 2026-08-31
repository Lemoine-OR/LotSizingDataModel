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
        if(instance is null||instance.PlanningHorizon<=0) return false;

        if(!GlspSchedulingData.TryGetSchedulingWorkCenter(
                instance,
                out int plantId,
                out var wc,
                out ProductionSchedulingProfile? profile) ||
           wc is null ||
           profile is null) return false;

        if(profile.BucketMode!=SchedulingBucketMode.MacroMicro ||
           !profile.HasExplicitMicroPeriodGrid ||
           profile.MicroPeriodLengthMode!=MicroPeriodLengthMode.Variable ||
           profile.MicroPeriodAssignmentMode!=MicroPeriodAssignmentMode.SingleItem ||
           profile.MicroPeriodCount is null ||
           profile.MicroPeriodCount.PlanningHorizon!=instance.PlanningHorizon) return false;

        for(int period=1;period<=instance.PlanningHorizon;period++)
        {
            if(profile.MicroPeriodCount.GetCount(period)<=0) return false;
        }

        if(profile.MaximumSetupCount is not null &&
           profile.MaximumSetupCount.PlanningHorizon!=instance.PlanningHorizon) return false;

        if(profile.MaximumProducedItemCount is not null &&
           profile.MaximumProducedItemCount.PlanningHorizon!=instance.PlanningHorizon) return false;

        var capacityConstraint=wc.CapacityConstraint;
        if(capacityConstraint is null ||
           capacityConstraint.PlanningHorizon!=instance.PlanningHorizon) return false;

        if(wc.AdditionalCapacity is not null &&
           wc.AdditionalCapacity.PlanningHorizon!=instance.PlanningHorizon) return false;

        LotSizingProblemFeatures features;
        try
        {
            features=LotSizingProblemFeatureExtractor.Extract(instance.SupplyChain);
        }
        catch(InvalidOperationException)
        {
            return false;
        }

        if(features.HasMinimumLotSizes ||
           features.HasMaximumLotSizes ||
           features.HasLotSizeMultiples ||
           features.IsMultiSite) return false;

        if(instance.SupplyChain.PeriodicOperatingExpenditureBudget is not null) return false;

        if(instance.SupplyChain.ObjectivePolicy is not null &&
           (instance.SupplyChain.ObjectivePolicy.AggregationMode!=ObjectiveAggregationMode.Single ||
            instance.SupplyChain.ObjectivePolicy.PrimaryObjectiveKind!=OptimizationObjectiveKind.Economic)) return false;

        var routings=GlspSchedulingData.GetRoutings(instance,plantId,wc.Id);
        if(routings.Count<2 ||
           routings.GroupBy(routing=>routing.ItemId).Any(group=>group.Count()!=1)) return false;

        var itemIds=routings.Select(routing=>routing.ItemId).ToHashSet();

        if(profile.HasInitialSetupState &&
           !itemIds.Contains(profile.InitialSetupItemId)) return false;

        foreach(ProductionRouting routing in routings)
        {
            if(routing.WorkCenters.Count!=1 ||
               routing.WorkCenters[0].PlantId!=plantId ||
               routing.WorkCenters[0].WorkCenterId!=wc.Id) return false;

            if(routing.GroupingConstraint is not null &&
               routing.GroupingConstraint.PlanningHorizon!=instance.PlanningHorizon) return false;

            if(!GlspSchedulingData.TryGetCharacteristic(
                    instance,
                    routing,
                    plantId,
                    wc.Id,
                    out ProductionCharacteristic? characteristic) ||
               characteristic is null) return false;

            var unitCapacityConsumption=characteristic.UnitCapacityConsumption;
            if(unitCapacityConsumption is null ||
               unitCapacityConsumption.PlanningHorizon!=instance.PlanningHorizon) return false;

            if(characteristic.SetupTime is not null &&
               characteristic.SetupTime.PlanningHorizon!=instance.PlanningHorizon) return false;

            if(characteristic.FixedSetupCost is not null &&
               characteristic.FixedSetupCost.PlanningHorizon!=instance.PlanningHorizon) return false;

            if(characteristic.StartUpTime is not null &&
               characteristic.StartUpTime.PlanningHorizon!=instance.PlanningHorizon) return false;

            if(characteristic.StartUpCost is not null &&
               characteristic.StartUpCost.PlanningHorizon!=instance.PlanningHorizon) return false;

            for(int period=1;period<=instance.PlanningHorizon;period++)
            {
                double cap=capacityConstraint[period];
                double consumption=unitCapacityConsumption[period];
                double startUpTime=characteristic.StartUpTime?[period]??0.0;
                double startUpCost=characteristic.StartUpCost?[period]??0.0;

                if(!double.IsFinite(cap) ||
                   cap<0 ||
                   !double.IsFinite(consumption) ||
                   consumption<=0 ||
                   !double.IsFinite(startUpTime) ||
                   startUpTime<0 ||
                   !double.IsFinite(startUpCost) ||
                   startUpCost<0) return false;
            }
        }

        if(profile.Changeovers.Any(
                changeover =>
                    changeover.FromItemId==changeover.ToItemId ||
                    !itemIds.Contains(changeover.FromItemId) ||
                    !itemIds.Contains(changeover.ToItemId) ||
                    (changeover.ChangeoverTime is not null &&
                     changeover.ChangeoverTime.PlanningHorizon!=instance.PlanningHorizon) ||
                    (changeover.ChangeoverCost is not null &&
                     changeover.ChangeoverCost.PlanningHorizon!=instance.PlanningHorizon))) return false;

        if(profile.Changeovers
            .GroupBy(changeover=>(changeover.FromItemId,changeover.ToItemId))
            .Any(group=>group.Count()>1)) return false;

        return true;
    }
}
