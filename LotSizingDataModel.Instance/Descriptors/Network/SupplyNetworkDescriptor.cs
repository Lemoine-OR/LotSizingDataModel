namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Describes the physical supply-flow network independently from the BOM.
/// </summary>
public sealed class SupplyNetworkDescriptor
{
    public DirectedSupplyNetworkDescriptor ForwardNetwork { get; init; } =
        new();

    public DirectedSupplyNetworkDescriptor? ReverseNetwork { get; init; }

    public NetworkCouplingType Coupling { get; init; } =
        NetworkCouplingType.ForwardOnly;

    public bool HasReverseNetwork => ReverseNetwork is not null;
    public bool HasMultiSourcing { get; init; }
    public bool HasTransshipment { get; init; }

    /// <summary>
    /// Gets whether the graph matches the structural signature of a classical
    /// single-sourcing, acyclic DRP distribution network.
    /// </summary>
    /// <remarks>
    /// This is only a structural candidate, not a complete problem-class claim.
    /// </remarks>
    public bool IsClassicalDrpTopologyCandidate { get; init; }
}
