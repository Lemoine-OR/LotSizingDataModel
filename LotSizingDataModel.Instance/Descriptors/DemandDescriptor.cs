namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes demand semantics already represented by the model.</summary>
public sealed class DemandDescriptor
{
    public bool HasDemand { get; init; }
    public bool IsDeterministic { get; init; }
    public bool IsTimeVarying { get; init; }
    public bool IsStationary => HasDemand && !IsTimeVarying;
}
