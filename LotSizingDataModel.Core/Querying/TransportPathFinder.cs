using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Querying;

/// <summary>
/// Finds transport paths between warehouses.
///
/// Transport lanes are directed. The path finder minimizes
/// the sum of the transport lead times of the selected lanes.
/// </summary>
public sealed class TransportPathFinder
{
    /// <summary>
    /// Initializes a path finder and creates a new entity index.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain whose transport network must be explored.
    /// </param>
    public TransportPathFinder(SupplyChain supplyChain)
        : this(
            new SupplyChainIndex(
                supplyChain ??
                throw new ArgumentNullException(
                    nameof(supplyChain))))
    {
    }

    /// <summary>
    /// Initializes a path finder using an existing entity index.
    /// </summary>
    /// <param name="index">
    /// Index used to resolve warehouse and item references.
    /// </param>
    public TransportPathFinder(SupplyChainIndex index)
    {
        Index = index ??
            throw new ArgumentNullException(nameof(index));

        SupplyChain = index.SupplyChain;
    }

    /// <summary>
    /// Gets the explored supply chain.
    /// </summary>
    public SupplyChain SupplyChain { get; }

    /// <summary>
    /// Gets the entity index used by the path finder.
    /// </summary>
    public SupplyChainIndex Index { get; }

    /// <summary>
    /// Rebuilds the underlying entity index.
    ///
    /// Call this method after directly modifying the supply-chain
    /// entity collections.
    /// </summary>
    public void RebuildIndex()
    {
        Index.Rebuild();
    }

    /// <summary>
    /// Finds the fastest transport path between two warehouses.
    ///
    /// Every transport resource and every valid transport lane
    /// may be used.
    /// </summary>
    /// <param name="origin">
    /// Origin warehouse.
    /// </param>
    /// <param name="destination">
    /// Destination warehouse.
    /// </param>
    /// <returns>
    /// The fastest path, or null when no path exists.
    /// </returns>
    public TransportPath? FindFastestPath(
        WarehouseReference origin,
        WarehouseReference destination)
    {
        return FindFastestPathCore(
            origin,
            destination,
            allowedTransportResourceIds: null);
    }

    /// <summary>
    /// Finds the fastest transport path compatible with
    /// a specified item.
    ///
    /// A transport resource is considered compatible when a
    /// TransportCharacteristic exists for the item-resource pair.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the transported item.
    /// </param>
    /// <param name="origin">
    /// Origin warehouse.
    /// </param>
    /// <param name="destination">
    /// Destination warehouse.
    /// </param>
    /// <returns>
    /// The fastest compatible path, or null when no path exists.
    /// </returns>
    public TransportPath? FindFastestPathForItem(
        int itemId,
        WarehouseReference origin,
        WarehouseReference destination)
    {
        Index.GetRequiredItem(itemId);

        // Build the set of transport resources compatible with the item.
        HashSet<int> allowedTransportResourceIds =
            SupplyChain.TransportCharacteristics
                .Where(
                    characteristic =>
                        characteristic.ItemId == itemId)
                .Select(
                    characteristic =>
                        characteristic.TransportResourceId)
                .ToHashSet();

        return FindFastestPathCore(
            origin,
            destination,
            allowedTransportResourceIds);
    }

    /// <summary>
    /// Gets the fastest transport path between two warehouses.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no path exists.
    /// </exception>
    public TransportPath GetRequiredFastestPath(
        WarehouseReference origin,
        WarehouseReference destination)
    {
        return FindFastestPath(
                   origin,
                   destination) ??
            throw new KeyNotFoundException(
                "No transport path exists between " +
                $"{FormatWarehouse(origin)} and " +
                $"{FormatWarehouse(destination)}.");
    }

    /// <summary>
    /// Gets the fastest item-compatible transport path
    /// between two warehouses.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no compatible path exists.
    /// </exception>
    public TransportPath GetRequiredFastestPathForItem(
        int itemId,
        WarehouseReference origin,
        WarehouseReference destination)
    {
        return FindFastestPathForItem(
                   itemId,
                   origin,
                   destination) ??
            throw new KeyNotFoundException(
                $"No transport path exists for item {itemId} " +
                $"between {FormatWarehouse(origin)} and " +
                $"{FormatWarehouse(destination)}.");
    }

    /// <summary>
    /// Determines whether a transport path exists between
    /// two warehouses.
    /// </summary>
    public bool HasPath(
        WarehouseReference origin,
        WarehouseReference destination)
    {
        return FindFastestPath(
            origin,
            destination) is not null;
    }

    /// <summary>
    /// Determines whether an item-compatible transport path
    /// exists between two warehouses.
    /// </summary>
    public bool HasPathForItem(
        int itemId,
        WarehouseReference origin,
        WarehouseReference destination)
    {
        return FindFastestPathForItem(
            itemId,
            origin,
            destination) is not null;
    }

