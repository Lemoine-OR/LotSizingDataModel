using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Instance.Analysis;

public static class ParallelSchedulingTopologyAnalyzer
{
    public static IReadOnlyList<ParallelRoutingTopologyDescriptor>
        Analyze(
            IEnumerable<ProductionRouting> routings)
    {
        ArgumentNullException.ThrowIfNull(
            routings);

        ProductionRouting[] normalized =
            routings.ToArray();

        foreach (ProductionRouting routing
                 in normalized)
        {
            ArgumentNullException.ThrowIfNull(
                routing);

            if (routing.Id <= 0 ||
                routing.ItemId <= 0 ||
                routing.PlantId <= 0)
            {
                throw new InvalidOperationException(
                    "Parallel scheduling analysis requires positive routing, item and plant identifiers.");
            }

            if (!routing.HasConsistentWorkCenterReferences)
            {
                throw new InvalidOperationException(
                    $"Routing '{routing.Id}' contains a work-center reference from another plant.");
            }
        }

        return normalized
            .GroupBy(
                routing =>
                    routing.ItemId)
            .Where(
                group =>
                    group.Count() > 1)
            .OrderBy(
                group =>
                    group.Key)
            .Select(
                group =>
                {
                    ProductionRouting[] itemRoutings =
                        group
                            .OrderBy(
                                routing =>
                                    routing.Id)
                            .ToArray();

                    bool repeatedPlant =
                        itemRoutings
                            .GroupBy(
                                routing =>
                                    routing.PlantId)
                            .Any(
                                plantGroup =>
                                    plantGroup.Count() > 1);

                    return new ParallelRoutingTopologyDescriptor(
                        group.Key,
                        itemRoutings
                            .Select(
                                routing =>
                                    routing.Id)
                            .ToArray(),
                        itemRoutings
                            .Select(
                                routing =>
                                    routing.PlantId)
                            .ToArray(),
                        repeatedPlant);
                })
            .ToArray();
    }
}
