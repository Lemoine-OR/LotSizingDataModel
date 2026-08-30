using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Solver.Formulation;

internal static class GlspSchedulingData
{
    public static bool TryGetSchedulingWorkCenter(
        LotSizingInstance instance,
        out int plantId,
        out WorkCenter? workCenter,
        out ProductionSchedulingProfile? profile)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var entries =
            instance.SupplyChain.Plants
                .SelectMany(
                    plant =>
                        plant.WorkCenters.Select(
                            candidate =>
                                (
                                    PlantId: plant.Id,
                                    WorkCenter: candidate
                                )))
                .Where(
                    entry =>
                        entry.WorkCenter.SchedulingProfile is not null)
                .Take(2)
                .ToArray();

        if (entries.Length != 1)
        {
            plantId = 0;
            workCenter = null;
            profile = null;

            return false;
        }

        plantId =
            entries[0].PlantId;

        workCenter =
            entries[0].WorkCenter;

        profile =
            entries[0].WorkCenter.SchedulingProfile;

        return
            profile is not null;
    }

    public static (
        int PlantId,
        WorkCenter WorkCenter,
        ProductionSchedulingProfile Profile)
        GetSchedulingWorkCenter(
            LotSizingInstance instance)
    {
        if (!TryGetSchedulingWorkCenter(
                instance,
                out int plantId,
                out WorkCenter? workCenter,
                out ProductionSchedulingProfile? profile) ||
            workCenter is null ||
            profile is null)
        {
            throw new InvalidOperationException(
                "Executable GLSP requires exactly one scheduling work center.");
        }

        return (
            plantId,
            workCenter,
            profile);
    }

    public static IReadOnlyList<ProductionRouting> GetRoutings(
        LotSizingInstance instance,
        int plantId,
        int workCenterId)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return instance.SupplyChain.ProductionRoutings
            .Where(
                routing =>
                    routing.WorkCenters.Any(
                        reference =>
                            reference.PlantId == plantId &&
                            reference.WorkCenterId ==
                                workCenterId))
            .OrderBy(
                routing =>
                    routing.Id)
            .ToArray();
    }

    public static bool TryGetCharacteristic(
        LotSizingInstance instance,
        ProductionRouting routing,
        int plantId,
        int workCenterId,
        out ProductionCharacteristic? characteristic)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(routing);

        ProductionCharacteristic[] matches =
            instance.SupplyChain.ProductionCharacteristics
                .Where(
                    candidate =>
                        candidate.ItemId ==
                            routing.ItemId &&
                        candidate.WorkCenter.PlantId ==
                            plantId &&
                        candidate.WorkCenter.WorkCenterId ==
                            workCenterId)
                .Take(2)
                .ToArray();

        if (matches.Length != 1)
        {
            characteristic = null;

            return false;
        }

        characteristic =
            matches[0];

        return true;
    }

    public static ProductionCharacteristic GetCharacteristic(
        LotSizingInstance instance,
        ProductionRouting routing,
        int plantId,
        int workCenterId)
    {
        if (!TryGetCharacteristic(
                instance,
                routing,
                plantId,
                workCenterId,
                out ProductionCharacteristic? characteristic) ||
            characteristic is null)
        {
            throw new InvalidOperationException(
                $"Executable GLSP requires exactly one production " +
                $"characteristic for routing {routing.Id} on " +
                $"Plant:{plantId}/WorkCenter:{workCenterId}.");
        }

        return characteristic;
    }

    public static ProductionChangeover? FindChangeover(
        ProductionSchedulingProfile profile,
        int fromItemId,
        int toItemId)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return profile.Changeovers
            .SingleOrDefault(
                changeover =>
                    changeover.FromItemId ==
                        fromItemId &&
                    changeover.ToItemId ==
                        toItemId);
    }
}
