namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes inventory and unmet-demand semantics.</summary>
public sealed class InventoryServiceDescriptor
{
    public bool HasInitialInventory { get; init; }
    public bool HasSafetyStockRequirements { get; init; }
    public bool HasBacklogging { get; init; }
    public bool HasLostSales { get; init; }
}
