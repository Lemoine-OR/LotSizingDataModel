using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Typed alpha-field representation of the physical network.
/// </summary>
public sealed class UniversalNetworkNotation
{
    public NetworkCouplingType Coupling { get; init; } =
        NetworkCouplingType.ForwardOnly;

    public SupplyNetworkTopologyType ForwardTopology { get; init; } =
        SupplyNetworkTopologyType.Unknown;

    public SupplyNetworkTopologyType? ReverseTopology { get; init; }

    public int? EchelonCount { get; init; }

    public bool HasCycles { get; init; }
    public bool HasMultiSourcing { get; init; }
    public bool HasTransshipment { get; init; }
}
