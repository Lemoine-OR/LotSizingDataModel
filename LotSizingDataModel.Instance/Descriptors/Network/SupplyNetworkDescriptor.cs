namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Describes the physical supply-flow network independently from the BOM and
/// independently from any planning paradigm such as MRP or DRP.
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
    /// Gets whether at least one distribution-center sourcing relationship
    /// is represented in the physical network.
    /// </summary>
    public bool HasDistributionNetwork { get; init; }

    /// <summary>
    /// Gets whether external demand records are declared at distribution
    /// centers.
    /// </summary>
    public bool HasExternalDemandAtDistributionCenters { get; init; }

    /// <summary>
    /// Gets whether the forward physical graph has at least two facility
    /// echelons.
    /// </summary>
    public bool HasMultiEchelonStructure =>
        ForwardNetwork.EchelonCount is >= 2;

    /// <summary>
    /// Gets whether the represented distribution network is acyclic and has
    /// no detected supplier/DC multisourcing.
    /// </summary>
    /// <remarks>
    /// This is a neutral structural fact. It is deliberately not named after
    /// DRP because Distribution Requirements Planning is a planning paradigm,
    /// not a lot-sizing model or a network-topology type.
    /// </remarks>
    public bool IsAcyclicSingleSourcingDistributionNetwork =>
        HasDistributionNetwork &&
        !ForwardNetwork.HasCycles &&
        !HasMultiSourcing;
}
