using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Extracts the physical forward supply-flow graph encoded by Core.
/// </summary>
/// <remarks>
/// BOM relationships are intentionally excluded. Forward arcs are induced by
/// SupplierDelivery, TransportLane and DistributionCenterSourcing.
/// </remarks>
public sealed class SupplyNetworkAnalyzer
{
    public SupplyNetworkDescriptor Analyze(SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        var nodeSeeds =
            new Dictionary<string, NodeSeed>(StringComparer.Ordinal);

        foreach (Supplier supplier in supplyChain.Suppliers)
        {
            AddDeclaredNode(
                nodeSeeds,
                SupplierKey(supplier.Id),
                SupplyNetworkNodeKind.Supplier,
                supplier.Id);
        }

        foreach (Plant plant in supplyChain.Plants)
        {
            AddDeclaredNode(
                nodeSeeds,
                PlantWarehouseKey(plant.Id),
                SupplyNetworkNodeKind.PlantWarehouse,
                plant.Id);
        }

        foreach (StandaloneWarehouse warehouse
                 in supplyChain.StandaloneWarehouses)
        {
            AddDeclaredNode(
                nodeSeeds,
                StandaloneWarehouseKey(warehouse.Id),
                SupplyNetworkNodeKind.StandaloneWarehouse,
                warehouse.Id);
        }

        foreach (DistributionCenter center
                 in supplyChain.DistributionCenters)
        {
            AddDeclaredNode(
                nodeSeeds,
                DistributionCenterKey(center.Id),
                SupplyNetworkNodeKind.DistributionCenter,
                center.Id);
        }

        var arcMultiplicity =
            new Dictionary<ArcKey, int>();

        foreach (var delivery in supplyChain.SupplierDeliveries)
        {
            string from = SupplierKey(delivery.SupplierId);
            string to = WarehouseKey(delivery.Warehouse);

            EnsureReferencedNode(
                nodeSeeds,
                from,
                SupplyNetworkNodeKind.Supplier,
                delivery.SupplierId);

            EnsureWarehouseNode(nodeSeeds, delivery.Warehouse);

            AddArc(
                arcMultiplicity,
                from,
                to,
                SupplyNetworkArcKind.SupplierDelivery);
        }

        foreach (TransportResource resource
                 in supplyChain.TransportResources)
        {
            foreach (TransportLane lane in resource.Lanes)
            {
                string from = WarehouseKey(lane.Origin);
                string to = WarehouseKey(lane.Destination);

                EnsureWarehouseNode(nodeSeeds, lane.Origin);
                EnsureWarehouseNode(nodeSeeds, lane.Destination);

                AddArc(
                    arcMultiplicity,
                    from,
                    to,
                    SupplyNetworkArcKind.TransportLane);
            }
        }

        foreach (var sourcing
                 in supplyChain.DistributionCenterSourcings)
        {
            string from = WarehouseKey(sourcing.Warehouse);
            string to =
                DistributionCenterKey(
                    sourcing.DistributionCenterId);

            EnsureWarehouseNode(nodeSeeds, sourcing.Warehouse);

            EnsureReferencedNode(
                nodeSeeds,
                to,
                SupplyNetworkNodeKind.DistributionCenter,
                sourcing.DistributionCenterId);

            AddArc(
                arcMultiplicity,
                from,
                to,
                SupplyNetworkArcKind.DistributionCenterSourcing);
        }

        var adjacency =
            nodeSeeds.Keys.ToDictionary(
                key => key,
                _ => new HashSet<string>(StringComparer.Ordinal),
                StringComparer.Ordinal);

        var reverseAdjacency =
            nodeSeeds.Keys.ToDictionary(
                key => key,
                _ => new HashSet<string>(StringComparer.Ordinal),
                StringComparer.Ordinal);

        foreach (ArcKey arc in arcMultiplicity.Keys)
        {
            adjacency[arc.From].Add(arc.To);
            reverseAdjacency[arc.To].Add(arc.From);
        }

        bool hasCycles =
            HasDirectedCycle(
                nodeSeeds.Keys,
                adjacency,
                reverseAdjacency);

        SupplyNetworkTopologyType topology =
            ClassifyTopology(
                nodeSeeds.Keys,
                adjacency,
                reverseAdjacency,
                hasCycles);

        int? echelonCount =
            hasCycles
                ? null
                : ComputeEchelonCount(
                    nodeSeeds.Keys,
                    adjacency,
                    reverseAdjacency);

        SupplyNetworkNodeDescriptor[] nodes =
            nodeSeeds.Values
                .OrderBy(seed => seed.Key, StringComparer.Ordinal)
                .Select(
                    seed =>
                        new SupplyNetworkNodeDescriptor
                        {
                            Key = seed.Key,
                            Kind = seed.Kind,
                            ReferenceId = seed.ReferenceId,
                            IsDeclared = seed.IsDeclared,
                            InDegree =
                                reverseAdjacency[seed.Key].Count,
                            OutDegree =
                                adjacency[seed.Key].Count
                        })
                .ToArray();

        SupplyNetworkArcDescriptor[] arcs =
            arcMultiplicity
                .OrderBy(pair => pair.Key.From, StringComparer.Ordinal)
                .ThenBy(pair => pair.Key.To, StringComparer.Ordinal)
                .ThenBy(pair => pair.Key.Kind)
                .Select(
                    pair =>
                        new SupplyNetworkArcDescriptor
                        {
                            FromKey = pair.Key.From,
                            ToKey = pair.Key.To,
                            Kind = pair.Key.Kind,
                            RelationshipMultiplicity = pair.Value
                        })
                .ToArray();

        var forward =
            new DirectedSupplyNetworkDescriptor
            {
                Nodes = nodes,
                Arcs = arcs,
                Topology = topology,
                HasCycles = hasCycles,
                EchelonCount = echelonCount
            };

        bool hasMultiSourcing =
            HasSupplierMultiSourcing(supplyChain) ||
            HasDistributionCenterMultiSourcing(supplyChain);

        bool hasTransshipment =
            supplyChain.TransportResources.Any(
                resource => resource.Lanes.Count > 0);

        bool hasDistributionNetwork =
            supplyChain.DistributionCenterSourcings.Count > 0;

        bool hasExternalDemandAtDistributionCenters =
            supplyChain.Demands.Count > 0;

        return new SupplyNetworkDescriptor
        {
            ForwardNetwork = forward,
            ReverseNetwork = null,
            Coupling = NetworkCouplingType.ForwardOnly,
            HasMultiSourcing = hasMultiSourcing,
            HasTransshipment = hasTransshipment,
            HasDistributionNetwork =
                hasDistributionNetwork,
            HasExternalDemandAtDistributionCenters =
                hasExternalDemandAtDistributionCenters
        };
    }

