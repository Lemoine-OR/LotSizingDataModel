using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Checker.Structural;

public sealed class MacroMicroSchedulingStructuralValidator
{
    public IReadOnlyList<SolutionCheckIssue> Validate(
        LotSizingInstance instance,
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(solution);
        var issues = new List<SolutionCheckIssue>();

        var expected = instance.SupplyChain.Plants
            .SelectMany(plant => plant.WorkCenters
                .Where(workCenter => workCenter.SchedulingProfile?.BucketMode == SchedulingBucketMode.MacroMicro)
                .Select(workCenter => (PlantId: plant.Id, WorkCenter: workCenter)))
            .ToArray();

        foreach (var entry in expected)
        {
            WorkCenterSchedulingDecision? schedule =
                solution.WorkCenterSchedulingDecisions.SingleOrDefault(candidate =>
                    candidate.WorkCenter.PlantId == entry.PlantId &&
                    candidate.WorkCenter.WorkCenterId == entry.WorkCenter.Id);

            if (schedule is null)
            {
                issues.Add(Error(
                    $"DECSCHED005: Missing macro/micro schedule for Plant:{entry.PlantId}/WorkCenter:{entry.WorkCenter.Id}."));
                continue;
            }

            ProductionSchedulingProfile profile = entry.WorkCenter.SchedulingProfile!;
            foreach (ProductionMicroPeriodReference microPeriod in profile.EnumerateMicroPeriods())
            {
                if (!schedule.MicroPeriods.Any(decision =>
                        decision.MicroPeriod.RefersToSameMicroPeriod(microPeriod)))
                {
                    issues.Add(Error(
                        $"DECSCHED006: Missing decision for micro-period {microPeriod} on {schedule.WorkCenter}."));
                }
            }
        }

        foreach (WorkCenterSchedulingDecision schedule in solution.WorkCenterSchedulingDecisions)
        {
            WorkCenter? workCenter = instance.SupplyChain.Plants
                .Where(plant => plant.Id == schedule.WorkCenter.PlantId)
                .SelectMany(plant => plant.WorkCenters)
                .SingleOrDefault(candidate => candidate.Id == schedule.WorkCenter.WorkCenterId);

            if (workCenter is null)
            {
                issues.Add(Error($"REFSCHED001: Unknown scheduling work center {schedule.WorkCenter}."));
                continue;
            }

            ProductionSchedulingProfile? profile = workCenter.SchedulingProfile;
            if (profile is null ||
                profile.BucketMode != SchedulingBucketMode.MacroMicro ||
                profile.MicroPeriodCount is null)
            {
                issues.Add(Error(
                    $"DECSCHED001: Work center {schedule.WorkCenter} does not expose an explicit macro/micro scheduling grid."));
                continue;
            }

            foreach (ProductionMicroPeriodDecision decision in schedule.MicroPeriods)
            {
                int macro = decision.MicroPeriod.MacroPeriod;
                int micro = decision.MicroPeriod.MicroPeriodIndex;

                if (macro <= 0 || macro > instance.PlanningHorizon ||
                    micro <= 0 || micro > profile.MicroPeriodCount.GetCount(macro))
                {
                    issues.Add(Error(
                        $"DECSCHED002: Micro-period {decision.MicroPeriod} is outside the instance scheduling grid."));
                    continue;
                }

                if (decision.SetupItemId <= 0 ||
                    !instance.SupplyChain.Items.Any(item => item.Id == decision.SetupItemId))
                {
                    issues.Add(Error(
                        $"REFSCHED002: Unknown or missing setup item {decision.SetupItemId} at {decision.MicroPeriod}."));
                }

                if (decision.RoutingId <= 0)
                {
                    if (decision.Quantity > 0.0)
                    {
                        issues.Add(Error(
                            $"DECSCHED007: Positive production at {decision.MicroPeriod} has no routing."));
                    }
                    continue;
                }

                ProductionRouting? routing = instance.SupplyChain.ProductionRoutings
                    .SingleOrDefault(candidate => candidate.Id == decision.RoutingId);

                if (routing is null)
                {
                    issues.Add(Error(
                        $"REFSCHED003: Unknown production routing {decision.RoutingId} at {decision.MicroPeriod}."));
                    continue;
                }

                if (routing.ItemId != decision.SetupItemId)
                {
                    issues.Add(Error(
                        $"DECSCHED003: Routing {routing.Id} produces item {routing.ItemId}, not setup item {decision.SetupItemId}."));
                }

                if (!routing.WorkCenters.Any(reference =>
                        reference.RefersToSameWorkCenter(schedule.WorkCenter)))
                {
                    issues.Add(Error(
                        $"DECSCHED004: Routing {routing.Id} does not use work center {schedule.WorkCenter}."));
                }
            }
        }

        return issues;
    }

    private static SolutionCheckIssue Error(string message) => new()
    {
        Severity = SolutionCheckSeverity.Error,
        Kind = SolutionCheckIssueKind.Structural,
        Message = message
    };
}
