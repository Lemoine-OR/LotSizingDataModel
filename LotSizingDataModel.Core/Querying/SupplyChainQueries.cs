using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Querying;

/// <summary>
/// Provides high-level read-only queries over a supply-chain model.
///
/// Entity references are resolved through <see cref="SupplyChainIndex"/>.
/// Relationship collections are queried directly from the current
/// supply-chain model.
/// </summary>
public sealed class SupplyChainQueries
{
    /// <summary>
    /// Initializes query services for a supply chain and creates
    /// a new entity index.
    /// </summary>
    public SupplyChainQueries(SupplyChain supplyChain)
        : this(new SupplyChainIndex(
            supplyChain ??
            throw new ArgumentNullException(nameof(supplyChain))))
    {
    }

    /// <summary>
    /// Initializes query services using an existing index.
    /// </summary>
    public SupplyChainQueries(SupplyChainIndex index)
    {
        Index = index ??
            throw new ArgumentNullException(nameof(index));

        SupplyChain = index.SupplyChain;
    }

    /// <summary>
    /// Gets the queried supply chain.
    /// </summary>
    public SupplyChain SupplyChain { get; }

    /// <summary>
    /// Gets the entity index used to resolve references.
    /// </summary>
    public SupplyChainIndex Index { get; }

    /// <summary>
    /// Rebuilds the underlying entity index.
    ///
    /// Call this method after directly adding or removing entities
    /// from the supply-chain collections.
    /// </summary>
    public void RebuildIndex()
    {
        Index.Rebuild();
    }

    #region Inventory queries

    /// <summary>
    /// Finds the inventory associated with an item and a warehouse.
    ///
    /// Returns null when the relationship does not exist.
    /// </summary>
    public Inventory? FindInventory(
        int itemId,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        return SupplyChain.Inventories.FirstOrDefault(
            inventory =>
                inventory.ItemId == itemId &&
                SameWarehouse(
                    inventory.Warehouse,
                    warehouse));
    }

    /// <summary>
    /// Gets the inventory associated with an item and a warehouse.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the inventory relationship does not exist.
    /// </exception>
    public Inventory GetRequiredInventory(
        int itemId,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        return FindInventory(itemId, warehouse) ??
            throw new KeyNotFoundException(
                $"No inventory exists for item {itemId} and " +
                $"warehouse {FormatWarehouse(warehouse)}.");
    }

