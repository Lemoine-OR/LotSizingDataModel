namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes procurement semantics represented by the current model.</summary>
public sealed class ProcurementDescriptor
{
    public bool HasPurchasing { get; init; }
    public bool HasSupplierLeadTimes { get; init; }
}
