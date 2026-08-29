namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes transportation and distribution semantics.</summary>
public sealed class TransportationDistributionDescriptor
{
    public bool HasTransportation { get; init; }
    public bool HasTransportLeadTimes { get; init; }
    public bool HasDistribution { get; init; }

    public bool HasNetworkDecisions =>
        HasTransportation || HasDistribution;
}
