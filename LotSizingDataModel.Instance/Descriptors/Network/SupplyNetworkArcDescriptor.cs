namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Describes one aggregated physical forward-flow relationship.
/// </summary>
public sealed class SupplyNetworkArcDescriptor
{
    public required string FromKey { get; init; }
    public required string ToKey { get; init; }
    public required SupplyNetworkArcKind Kind { get; init; }

    /// <summary>
    /// Gets how many Core relationships collapse into this facility-level arc.
    /// </summary>
    public int RelationshipMultiplicity { get; init; } = 1;
}
