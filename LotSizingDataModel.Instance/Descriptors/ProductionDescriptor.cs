namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes production and lot-size semantics.</summary>
public sealed class ProductionDescriptor
{
    public bool HasProduction { get; init; }
    public bool HasLeadTimes { get; init; }
    public bool HasMinimumLotSizes { get; init; }
    public bool HasMaximumLotSizes { get; init; }
    public bool HasLotSizeMultiples { get; init; }

    public bool HasLotSizeRestrictions =>
        HasMinimumLotSizes || HasMaximumLotSizes || HasLotSizeMultiples;
}
