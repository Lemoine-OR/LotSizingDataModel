namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Describes one node of the physical supply-flow graph.
/// </summary>
public sealed class SupplyNetworkNodeDescriptor
{
    public required string Key { get; init; }
    public required SupplyNetworkNodeKind Kind { get; init; }
    public int ReferenceId { get; init; }
    public bool IsDeclared { get; init; } = true;
    public int InDegree { get; init; }
    public int OutDegree { get; init; }
    public bool IsSource => InDegree == 0;
    public bool IsSink => OutDegree == 0;
}