    private static bool HasSupplierMultiSourcing(
        SupplyChain supplyChain)
    {
        return supplyChain.SupplierDeliveries
            .GroupBy(
                delivery =>
                    $"{delivery.ItemId}|" +
                    $"{delivery.Warehouse.Kind}|" +
                    $"{delivery.Warehouse.ReferenceId}",
                StringComparer.Ordinal)
            .Any(
                group =>
                    group
                        .Select(delivery => delivery.SupplierId)
                        .Distinct()
                        .Skip(1)
                        .Any());
    }

    private static bool HasDistributionCenterMultiSourcing(
        SupplyChain supplyChain)
    {
        return supplyChain.DistributionCenterSourcings
            .GroupBy(
                sourcing =>
                    $"{sourcing.ItemId}|" +
                    $"{sourcing.DistributionCenterId}",
                StringComparer.Ordinal)
            .Any(
                group =>
                    group
                        .Select(
                            sourcing =>
                                $"{sourcing.Warehouse.Kind}|" +
                                $"{sourcing.Warehouse.ReferenceId}")
                        .Distinct(StringComparer.Ordinal)
                        .Skip(1)
                        .Any());
    }

    private static SupplyNetworkTopologyType ClassifyTopology(
        IEnumerable<string> keys,
        IReadOnlyDictionary<string, HashSet<string>> adjacency,
        IReadOnlyDictionary<string, HashSet<string>> reverseAdjacency,
        bool hasCycles)
    {
        string[] nodes = keys.ToArray();

        if (nodes.Length == 0)
        {
            return SupplyNetworkTopologyType.Unknown;
        }

        int physicalEdgeCount =
            adjacency.Values.Sum(targets => targets.Count);

        if (physicalEdgeCount == 0)
        {
            return SupplyNetworkTopologyType.Independent;
        }

        if (hasCycles)
        {
            return SupplyNetworkTopologyType.General;
        }

        int maxIn =
            nodes.Max(key => reverseAdjacency[key].Count);

        int maxOut =
            nodes.Max(key => adjacency[key].Count);

        if (maxIn <= 1 && maxOut <= 1)
        {
            return SupplyNetworkTopologyType.Serial;
        }

        if (maxIn > 1 && maxOut <= 1)
        {
            return SupplyNetworkTopologyType.Convergent;
        }

        if (maxOut > 1 && maxIn <= 1)
        {
            return SupplyNetworkTopologyType.Divergent;
        }

        if (IsUndirectedForest(nodes, adjacency))
        {
            return SupplyNetworkTopologyType.Tree;
        }

        return SupplyNetworkTopologyType.General;
    }

    private static bool HasDirectedCycle(
        IEnumerable<string> keys,
        IReadOnlyDictionary<string, HashSet<string>> adjacency,
        IReadOnlyDictionary<string, HashSet<string>> reverseAdjacency)
    {
        var indegree =
            keys.ToDictionary(
                key => key,
                key => reverseAdjacency[key].Count,
                StringComparer.Ordinal);

        var queue =
            new Queue<string>(
                indegree
                    .Where(pair => pair.Value == 0)
                    .Select(pair => pair.Key));

        int visited = 0;

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            visited++;

            foreach (string target in adjacency[current])
            {
                indegree[target]--;

                if (indegree[target] == 0)
                {
                    queue.Enqueue(target);
                }
            }
        }

