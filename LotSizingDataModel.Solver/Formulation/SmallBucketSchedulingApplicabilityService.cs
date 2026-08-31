using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketSchedulingApplicabilityService
{
    public bool CanBuild(
        LotSizingInstance instance,
        SmallBucketSchedulingFormulationKind kind)
    {
        if(instance is null || instance.PlanningHorizon<=0) return false;

        var entries=
            instance.SupplyChain.Plants
                .SelectMany(
                    plant =>
                        plant.WorkCenters.Select(
                            workCenter =>
                                (PlantId:plant.Id,WorkCenter:workCenter)))
                .Where(entry=>entry.WorkCenter.SchedulingProfile is not null)
                .Take(2)
                .ToArray();

        if(entries.Length!=1) return false;

        int plantId=entries[0].PlantId;
        WorkCenter wc=entries[0].WorkCenter;
        ProductionSchedulingProfile profile=wc.SchedulingProfile!;

        if(profile.BucketMode!=SchedulingBucketMode.SmallBucket ||
           profile.MaximumProducedItemCount is null ||
           profile.MaximumProducedItemCount.PlanningHorizon!=instance.PlanningHorizon ||
           profile.HasSequenceDependentChangeoverTimes ||
           profile.HasSequenceDependentChangeoverCosts ||
           profile.Changeovers.Count>0) return false;

        if(kind==SmallBucketSchedulingFormulationKind.Plsp &&
           profile.SetupCarryOverPolicy==SetupCarryOverPolicy.Forbidden) return false;

        if(!ValidCore(profile,instance.PlanningHorizon,kind)) return false;

        var capacityConstraint=wc.CapacityConstraint;
        if(capacityConstraint is null ||
           capacityConstraint.PlanningHorizon!=instance.PlanningHorizon) return false;

        if(wc.AdditionalCapacity is not null &&
           (kind==SmallBucketSchedulingFormulationKind.Dlsp ||
            wc.AdditionalCapacity.PlanningHorizon!=instance.PlanningHorizon)) return false;

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

        // DLSP all-or-nothing production has no proven exact residual-capacity
        // reformulation for setup/start-up time in this implementation.
        if(kind==SmallBucketSchedulingFormulationKind.Dlsp &&
           (features.HasSetupTimes ||
            features.HasStartUpTimes ||
            features.HasAdditionalProductionCapacity)) return false;

        var routings=instance.SupplyChain.ProductionRoutings.ToArray();
        if(routings.Length==0 ||
           routings.GroupBy(routing=>routing.ItemId).Any(group=>group.Count()!=1)) return false;

        var itemIds=routings.Select(routing=>routing.ItemId).ToHashSet();
        if(profile.HasInitialSetupState &&
           !itemIds.Contains(profile.InitialSetupItemId)) return false;

        if(profile.MaximumSetupCount is not null &&
           profile.MaximumSetupCount.PlanningHorizon!=instance.PlanningHorizon) return false;

        foreach(ProductionRouting routing in routings)
        {
            if(routing.WorkCenters.Count!=1 ||
               routing.WorkCenters[0].PlantId!=plantId ||
               routing.WorkCenters[0].WorkCenterId!=wc.Id) return false;

            if(routing.GroupingConstraint is not null &&
               routing.GroupingConstraint.PlanningHorizon!=instance.PlanningHorizon) return false;

            var matches=
                instance.SupplyChain.ProductionCharacteristics
                    .Where(
                        characteristic =>
                            characteristic.ItemId==routing.ItemId &&
                            characteristic.WorkCenter.PlantId==plantId &&
                            characteristic.WorkCenter.WorkCenterId==wc.Id)
                    .Take(2)
                    .ToArray();

            if(matches.Length!=1) return false;

            var characteristic=matches[0];
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

            for(int period=1;
                period<=instance.PlanningHorizon;
                period++)
            {
                double cap=capacityConstraint[period];
                double consumption=unitCapacityConsumption[period];

                if(!double.IsFinite(cap) ||
                   cap<0 ||
                   !double.IsFinite(consumption) ||
                   consumption<=0) return false;

                double startUpTime=characteristic.StartUpTime?[period]??0.0;
                double startUpCost=characteristic.StartUpCost?[period]??0.0;

                if(!double.IsFinite(startUpTime) ||
                   startUpTime<0 ||
                   !double.IsFinite(startUpCost) ||
                   startUpCost<0) return false;
            }
        }

        return true;
    }

    private static bool ValidCore(
        ProductionSchedulingProfile profile,
        int horizon,
        SmallBucketSchedulingFormulationKind kind)
    {
        var required=
            kind==SmallBucketSchedulingFormulationKind.Dlsp
                ? SmallBucketProductionMode.AllOrNothing
                : SmallBucketProductionMode.Continuous;

        if(profile.SmallBucketProductionMode!=required) return false;

        int max=
            kind==SmallBucketSchedulingFormulationKind.Plsp
                ? 2
                : 1;

        for(int period=1;period<=horizon;period++)
        {
            int count=profile.MaximumProducedItemCount!.GetCount(period);
            if(count<0||count>max) return false;
        }

        if(kind!=SmallBucketSchedulingFormulationKind.Plsp) return true;
        if(profile.MaximumSetupCount is null) return false;

        for(int period=1;period<=horizon;period++)
        {
            int count=profile.MaximumSetupCount.GetCount(period);
            if(count<0||count>1) return false;
            if(period==1&&count==0&&!profile.HasInitialSetupState) return false;
        }

        return true;
    }
}