    /// <summary>
    /// Gets all inventories defined for an item.
    /// </summary>
    public IReadOnlyList<Inventory> GetInventoriesForItem(
        int itemId)
    {
        Index.GetRequiredItem(itemId);

        return SupplyChain.Inventories
            .Where(inventory => inventory.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all inventories defined in a warehouse.
    /// </summary>
    public IReadOnlyList<Inventory> GetInventoriesForWarehouse(
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        Index.GetRequiredWarehouse(warehouse);

        return SupplyChain.Inventories
            .Where(
                inventory =>
                    SameWarehouse(
                        inventory.Warehouse,
                        warehouse))
            .ToArray();
    }

    /// <summary>
    /// Gets the items that can be stored in a warehouse.
    /// </summary>
    public IReadOnlyList<Item> GetItemsStoredInWarehouse(
        WarehouseReference warehouse)
    {
        return GetInventoriesForWarehouse(warehouse)
            .Select(
                inventory =>
                    Index.GetRequiredItem(inventory.ItemId))
            .ToArray();
    }

    #endregion

    #region Production-routing queries

    /// <summary>
    /// Finds a production routing by its identifier.
    /// </summary>
    public ProductionRouting? FindProductionRouting(
        int routingId)
    {
        return SupplyChain.ProductionRoutings.FirstOrDefault(
            routing => routing.Id == routingId);
    }

    /// <summary>
    /// Gets a production routing by its identifier.
    /// </summary>
    public ProductionRouting GetRequiredProductionRouting(
        int routingId)
    {
        return FindProductionRouting(routingId) ??
            throw new KeyNotFoundException(
                $"Production routing {routingId} does not exist.");
    }

    /// <summary>
    /// Gets all production routings available for an item.
    /// </summary>
    public IReadOnlyList<ProductionRouting>
        GetProductionRoutingsForItem(int itemId)
    {
        Index.GetRequiredItem(itemId);

        return SupplyChain.ProductionRoutings
            .Where(routing => routing.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all production routings belonging to a plant.
    /// </summary>
    public IReadOnlyList<ProductionRouting>
        GetProductionRoutingsForPlant(int plantId)
    {
        Index.GetRequiredPlant(plantId);

        return SupplyChain.ProductionRoutings
            .Where(routing => routing.PlantId == plantId)
            .ToArray();
    }

    /// <summary>
    /// Gets all production routings for an item in a plant.
    /// </summary>
    public IReadOnlyList<ProductionRouting>
        GetProductionRoutings(
            int itemId,
            int plantId)
    {
        Index.GetRequiredItem(itemId);
        Index.GetRequiredPlant(plantId);

        return SupplyChain.ProductionRoutings
            .Where(
                routing =>
                    routing.ItemId == itemId &&
                    routing.PlantId == plantId)
            .ToArray();
    }

    #endregion

    #region Production-characteristic queries

    /// <summary>
    /// Finds the production characteristic associated with
    /// an item and a work center.
    /// </summary>
    public ProductionCharacteristic?
        FindProductionCharacteristic(
            int itemId,
            WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        return SupplyChain.ProductionCharacteristics
            .FirstOrDefault(
                characteristic =>
                    characteristic.ItemId == itemId &&
                    SameWorkCenter(
                        characteristic.WorkCenter,
                        workCenter));
    }

    /// <summary>
    /// Gets the production characteristic associated with
    /// an item and a work center.
    /// </summary>
    public ProductionCharacteristic
        GetRequiredProductionCharacteristic(
            int itemId,
            WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        return FindProductionCharacteristic(
                   itemId,
                   workCenter) ??
            throw new KeyNotFoundException(
                $"No production characteristic exists for " +
                $"item {itemId} and work center " +
                $"{FormatWorkCenter(workCenter)}.");
    }

    /// <summary>
    /// Gets all production characteristics defined for an item.
    /// </summary>
    public IReadOnlyList<ProductionCharacteristic>
        GetProductionCharacteristicsForItem(int itemId)
    {
        Index.GetRequiredItem(itemId);

        return SupplyChain.ProductionCharacteristics
            .Where(
                characteristic =>
                    characteristic.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all production characteristics using a work center.
    /// </summary>
    public IReadOnlyList<ProductionCharacteristic>
        GetProductionCharacteristicsForWorkCenter(
            WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        Index.GetRequiredWorkCenter(workCenter);

        return SupplyChain.ProductionCharacteristics
            .Where(
                characteristic =>
                    SameWorkCenter(
                        characteristic.WorkCenter,
                        workCenter))
            .ToArray();
    }

    /// <summary>
    /// Gets all items that may be produced on a work center.
    /// </summary>
    public IReadOnlyList<Item> GetItemsProducedByWorkCenter(
        WorkCenterReference workCenter)
    {
        return GetProductionCharacteristicsForWorkCenter(
                workCenter)
            .Select(
                characteristic =>
                    Index.GetRequiredItem(
                        characteristic.ItemId))
            .ToArray();
    }

    #endregion

    #region Demand queries

    /// <summary>
    /// Finds the demand associated with an item and
    /// a distribution center.
    /// </summary>
    public Demand? FindDemand(
        int itemId,
        int distributionCenterId)
    {
        return SupplyChain.Demands.FirstOrDefault(
            demand =>
                demand.ItemId == itemId &&
                demand.DistributionCenterId ==
                    distributionCenterId);
    }

    /// <summary>
    /// Gets the demand associated with an item and
    /// a distribution center.
    /// </summary>
    public Demand GetRequiredDemand(
        int itemId,
        int distributionCenterId)
    {
        return FindDemand(
                   itemId,
                   distributionCenterId) ??
            throw new KeyNotFoundException(
                $"No demand exists for item {itemId} and " +
                $"distribution center {distributionCenterId}.");
    }

    /// <summary>
    /// Gets all demands for an item.
    /// </summary>
    public IReadOnlyList<Demand> GetDemandsForItem(
        int itemId)
    {
        Index.GetRequiredItem(itemId);

        return SupplyChain.Demands
            .Where(demand => demand.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all demands expressed by a distribution center.
    /// </summary>
    public IReadOnlyList<Demand>
        GetDemandsForDistributionCenter(
            int distributionCenterId)
    {
        Index.GetRequiredDistributionCenter(
            distributionCenterId);

        return SupplyChain.Demands
            .Where(
                demand =>
                    demand.DistributionCenterId ==
                        distributionCenterId)
            .ToArray();
    }

    /// <summary>
    /// Gets the demand quantity for a specific period.
    /// </summary>
    public double GetDemandQuantity(
        int itemId,
        int distributionCenterId,
        int period)
    {
        ValidatePeriod(period);

        return GetRequiredDemand(
                itemId,
                distributionCenterId)
            .GetQuantity(period);
    }

    #endregion

    #region Distribution-center sourcing queries

    /// <summary>
    /// Gets all warehouse sourcing options available for
    /// an item and a distribution center.
    /// </summary>
    public IReadOnlyList<DistributionCenterSourcing>
        GetSourcingOptions(
            int distributionCenterId,
            int itemId)
    {
        Index.GetRequiredDistributionCenter(
            distributionCenterId);

        Index.GetRequiredItem(itemId);

        return SupplyChain.DistributionCenterSourcings
            .Where(
                sourcing =>
                    sourcing.DistributionCenterId ==
                        distributionCenterId &&
                    sourcing.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all sourcing relationships using a warehouse.
    /// </summary>
    public IReadOnlyList<DistributionCenterSourcing>
        GetSourcingsFromWarehouse(
            WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        Index.GetRequiredWarehouse(warehouse);

        return SupplyChain.DistributionCenterSourcings
            .Where(
                sourcing =>
                    SameWarehouse(
                        sourcing.Warehouse,
                        warehouse))
            .ToArray();
    }

    /// <summary>
    /// Gets the warehouses that can supply an item to
    /// a distribution center.
    /// </summary>
    public IReadOnlyList<Warehouse>
        GetSupplyingWarehouses(
            int distributionCenterId,
            int itemId)
    {
        return GetSourcingOptions(
                distributionCenterId,
                itemId)
            .Select(
                sourcing =>
                    Index.GetRequiredWarehouse(
                        sourcing.Warehouse))
            .ToArray();
    }

    #endregion

    #region Supplier-delivery queries

    /// <summary>
    /// Gets all delivery possibilities offered by a supplier.
    /// </summary>
    public IReadOnlyList<SupplierDelivery>
        GetDeliveriesFromSupplier(int supplierId)
    {
        Index.GetRequiredSupplier(supplierId);

        return SupplyChain.SupplierDeliveries
            .Where(
                delivery =>
                    delivery.SupplierId == supplierId)
            .ToArray();
    }

    /// <summary>
    /// Gets all supplier deliveries available for an item.
    /// </summary>
    public IReadOnlyList<SupplierDelivery>
        GetDeliveriesForItem(int itemId)
    {
        Index.GetRequiredItem(itemId);

        return SupplyChain.SupplierDeliveries
            .Where(delivery => delivery.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all supplier deliveries whose destination is
    /// the specified warehouse.
    /// </summary>
    public IReadOnlyList<SupplierDelivery>
        GetDeliveriesToWarehouse(
            WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        Index.GetRequiredWarehouse(warehouse);

        return SupplyChain.SupplierDeliveries
            .Where(
                delivery =>
                    SameWarehouse(
                        delivery.Warehouse,
                        warehouse))
            .ToArray();
    }

    /// <summary>
    /// Gets all suppliers able to deliver an item
    /// to a warehouse.
    /// </summary>
    public IReadOnlyList<Supplier>
        GetSuppliersForInventory(
            int itemId,
            WarehouseReference warehouse)
    {
        GetRequiredInventory(itemId, warehouse);

        return SupplyChain.SupplierDeliveries
            .Where(
                delivery =>
                    delivery.ItemId == itemId &&
                    SameWarehouse(
                        delivery.Warehouse,
                        warehouse))
            .Select(
                delivery =>
                    Index.GetRequiredSupplier(
                        delivery.SupplierId))
            .ToArray();
    }

    #endregion

    #region Transport-characteristic queries

    /// <summary>
    /// Finds the transport characteristic associated with
    /// an item and a transport resource.
    /// </summary>
    public TransportCharacteristic?
        FindTransportCharacteristic(
            int itemId,
            int transportResourceId)
    {
        return SupplyChain.TransportCharacteristics
            .FirstOrDefault(
                characteristic =>
                    characteristic.ItemId == itemId &&
                    characteristic.TransportResourceId ==
                        transportResourceId);
    }

    /// <summary>
    /// Gets the transport characteristic associated with
    /// an item and a transport resource.
    /// </summary>
    public TransportCharacteristic
        GetRequiredTransportCharacteristic(
            int itemId,
            int transportResourceId)
    {
        return FindTransportCharacteristic(
                   itemId,
                   transportResourceId) ??
            throw new KeyNotFoundException(
                $"No transport characteristic exists for " +
                $"item {itemId} and transport resource " +
                $"{transportResourceId}.");
    }

    /// <summary>
    /// Gets all transport characteristics available for an item.
    /// </summary>
    public IReadOnlyList<TransportCharacteristic>
        GetTransportCharacteristicsForItem(int itemId)
    {
        Index.GetRequiredItem(itemId);

        return SupplyChain.TransportCharacteristics
            .Where(
                characteristic =>
                    characteristic.ItemId == itemId)
            .ToArray();
    }

    /// <summary>
    /// Gets all items compatible with a transport resource.
    /// </summary>
    public IReadOnlyList<Item>
        GetItemsTransportedByResource(
            int transportResourceId)
    {
        Index.GetRequiredTransportResource(
            transportResourceId);

        return SupplyChain.TransportCharacteristics
            .Where(
                characteristic =>
                    characteristic.TransportResourceId ==
                        transportResourceId)
            .Select(
                characteristic =>
                    Index.GetRequiredItem(
                        characteristic.ItemId))
            .ToArray();
    }

    #endregion

    #region Transport-lane queries

    /// <summary>
    /// Gets all direct transport options leaving a warehouse.
    /// </summary>
    public IReadOnlyList<DirectTransportOption>
        GetOutgoingTransportOptions(
            WarehouseReference origin)
    {
        ArgumentNullException.ThrowIfNull(origin);

        Index.GetRequiredWarehouse(origin);

        return EnumerateTransportOptions()
            .Where(
                option =>
                    SameWarehouse(
                        option.Lane.Origin,
                        origin))
            .ToArray();
    }

    /// <summary>
    /// Gets all direct transport options arriving at a warehouse.
    /// </summary>
    public IReadOnlyList<DirectTransportOption>
        GetIncomingTransportOptions(
            WarehouseReference destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        Index.GetRequiredWarehouse(destination);

        return EnumerateTransportOptions()
            .Where(
                option =>
                    SameWarehouse(
                        option.Lane.Destination,
                        destination))
            .ToArray();
    }

    /// <summary>
    /// Gets all direct transport options between two warehouses.
    /// </summary>
    public IReadOnlyList<DirectTransportOption>
        GetDirectTransportOptions(
            WarehouseReference origin,
            WarehouseReference destination)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        Index.GetRequiredWarehouse(origin);
        Index.GetRequiredWarehouse(destination);

        return EnumerateTransportOptions()
            .Where(
                option =>
                    SameWarehouse(
                        option.Lane.Origin,
                        origin) &&
                    SameWarehouse(
                        option.Lane.Destination,
                        destination))
            .ToArray();
    }

    /// <summary>
    /// Determines whether at least one direct transport option
    /// exists between two warehouses.
    /// </summary>
    public bool HasDirectTransportConnection(
        WarehouseReference origin,
        WarehouseReference destination)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        return GetDirectTransportOptions(
            origin,
            destination).Count > 0;
    }

    /// <summary>
    /// Gets the direct transport options that can carry
    /// a specified item.
    /// </summary>
    public IReadOnlyList<DirectTransportOption>
        GetDirectTransportOptionsForItem(
            int itemId,
            WarehouseReference origin,
            WarehouseReference destination)
    {
        Index.GetRequiredItem(itemId);

        // Build the set of transport resources compatible with the item.
        HashSet<int> compatibleResourceIds =
            SupplyChain.TransportCharacteristics
                .Where(
                    characteristic =>
                        characteristic.ItemId == itemId)
                .Select(
                    characteristic =>
                        characteristic.TransportResourceId)
                .ToHashSet();

        return GetDirectTransportOptions(
                origin,
                destination)
            .Where(
                option =>
                    compatibleResourceIds.Contains(
                        option.TransportResource.Id))
            .ToArray();
    }

    private IEnumerable<DirectTransportOption>
        EnumerateTransportOptions()
    {
        foreach (TransportResource resource
                 in SupplyChain.TransportResources)
        {
            foreach (TransportLane lane in resource.Lanes)
            {
                yield return new DirectTransportOption(
                    resource,
                    lane);
            }
        }
    }

    #endregion

    #region Capacity queries

    /// <summary>
    /// Gets the capacity available on a work center
    /// during a planning period.
    /// </summary>
    public CapacitySnapshot GetWorkCenterCapacity(
        WorkCenterReference workCenter,
        int period)
    {
        ArgumentNullException.ThrowIfNull(workCenter);
        ValidatePeriod(period);

        WorkCenter resource =
            Index.GetRequiredWorkCenter(workCenter);

        return CreateCapacitySnapshot(
            resource.CapacityConstraint,
            resource.AdditionalCapacity,
            period,
            isItemSpecific: false);
    }

    /// <summary>
    /// Gets the global capacity of a warehouse
    /// during a planning period.
    /// </summary>
    public CapacitySnapshot GetWarehouseCapacity(
        WarehouseReference warehouse,
        int period)
    {
        ArgumentNullException.ThrowIfNull(warehouse);
        ValidatePeriod(period);

        Warehouse resource =
            Index.GetRequiredWarehouse(warehouse);

        return CreateCapacitySnapshot(
            resource.CapacityConstraint,
            resource.AdditionalCapacity,
            period,
            isItemSpecific: false);
    }

    /// <summary>
    /// Gets the capacity applicable to a specific inventory.
    ///
    /// An item-specific capacity is used when it exists.
    /// Otherwise, the global warehouse capacity is returned.
    /// </summary>
    public CapacitySnapshot GetInventoryCapacity(
        int itemId,
        WarehouseReference warehouse,
        int period)
    {
        ArgumentNullException.ThrowIfNull(warehouse);
        ValidatePeriod(period);

        Inventory inventory =
            GetRequiredInventory(
                itemId,
                warehouse);

        // Use item-specific capacity if defined; otherwise fall back to global warehouse capacity.
        if (inventory.CapacityConstraint is not null)
        {
            return CreateCapacitySnapshot(
                inventory.CapacityConstraint,
                inventory.AdditionalCapacity,
                period,
                isItemSpecific: true);
        }

        return GetWarehouseCapacity(
            warehouse,
            period);
    }

    /// <summary>
    /// Gets the global capacity of a transport resource
    /// during a planning period.
    /// </summary>
    public CapacitySnapshot GetTransportResourceCapacity(
        int transportResourceId,
        int period)
    {
        ValidatePeriod(period);

        TransportResource resource =
            Index.GetRequiredTransportResource(
                transportResourceId);

        return CreateCapacitySnapshot(
            resource.CapacityConstraint,
            resource.AdditionalCapacity,
            period,
            isItemSpecific: false);
    }

    /// <summary>
    /// Gets the capacity applicable to an item transported
    /// by a transport resource.
    ///
    /// An item-specific capacity is used when it exists.
    /// Otherwise, the global transport-resource capacity is returned.
    /// </summary>
    public CapacitySnapshot GetTransportCapacity(
        int itemId,
        int transportResourceId,
        int period)
    {
        ValidatePeriod(period);

        TransportCharacteristic characteristic =
            GetRequiredTransportCharacteristic(
                itemId,
                transportResourceId);

        // Use item-specific capacity if defined; otherwise fall back to global resource capacity.
        if (characteristic.CapacityConstraint is not null)
        {
            return CreateCapacitySnapshot(
                characteristic.CapacityConstraint,
                characteristic.AdditionalCapacity,
                period,
                isItemSpecific: true);
        }

        return GetTransportResourceCapacity(
            transportResourceId,
            period);
    }

    private static CapacitySnapshot CreateCapacitySnapshot(
        CapacityConstraint? regularCapacity,
        AdditionalCapacity? additionalCapacity,
        int period,
        bool isItemSpecific)
    {
        double? maximumRegularCapacity =
            regularCapacity?.GetMaximumCapacity(period);

        double? maximumAdditionalCapacity =
            additionalCapacity
                ?.GetMaximumAdditionalCapacity(period);

        return new CapacitySnapshot(
            maximumRegularCapacity,
            maximumAdditionalCapacity,
            isItemSpecific);
    }

    #endregion

    #region Helpers

    private void ValidatePeriod(int period)
    {
        if (period < 1 ||
            period > SupplyChain.PlanningHorizon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(period),
                period,
                $"The period must be between 1 and " +
                $"{SupplyChain.PlanningHorizon}.");
        }
    }

    private static bool SameWarehouse(
        WarehouseReference? first,
        WarehouseReference? second)
    {
        // Null-safe: both null = equal, only one null = not equal.
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return first.Kind == second.Kind &&
               first.ReferenceId == second.ReferenceId;
    }

    private static bool SameWorkCenter(
        WorkCenterReference? first,
        WorkCenterReference? second)
    {
        // Null-safe: both null = equal, only one null = not equal.
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return first.PlantId == second.PlantId &&
               first.WorkCenterId == second.WorkCenterId;
    }

    private static string FormatWarehouse(
        WarehouseReference reference)
    {
        return $"{reference.Kind}:{reference.ReferenceId}";
    }

    private static string FormatWorkCenter(
        WorkCenterReference reference)
    {
        return
            $"{reference.PlantId}:{reference.WorkCenterId}";
    }

    #endregion

    /// <summary>
    /// Represents one direct transport possibility between
    /// two warehouses.
    /// </summary>
    public sealed class DirectTransportOption
    {
        /// <summary>
        /// Initializes a direct transport option associating
        /// a transport resource with one of its lanes.
        /// </summary>
        /// <param name="transportResource">
        /// Transport resource performing the movement.
        /// </param>
        /// <param name="lane">
        /// Directed transport lane used by the option.
        /// </param>
        public DirectTransportOption(
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
        /// Gets the transport resource performing the movement.
        /// </summary>
        public TransportResource TransportResource { get; }

        /// <summary>
        /// Gets the origin, destination and lead time.
        /// </summary>
        public TransportLane Lane { get; }

        /// <summary>
        /// Gets the direct-transport lead time.
        /// </summary>
        public int LeadTime => Lane.LeadTime;

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                $"{TransportResource.Name}: " +
                $"{FormatWarehouse(Lane.Origin)} -> " +
                $"{FormatWarehouse(Lane.Destination)} " +
                $"({LeadTime} period(s))";
        }
    }

    /// <summary>
    /// Represents the maximum regular and additional capacity
    /// available during one planning period.
    /// </summary>
    public sealed class CapacitySnapshot
    {
        /// <summary>
        /// Initializes a capacity snapshot for one resource
        /// and one planning period.
        /// </summary>
        /// <param name="maximumRegularCapacity">
        /// Maximum regular capacity,
        /// or null when no regular-capacity constraint exists.
        /// </param>
        /// <param name="maximumAdditionalCapacity">
        /// Maximum additional capacity,
        /// or null when no additional capacity is defined.
        /// </param>
        /// <param name="isItemSpecific">
        /// Indicates whether the capacity belongs to an
        /// item-specific relationship.
        /// </param>
        public CapacitySnapshot(
            double? maximumRegularCapacity,
            double? maximumAdditionalCapacity,
            bool isItemSpecific)
        {
            MaximumRegularCapacity =
                maximumRegularCapacity;

            MaximumAdditionalCapacity =
                maximumAdditionalCapacity;

            IsItemSpecific = isItemSpecific;
        }

        /// <summary>
        /// Gets the regular-capacity limit.
        ///
        /// Null means that no regular-capacity constraint is defined.
        /// </summary>
        public double? MaximumRegularCapacity { get; }

        /// <summary>
        /// Gets the additional-capacity limit.
        ///
        /// Null means that no additional capacity is defined.
        /// </summary>
        public double? MaximumAdditionalCapacity { get; }

        /// <summary>
        /// Gets a value indicating whether the capacity is defined
        /// specifically for an item-resource relationship.
        /// </summary>
        public bool IsItemSpecific { get; }

        /// <summary>
        /// Gets a value indicating whether a regular-capacity
        /// constraint is active.
        /// </summary>
        public bool HasRegularCapacity =>
            MaximumRegularCapacity.HasValue;

        /// <summary>
        /// Gets a value indicating whether additional capacity
        /// is available.
        /// </summary>
        public bool HasAdditionalCapacity =>
            MaximumAdditionalCapacity.HasValue;

        /// <summary>
        /// Gets the total maximum capacity.
        ///
        /// Returns null when no regular-capacity constraint exists.
        /// </summary>
        public double? TotalMaximumCapacity =>
            MaximumRegularCapacity.HasValue
                ? MaximumRegularCapacity.Value +
                  (MaximumAdditionalCapacity ?? 0.0)
                : null;

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!MaximumRegularCapacity.HasValue)
            {
                return "No capacity constraint";
            }

            return
                $"Regular: {MaximumRegularCapacity.Value}; " +
                $"Additional: " +
                $"{MaximumAdditionalCapacity ?? 0.0}; " +
                $"Total: {TotalMaximumCapacity}";
        }
    }
}