    private TransportPath? FindFastestPathCore(
        WarehouseReference origin,
        WarehouseReference destination,
        IReadOnlySet<int>? allowedTransportResourceIds)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        /*
         * These calls also ensure that both references identify
         * warehouses that exist in the current supply chain.
         */
        Index.GetRequiredWarehouse(origin);
        Index.GetRequiredWarehouse(destination);

        WarehouseKey originKey =
            WarehouseKey.FromReference(origin);

        WarehouseKey destinationKey =
            WarehouseKey.FromReference(destination);

        // Early return for same-warehouse case: empty path with zero lead time.
        if (originKey == destinationKey)
        {
            return new TransportPath(
                origin,
                destination,
                Array.Empty<TransportLeg>());
        }

        Dictionary<WarehouseKey, List<TransportLeg>>
            adjacency = BuildAdjacency(
                allowedTransportResourceIds);

        // Initialize Dijkstra's algorithm: distance to origin is zero.
        var distances =
            new Dictionary<WarehouseKey, int>
            {
                [originKey] = 0
            };

        var predecessors =
            new Dictionary<WarehouseKey, TransportLeg>();

        var queue =
            new PriorityQueue<WarehouseKey, int>();

        queue.Enqueue(
            originKey,
            priority: 0);

        while (queue.TryDequeue(
                   out WarehouseKey current,
                   out int currentDistance))
        {
            if (!distances.TryGetValue(
                    current,
                    out int bestKnownDistance) ||
                currentDistance != bestKnownDistance)
            {
                /*
                 * A better route to this warehouse has already
                 * been inserted into the priority queue.
                 */
                continue;
            }

            // Stop when destination is reached; Dijkstra guarantees this is the shortest path.
            if (current == destinationKey)
            {
                break;
            }

            if (!adjacency.TryGetValue(
                    current,
                    out List<TransportLeg>? outgoingLegs))
            {
                continue;
            }

            foreach (TransportLeg leg in outgoingLegs)
            {
                WarehouseKey next =
                    WarehouseKey.FromReference(
                        leg.Destination);

                int candidateDistance;

                // Protect against overflow when summing lead times.
                try
                {
                    candidateDistance = checked(
                        currentDistance +
                        leg.LeadTime);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidOperationException(
                        "The accumulated transport lead time " +
                        "exceeds the supported integer range.",
                        exception);
                }

                if (distances.TryGetValue(
                        next,
                        out int existingDistance) &&
                    candidateDistance >= existingDistance)
                {
                    continue;
                }

                distances[next] = candidateDistance;
                predecessors[next] = leg;

                queue.Enqueue(
                    next,
                    candidateDistance);
            }
        }

        if (!distances.ContainsKey(destinationKey))
        {
            return null;
        }

        // Reconstruct the path backward from destination to origin.
        List<TransportLeg> reversedLegs =
            ReconstructPath(
                originKey,
                destinationKey,
                predecessors);

        reversedLegs.Reverse();

