using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes structural dimensions of a lot-sizing problem.</summary>
public sealed class StructureDescriptor
{
    public int ItemCount { get; init; }
    public int PlantCount { get; init; }
    public int WorkCenterCount { get; init; }
    public int WarehouseCount { get; init; }
    public int SupplierCount { get; init; }
    public int DistributionCenterCount { get; init; }
    public int TransportResourceCount { get; init; }
    public int ProductStructureRelationshipCount { get; init; }
    public int MaximumProductStructureDepth { get; init; }
    public ProductStructureType ProductStructureType { get; init; }
    public bool IsMultiSite { get; init; }

    public bool IsSingleItem => ItemCount == 1;
    public bool IsMultiItem => ItemCount > 1;
    public bool HasProductStructure => ProductStructureRelationshipCount > 0;
    public bool IsSingleLevel => !HasProductStructure;
    public bool IsMultiLevel => HasProductStructure;
}
