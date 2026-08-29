namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Immutable analysis result for one directed physical supply network.
/// </summary>
public sealed class DirectedSupplyNetworkDescriptor
{
    public IReadOnlyList<SupplyNetworkNodeDescriptor> Nodes { get; init; } =
        Array.Empty<SupplyNetworkNodeDescriptor>();

    public IReadOnlyList<SupplyNetworkArcDescriptor> Arcs { get; init; } =
        Array.Empty<SupplyNetworkArcDescriptor>();

    public SupplyNetworkTopologyType Topology { get; init; } =
        SupplyNetworkTopologyType.Unknown;

    public bool HasCycles { get; init; }

    /// <summary>
    /// Gets the number of facility echelons when the graph is acyclic.
    /// Null means that a longest-path echelon count is not defined because
    /// the physical graph contains a directed cycle.
    /// </summary>
    public int? EchelonCount { get; init; }

    public int NodeCount => Nodes.Count;
    public int ArcCount => Arcs.Count;
    public int SourceCount => Nodes.Count(node => node.IsSource);
    public int SinkCount => Nodes.Count(node => node.IsSink);

    public IReadOnlyList<string> SourceKeys =>
        Nodes.Where(node => node.IsSource).Select(node => node.Key).ToArray();

    public IReadOnlyList<string> SinkKeys =>
        Nodes.Where(node => node.IsSink).Select(node => node.Key).ToArray();
}