        return new TransportPath(
            origin,
            destination,
            reversedLegs);
    }

    private Dictionary<WarehouseKey, List<TransportLeg>>
        BuildAdjacency(
            IReadOnlySet<int>? allowedTransportResourceIds)
    {
        var adjacency =
            new Dictionary<WarehouseKey, List<TransportLeg>>();

        foreach (TransportResource resource
                 in SupplyChain.TransportResources)
        {
            // Skip resources not in the allowed set (if filtering by item compatibility).
            if (allowedTransportResourceIds is not null &&
                !allowedTransportResourceIds.Contains(
                    resource.Id))
            {
                continue;
            }

            foreach (TransportLane lane in resource.Lanes)
            {
                if (lane.Origin is null ||
                    lane.Destination is null)
                {
                    continue;
                }

                /*
                 * Invalid references are ignored here. They are
                 * reported separately by SupplyChainValidator.
                 */
                if (!Index.TryGetWarehouse(
                        lane.Origin,
                        out _) ||
                    !Index.TryGetWarehouse(
                        lane.Destination,
                        out _))
                {
                    continue;
                }

                if (lane.LeadTime < 0)
                {
                    throw new InvalidOperationException(
                        "A transport path cannot be calculated " +
                        "because a lane has a negative lead time.");
                }

                WarehouseKey originKey =
                    WarehouseKey.FromReference(
                        lane.Origin);

                if (!adjacency.TryGetValue(
                        originKey,
                        out List<TransportLeg>? legs))
                {
                    legs = new List<TransportLeg>();
                    adjacency.Add(originKey, legs);
                }

                legs.Add(
                    new TransportLeg(
                        resource,
                        lane));
            }
        }

        return adjacency;
    }

    private static List<TransportLeg> ReconstructPath(
        WarehouseKey origin,
        WarehouseKey destination,
        IReadOnlyDictionary<
            WarehouseKey,
            TransportLeg> predecessors)
    {
        var reversedLegs =
            new List<TransportLeg>();

        WarehouseKey current = destination;

        while (current != origin)
        {
            if (!predecessors.TryGetValue(
                    current,
                    out TransportLeg? leg))
            {
                throw new InvalidOperationException(
                    "The transport path cannot be reconstructed.");
            }

            reversedLegs.Add(leg);

            current =
                WarehouseKey.FromReference(
                    leg.Origin);
        }

        return reversedLegs;
    }

    private static string FormatWarehouse(
        WarehouseReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        return
            $"{reference.Kind}:{reference.ReferenceId}";
    }

    /// <summary>
    /// Internal value-based identifier used by the
    /// shortest-path algorithm.
    /// </summary>
    private readonly record struct WarehouseKey(
        WarehouseReferenceKind Kind,
        int ReferenceId)
    {
        public static WarehouseKey FromReference(
            WarehouseReference reference)
        {
            ArgumentNullException.ThrowIfNull(reference);

            return new WarehouseKey(
                reference.Kind,
                reference.ReferenceId);
        }
    }

    /// <summary>
    /// Represents one leg of a transport path.
    /// </summary>
    public sealed class TransportLeg
    {
        /// <summary>
        /// Initializes one leg of a transport path.
        /// </summary>
        /// <param name="transportResource">
        /// Transport resource used for this leg.
        /// </param>
        /// <param name="lane">
        /// Directed transport lane used for this leg.
        /// </param>
        public TransportLeg(
            TransportResource transportResource,
            TransportLane lane)
        {
            TransportResource = transportResource ??
                throw new ArgumentNullException(
                    nameof(transportResource));

            Lane = lane ??
                throw new ArgumentNullException(nameof(lane));
        }

        /// <summary>
        /// Gets the transport resource used for this leg.
        /// </summary>
        public TransportResource TransportResource { get; }

        /// <summary>
        /// Gets the transport lane used for this leg.
        /// </summary>
        public TransportLane Lane { get; }

        /// <summary>
        /// Gets the origin warehouse reference.
        /// </summary>
        public WarehouseReference Origin => Lane.Origin;

        /// <summary>
        /// Gets the destination warehouse reference.
        /// </summary>
        public WarehouseReference Destination =>
            Lane.Destination;

        /// <summary>
        /// Gets the transport lead time of this leg.
        /// </summary>
        public int LeadTime => Lane.LeadTime;

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                $"{TransportResource.Name}: " +
                $"{FormatWarehouse(Origin)} -> " +
                $"{FormatWarehouse(Destination)} " +
                $"({LeadTime} period(s))";
        }
    }

    /// <summary>
    /// Represents a complete transport path between
    /// two warehouses.
    /// </summary>
    public sealed class TransportPath
    {
        private readonly TransportLeg[] _legs;

        /// <summary>
        /// Initializes a complete transport path between
        /// two warehouses.
        /// </summary>
        /// <param name="origin">
        /// Origin warehouse reference.
        /// </param>
        /// <param name="destination">
        /// Destination warehouse reference.
        /// </param>
        /// <param name="legs">
        /// Ordered sequence of transport legs forming the path.
        /// </param>
        public TransportPath(
            WarehouseReference origin,
            WarehouseReference destination,
            IEnumerable<TransportLeg> legs)
        {
            Origin = origin ??
                throw new ArgumentNullException(nameof(origin));

            Destination = destination ??
                throw new ArgumentNullException(
                    nameof(destination));

            ArgumentNullException.ThrowIfNull(legs);

            _legs = legs.ToArray();

            TotalLeadTime = _legs.Sum(
                leg => leg.LeadTime);
        }

        /// <summary>
        /// Gets the path origin.
        /// </summary>
        public WarehouseReference Origin { get; }

        /// <summary>
        /// Gets the path destination.
        /// </summary>
        public WarehouseReference Destination { get; }

        /// <summary>
        /// Gets the ordered transport legs.
        /// </summary>
        public IReadOnlyList<TransportLeg> Legs =>
            _legs;

        /// <summary>
        /// Gets the number of transport legs.
        /// </summary>
        public int LegCount => _legs.Length;

        /// <summary>
        /// Gets the sum of the lead times of all transport legs.
        /// </summary>
        public int TotalLeadTime { get; }

        /// <summary>
        /// Gets a value indicating whether the origin and
        /// destination are the same warehouse.
        /// </summary>
        public bool IsEmpty => _legs.Length == 0;
        
        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsEmpty)
            {
                return
                    $"{FormatWarehouse(Origin)} " +
                    "(same origin and destination)";
            }

            string route = string.Join(
                " | ",
                _legs.Select(
                    leg => leg.ToString()));

            return
                $"{route} — total lead time: " +
                $"{TotalLeadTime} period(s)";
        }
    }
}