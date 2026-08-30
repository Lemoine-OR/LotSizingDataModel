using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Conservative technical applicability contract for executable DLSP, CSLP
/// and PLSP formulations.
/// </summary>
public sealed class SmallBucketSchedulingApplicabilityService
{
    public bool CanBuild(
        LotSizingInstance instance,
        SmallBucketSchedulingFormulationKind kind)
    {
        if (
            instance is null ||
            instance.PlanningHorizon <= 0)
        {
            return false;
        }

        var schedulingWorkCenters =
            instance.SupplyChain.WorkCenters
                .Where(
                    workCenter =>
                        workCenter.SchedulingProfile is not null)
                .ToArray();

        if (schedulingWorkCenters.Length != 1)
        {
            return false;
        }

        WorkCenter workCenter =
            schedulingWorkCenters[0];

        ProductionSchedulingProfile profile =
            workCenter.SchedulingProfile!;

        if (
            profile.BucketMode !=
                SchedulingBucketMode.SmallBucket ||
            profile.MaximumProducedItemCount is null ||
            profile.HasInitialSetupState ||
            profile.HasSequenceDependentChangeoverTimes ||
            profile.HasSequenceDependentChangeoverCosts ||
            profile.SetupCarryOverPolicy ==
                SetupCarryOverPolicy.Forbidden ||
            profile.Changeovers.Count > 0)
        {
            return false;
        }

        if (!HasValidCanonicalSchedulingSemantics(
                profile,
                instance.PlanningHorizon,
                kind))
        {
            return false;
        }

        if (
            workCenter.CapacityConstraint is null ||
            workCenter.AdditionalCapacity is not null)
        {
            return false;
        }

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(
                instance.SupplyChain);

        if (
            features.HasSetupTimes ||
            features.HasStartUpCosts ||
            features.HasStartUpTimes ||
            features.HasMinimumLotSizes ||
            features.HasMaximumLotSizes ||
            features.HasLotSizeMultiples ||
            features.HasAdditionalProductionCapacity ||
            features.IsMultiSite)
        {
            return false;
        }

        ProductionRouting[] routings =
            instance.SupplyChain.ProductionRoutings
                .ToArray();

        if (routings.Length == 0)
        {
            return false;
        }

        if (
            routings
                .GroupBy(routing => routing.ItemId)
                .Any(group => group.Count() != 1))
        {
            return false;
        }

        int plantId =
            GetPlantId(
                instance,
                workCenter);

        foreach (ProductionRouting routing in routings)
        {
            if (
                routing.WorkCenters.Count != 1 ||
                routing.WorkCenters[0].PlantId != plantId ||
                routing.WorkCenters[0].WorkCenterId !=
                    workCenter.Id ||
                routing.GroupingConstraint is not null)
            {
                return false;
            }

            ProductionCharacteristic? characteristic =
                instance.SupplyChain.ProductionCharacteristics
                    .SingleOrDefault(
                        candidate =>
                            candidate.ItemId == routing.ItemId &&
                            candidate.WorkCenter.PlantId ==
                                routing.WorkCenters[0].PlantId &&
                            candidate.WorkCenter.WorkCenterId ==
                                workCenter.Id);

            if (
                characteristic?.UnitCapacityConsumption is null)
            {
                return false;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                double capacity =
                    workCenter.CapacityConstraint[period];

                double consumption =
                    characteristic
                        .UnitCapacityConsumption[period];

                if (
                    !double.IsFinite(capacity) ||
                    capacity < 0.0 ||
                    !double.IsFinite(consumption) ||
                    consumption <= 0.0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool HasValidCanonicalSchedulingSemantics(
        ProductionSchedulingProfile profile,
        int planningHorizon,
        SmallBucketSchedulingFormulationKind kind)
    {
        SmallBucketProductionMode requiredMode =
            kind ==
                SmallBucketSchedulingFormulationKind.Dlsp
                ? SmallBucketProductionMode.AllOrNothing
                : SmallBucketProductionMode.Continuous;

        if (
            profile.SmallBucketProductionMode !=
                requiredMode)
        {
            return false;
        }

        int maximumProducedItems =
            kind ==
                SmallBucketSchedulingFormulationKind.Plsp
                ? 2
                : 1;

        for (
            int period = 1;
            period <= planningHorizon;
            period++)
        {
            int count =
                profile.MaximumProducedItemCount!
                    .GetCount(period);

            if (
                count < 0 ||
                count > maximumProducedItems)
            {
                return false;
            }
        }

        if (
            kind !=
            SmallBucketSchedulingFormulationKind.Plsp)
        {
            return
                profile.MaximumSetupCount is null;
        }

        if (profile.MaximumSetupCount is null)
        {
            return false;
        }

        for (
            int period = 1;
            period <= planningHorizon;
            period++)
        {
            int count =
                profile.MaximumSetupCount.GetCount(period);

            if (count is < 0 or > 1)
            {
                return false;
            }

            // No initial setup state is currently executable. PLSP requires
            // one end-of-period setup state, so period 1 must allow the
            // initial setup operation.
            if (
                period == 1 &&
                count == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static int GetPlantId(
        LotSizingInstance instance,
        WorkCenter workCenter)
    {
        return instance.SupplyChain.Plants
            .Single(
                plant =>
                    plant.WorkCenters.Any(
                        candidate =>
                            ReferenceEquals(
                                candidate,
                                workCenter)))
            .Id;
    }
}
