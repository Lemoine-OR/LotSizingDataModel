using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Conservative technical applicability contract for the first executable
/// single-resource GLSP formulation.
/// </summary>
/// <remarks>
/// This service is a predicate used during automatic formulation discovery.
/// Normal non-applicability must therefore return false rather than throw.
/// </remarks>
public sealed class GlspSchedulingApplicabilityService
{
    public bool CanBuild(
        LotSizingInstance instance)
    {
        if (
            instance is null ||
            instance.PlanningHorizon <= 0)
        {
            return false;
        }

        if (!GlspSchedulingData.TryGetSchedulingWorkCenter(
                instance,
                out int plantId,
                out var workCenter,
                out ProductionSchedulingProfile? profile) ||
            workCenter is null ||
            profile is null)
        {
            return false;
        }

        if (
            profile.BucketMode !=
                SchedulingBucketMode.MacroMicro ||
            !profile.HasExplicitMicroPeriodGrid ||
            profile.MicroPeriodLengthMode !=
                MicroPeriodLengthMode.Variable ||
            profile.MicroPeriodAssignmentMode !=
                MicroPeriodAssignmentMode.SingleItem ||
            profile.HasInitialSetupState ||
            profile.SetupCarryOverPolicy ==
                SetupCarryOverPolicy.Forbidden ||
            profile.MaximumSetupCount is not null ||
            profile.MaximumProducedItemCount is not null ||
            profile.MicroPeriodCount is null ||
            profile.MicroPeriodCount.PlanningHorizon !=
                instance.PlanningHorizon)
        {
            return false;
        }

        for (
            int period = 1;
            period <= instance.PlanningHorizon;
            period++)
        {
            if (
                profile.MicroPeriodCount.GetCount(period) <=
                0)
            {
                return false;
            }
        }

        if (
            workCenter.CapacityConstraint is null ||
            workCenter.CapacityConstraint.PlanningHorizon !=
                instance.PlanningHorizon ||
            workCenter.AdditionalCapacity is not null)
        {
            return false;
        }

        LotSizingProblemFeatures features;

        try
        {
            features =
                LotSizingProblemFeatureExtractor.Extract(
                    instance.SupplyChain);
        }
        catch (
            InvalidOperationException)
        {
            return false;
        }

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

        if (
            instance.SupplyChain.PeriodicOperatingExpenditureBudget
                is not null)
        {
            return false;
        }

        if (
            instance.SupplyChain.ObjectivePolicy is not null &&
            (
                instance.SupplyChain.ObjectivePolicy.AggregationMode !=
                    ObjectiveAggregationMode.Single ||
                instance.SupplyChain.ObjectivePolicy.PrimaryObjectiveKind !=
                    OptimizationObjectiveKind.Economic
            ))
        {
            return false;
        }

        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(
                instance,
                plantId,
                workCenter.Id);

        if (routings.Count < 2)
        {
            return false;
        }

        if (
            routings
                .GroupBy(
                    routing =>
                        routing.ItemId)
                .Any(
                    group =>
                        group.Count() != 1))
        {
            return false;
        }

        foreach (
            ProductionRouting routing
            in routings)
        {
            if (
                routing.WorkCenters.Count != 1 ||
                routing.WorkCenters[0].PlantId !=
                    plantId ||
                routing.WorkCenters[0].WorkCenterId !=
                    workCenter.Id ||
                routing.GroupingConstraint is not null)
            {
                return false;
            }

            if (!GlspSchedulingData.TryGetCharacteristic(
                    instance,
                    routing,
                    plantId,
                    workCenter.Id,
                    out ProductionCharacteristic? characteristic) ||
                characteristic is null)
            {
                return false;
            }

            if (
                characteristic.UnitCapacityConsumption is null ||
                characteristic.UnitCapacityConsumption.PlanningHorizon !=
                    instance.PlanningHorizon ||
                characteristic.SetupTime is not null ||
                characteristic.FixedSetupCost is not null)
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

        HashSet<int> routedItems =
            routings
                .Select(
                    routing =>
                        routing.ItemId)
                .ToHashSet();

        if (
            profile.Changeovers.Any(
                changeover =>
                    changeover.FromItemId ==
                        changeover.ToItemId ||
                    !routedItems.Contains(
                        changeover.FromItemId) ||
                    !routedItems.Contains(
                        changeover.ToItemId) ||
                    (
                        changeover.ChangeoverTime is not null &&
                        changeover.ChangeoverTime.PlanningHorizon !=
                            instance.PlanningHorizon
                    ) ||
                    (
                        changeover.ChangeoverCost is not null &&
                        changeover.ChangeoverCost.PlanningHorizon !=
                            instance.PlanningHorizon
                    )))
        {
            return false;
        }

        if (
            profile.Changeovers
                .GroupBy(
                    changeover =>
                        (
                            changeover.FromItemId,
                            changeover.ToItemId
                        ))
                .Any(
                    group =>
                        group.Count() > 1))
        {
            return false;
        }

        return true;
    }
}
