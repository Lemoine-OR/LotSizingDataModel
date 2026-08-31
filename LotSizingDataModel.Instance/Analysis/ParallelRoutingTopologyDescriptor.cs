namespace LotSizingDataModel.Instance.Analysis;

public sealed class ParallelRoutingTopologyDescriptor
{
    public ParallelRoutingTopologyDescriptor(
        int itemId,
        IReadOnlyList<int> routingIds,
        IReadOnlyList<int> plantIds,
        bool hasMultipleRoutingsWithinPlant)
    {
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemId));
        }

        ArgumentNullException.ThrowIfNull(
            routingIds);

        ArgumentNullException.ThrowIfNull(
            plantIds);

        ItemId =
            itemId;

        RoutingIds =
            routingIds
                .Distinct()
                .OrderBy(
                    value =>
                        value)
                .ToArray();

        PlantIds =
            plantIds
                .Distinct()
                .OrderBy(
                    value =>
                        value)
                .ToArray();

        HasMultipleRoutingsWithinPlant =
            hasMultipleRoutingsWithinPlant;

        if (RoutingIds.Count < 2)
        {
            throw new InvalidOperationException(
                "A parallel-routing topology requires at least two routings.");
        }
    }

    public int ItemId
    {
        get;
    }

    public IReadOnlyList<int> RoutingIds
    {
        get;
    }

    public IReadOnlyList<int> PlantIds
    {
        get;
    }

    public bool IsMultiSite =>
        PlantIds.Count > 1;

    public bool HasMultipleRoutingsWithinPlant
    {
        get;
    }
}
