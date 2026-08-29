namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes represented capacity families and extensions.</summary>
public sealed class CapacityDescriptor
{
    public bool HasProductionCapacity { get; init; }
    public bool HasSharedProductionCapacity { get; init; }
    public bool HasTimeVaryingProductionCapacity { get; init; }
    public bool HasSupplierCapacity { get; init; }
    public bool HasTransportCapacity { get; init; }
    public bool HasWarehouseCapacity { get; init; }
    public bool HasAdditionalProductionCapacity { get; init; }
    public bool HasAdditionalWarehouseCapacity { get; init; }
    public bool HasAdditionalTransportCapacity { get; init; }

    public bool IsCapacitated =>
        HasProductionCapacity || HasSupplierCapacity ||
        HasTransportCapacity || HasWarehouseCapacity;

    public bool HasConstantProductionCapacity =>
        HasProductionCapacity && !HasTimeVaryingProductionCapacity;

    public bool HasAdditionalCapacity =>
        HasAdditionalProductionCapacity ||
        HasAdditionalWarehouseCapacity ||
        HasAdditionalTransportCapacity;
}
