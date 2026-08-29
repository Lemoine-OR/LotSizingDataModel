namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Identifies the Core relationship that induces a physical forward-flow arc.
/// </summary>
public enum SupplyNetworkArcKind
{
    SupplierDelivery,
    TransportLane,
    DistributionCenterSourcing
}
