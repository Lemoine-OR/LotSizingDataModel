namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Classifies the directed physical topology of a supply-flow network.
/// </summary>
/// <remarks>
/// Cyclic physical networks are represented as <see cref="General"/> with
/// <c>HasCycles = true</c>. A physical cycle is not invalid by definition.
/// </remarks>
public enum SupplyNetworkTopologyType
{
    Unknown,
    Independent,
    Serial,
    Convergent,
    Divergent,
    Tree,
    General
}
