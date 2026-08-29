namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Identifies one physical facility category in the supply-flow graph.
/// </summary>
public enum SupplyNetworkNodeKind
{
    Supplier,
    PlantWarehouse,
    StandaloneWarehouse,
    DistributionCenter
}