        return visited != indegree.Count;
    }

    private static int ComputeEchelonCount(
        IEnumerable<string> keys,
        IReadOnlyDictionary<string, HashSet<string>> adjacency,
        IReadOnlyDictionary<string, HashSet<string>> reverseAdjacency)
    {
        string[] nodes = keys.ToArray();

        if (nodes.Length == 0)
        {
            return 0;
        }

        var indegree =
            nodes.ToDictionary(
                key => key,
                key => reverseAdjacency[key].Count,
                StringComparer.Ordinal);

        var depth =
            nodes.ToDictionary(
                key => key,
                _ => 1,
                StringComparer.Ordinal);

        var queue =
            new Queue<string>(
                indegree
                    .Where(pair => pair.Value == 0)
                    .Select(pair => pair.Key));

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            foreach (string target in adjacency[current])
            {
                depth[target] =
                    Math.Max(
                        depth[target],
                        depth[current] + 1);

                indegree[target]--;

                if (indegree[target] == 0)
                {
                    queue.Enqueue(target);
                }
            }
        }

        return depth.Values.Max();
    }

    private static bool IsUndirectedForest(
        IReadOnlyCollection<string> nodes,
        IReadOnlyDictionary<string, HashSet<string>> adjacency)
    {
        var undirected =
            nodes.ToDictionary(
                key => key,
                _ => new HashSet<string>(StringComparer.Ordinal),
                StringComparer.Ordinal);

        int edgeCount = 0;

        foreach (string from in nodes)
        {
            foreach (string to in adjacency[from])
            {
                if (undirected[from].Add(to))
                {
                    undirected[to].Add(from);
                    edgeCount++;
                }
            }
        }

        int components = 0;
        var visited =
            new HashSet<string>(StringComparer.Ordinal);

        foreach (string start in nodes)
        {
            if (!visited.Add(start))
            {
                continue;
            }

            components++;

            var stack = new Stack<string>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                string current = stack.Pop();

                foreach (string neighbor in undirected[current])
                {
                    if (visited.Add(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }
        }

        return edgeCount == nodes.Count - components;
    }

    private static void AddArc(
        IDictionary<ArcKey, int> arcs,
        string from,
        string to,
        SupplyNetworkArcKind kind)
    {
        var key = new ArcKey(from, to, kind);

        if (arcs.TryGetValue(key, out int count))
        {
            arcs[key] = count + 1;
        }
        else
        {
            arcs.Add(key, 1);
        }
    }

    private static void AddDeclaredNode(
        IDictionary<string, NodeSeed> nodes,
        string key,
        SupplyNetworkNodeKind kind,
        int referenceId)
    {
        nodes[key] =
            new NodeSeed(
                key,
                kind,
                referenceId,
                true);
    }

    private static void EnsureReferencedNode(
        IDictionary<string, NodeSeed> nodes,
        string key,
        SupplyNetworkNodeKind kind,
        int referenceId)
    {
        if (!nodes.ContainsKey(key))
        {
            nodes.Add(
                key,
                new NodeSeed(
                    key,
                    kind,
                    referenceId,
                    false));
        }
    }

    private static void EnsureWarehouseNode(
        IDictionary<string, NodeSeed> nodes,
        WarehouseReference warehouse)
    {
        SupplyNetworkNodeKind kind =
            warehouse.Kind ==
            WarehouseReferenceKind.PlantWarehouse
                ? SupplyNetworkNodeKind.PlantWarehouse
                : SupplyNetworkNodeKind.StandaloneWarehouse;

        EnsureReferencedNode(
            nodes,
            WarehouseKey(warehouse),
            kind,
            warehouse.ReferenceId);
    }

    private static string SupplierKey(int id) =>
        $"supplier:{id}";

    private static string DistributionCenterKey(int id) =>
        $"distributionCenter:{id}";

    private static string PlantWarehouseKey(int plantId) =>
        $"plantWarehouse:{plantId}";

    private static string StandaloneWarehouseKey(int warehouseId) =>
        $"warehouse:{warehouseId}";

    private static string WarehouseKey(
        WarehouseReference warehouse)
    {
        return warehouse.Kind ==
               WarehouseReferenceKind.PlantWarehouse
            ? PlantWarehouseKey(warehouse.ReferenceId)
            : StandaloneWarehouseKey(warehouse.ReferenceId);
    }

    private sealed record NodeSeed(
        string Key,
        SupplyNetworkNodeKind Kind,
        int ReferenceId,
        bool IsDeclared);

    private readonly record struct ArcKey(
        string From,
        string To,
        SupplyNetworkArcKind Kind);
}
