using System;
using System.Collections.Generic;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Indexing;

/// <summary>
/// Provides fast access to the entities contained in a supply chain.
///
/// The index is a technical runtime object and is not serialized.
/// It must be rebuilt when entities are added to or removed from
/// the underlying supply chain.
/// </summary>
public sealed class SupplyChainIndex
{
    private readonly Dictionary<int, Item> _items = new();
    private readonly Dictionary<int, Plant> _plants = new();

    private readonly Dictionary<int, StandaloneWarehouse>
        _standaloneWarehouses = new();

    private readonly Dictionary<
        (WarehouseReferenceKind Kind, int ReferenceId),
        Warehouse> _warehouses = new();

    private readonly Dictionary<
        (int PlantId, int WorkCenterId),
        WorkCenter> _workCenters = new();

    private readonly Dictionary<int, Supplier> _suppliers = new();

    private readonly Dictionary<int, DistributionCenter>
        _distributionCenters = new();

    private readonly Dictionary<int, TransportResource>
        _transportResources = new();

    /// <summary>
    /// Initializes an index for the specified supply chain.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain whose entities must be indexed.
    /// </param>
    public SupplyChainIndex(SupplyChain supplyChain)
    {
        SupplyChain = supplyChain ??
            throw new ArgumentNullException(nameof(supplyChain));

        Rebuild();
    }

    /// <summary>
    /// Gets the indexed supply chain.
    /// </summary>
    public SupplyChain SupplyChain { get; }

    /// <summary>
    /// Gets the number of indexed items.
    /// </summary>
    public int ItemCount => _items.Count;

    /// <summary>
    /// Gets the number of indexed plants.
    /// </summary>
    public int PlantCount => _plants.Count;

    /// <summary>
    /// Gets the number of indexed warehouses, including
    /// standalone warehouses and plant warehouses.
    /// </summary>
    public int WarehouseCount => _warehouses.Count;

    /// <summary>
    /// Gets the number of indexed work centers.
    /// </summary>
    public int WorkCenterCount => _workCenters.Count;

    /// <summary>
    /// Gets the number of indexed suppliers.
    /// </summary>
    public int SupplierCount => _suppliers.Count;

    /// <summary>
    /// Gets the number of indexed distribution centers.
    /// </summary>
    public int DistributionCenterCount =>
        _distributionCenters.Count;

    /// <summary>
    /// Gets the number of indexed transport resources.
    /// </summary>
    public int TransportResourceCount =>
        _transportResources.Count;

    /// <summary>
    /// Rebuilds every index from the current content of
    /// the supply chain.
    ///
    /// This method must be called after direct modifications
    /// of the supply-chain collections.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        // Rebuild all internal indexes from the current supply-chain state.
        IndexItems();
        IndexPlantsAndWorkCenters();
        IndexStandaloneWarehouses();
        IndexSuppliers();
        IndexDistributionCenters();
        IndexTransportResources();
    }

    /// <summary>
    /// Removes every entry from the index.
    /// </summary>
    private void Clear()
    {
        _items.Clear();
        _plants.Clear();
        _standaloneWarehouses.Clear();
        _warehouses.Clear();
        _workCenters.Clear();
        _suppliers.Clear();
        _distributionCenters.Clear();
        _transportResources.Clear();
    }

    #region Item resolution

    /// <summary>
    /// Attempts to resolve an item by its identifier.
    /// </summary>
    public bool TryGetItem(
        int itemId,
        out Item? item)
    {
        return _items.TryGetValue(
            itemId,
            out item);
    }

    /// <summary>
    /// Resolves an item by its identifier.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the item does not exist.
    /// </exception>
    public Item GetRequiredItem(int itemId)
    {
        return GetRequired(
            _items,
            itemId,
            $"Item {itemId} does not exist.");
    }

    #endregion

    #region Plant resolution

    /// <summary>
    /// Attempts to resolve a plant by its identifier.
    /// </summary>
    public bool TryGetPlant(
        int plantId,
        out Plant? plant)
    {
        return _plants.TryGetValue(
            plantId,
            out plant);
    }

    /// <summary>
    /// Resolves a plant by its identifier.
    /// </summary>
    public Plant GetRequiredPlant(int plantId)
    {
        return GetRequired(
            _plants,
            plantId,
            $"Plant {plantId} does not exist.");
    }

    #endregion

    #region Warehouse resolution

    /// <summary>
    /// Attempts to resolve a standalone warehouse
    /// by its own identifier.
    /// </summary>
    public bool TryGetStandaloneWarehouse(
        int warehouseId,
        out StandaloneWarehouse? warehouse)
    {
        return _standaloneWarehouses.TryGetValue(
            warehouseId,
            out warehouse);
    }

    /// <summary>
    /// Resolves a standalone warehouse by its own identifier.
    /// </summary>
    public StandaloneWarehouse
        GetRequiredStandaloneWarehouse(int warehouseId)
    {
        return GetRequired(
            _standaloneWarehouses,
            warehouseId,
            $"Standalone warehouse {warehouseId} does not exist.");
    }

    /// <summary>
    /// Attempts to resolve a warehouse from a warehouse reference.
    ///
    /// For a plant warehouse, ReferenceId contains the plant
    /// identifier. For a standalone warehouse, it contains the
    /// warehouse identifier.
    /// </summary>
    public bool TryGetWarehouse(
        WarehouseReference? reference,
        out Warehouse? warehouse)
    {
        if (reference is null)
        {
            warehouse = null;
            return false;
        }

        return _warehouses.TryGetValue(
            CreateWarehouseKey(reference),
            out warehouse);
    }

    /// <summary>
    /// Resolves a warehouse from a warehouse reference.
    /// </summary>
    public Warehouse GetRequiredWarehouse(
        WarehouseReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var key = CreateWarehouseKey(reference);

        return GetRequired(
            _warehouses,
            key,
            $"Warehouse {FormatWarehouseKey(key)} does not exist.");
    }

    /// <summary>
    /// Resolves the warehouse attached to a plant.
    /// </summary>
    public PlantWarehouse GetRequiredPlantWarehouse(
        int plantId)
    {
        Warehouse warehouse =
            GetRequired(
                _warehouses,
                (
                    WarehouseReferenceKind.PlantWarehouse,
                    plantId
                ),
                $"The warehouse of plant {plantId} does not exist.");

        if (warehouse is not PlantWarehouse plantWarehouse)
        {
            throw new InvalidOperationException(
                $"Warehouse reference PlantWarehouse:{plantId} " +
                "does not resolve to a plant warehouse.");
        }

        return plantWarehouse;
    }

    #endregion

    #region Work-center resolution

    /// <summary>
    /// Attempts to resolve a work center from a reference.
    /// </summary>
    public bool TryGetWorkCenter(
        WorkCenterReference? reference,
        out WorkCenter? workCenter)
    {
        if (reference is null)
        {
            workCenter = null;
            return false;
        }

        return TryGetWorkCenter(
            reference.PlantId,
            reference.WorkCenterId,
            out workCenter);
    }

    /// <summary>
    /// Attempts to resolve a work center from its plant
    /// and local work-center identifiers.
    /// </summary>
    public bool TryGetWorkCenter(
        int plantId,
        int workCenterId,
        out WorkCenter? workCenter)
    {
        return _workCenters.TryGetValue(
            (plantId, workCenterId),
            out workCenter);
    }

    /// <summary>
    /// Resolves a work center from a reference.
    /// </summary>
    public WorkCenter GetRequiredWorkCenter(
        WorkCenterReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        return GetRequiredWorkCenter(
            reference.PlantId,
            reference.WorkCenterId);
    }

    /// <summary>
    /// Resolves a work center from its plant and local
    /// work-center identifiers.
    /// </summary>
    public WorkCenter GetRequiredWorkCenter(
        int plantId,
        int workCenterId)
    {
        return GetRequired(
            _workCenters,
            (plantId, workCenterId),
            $"Work center {plantId}:{workCenterId} does not exist.");
    }

    #endregion

    #region Supplier resolution

    /// <summary>
    /// Attempts to resolve a supplier by its identifier.
    /// </summary>
    public bool TryGetSupplier(
        int supplierId,
        out Supplier? supplier)
    {
        return _suppliers.TryGetValue(
            supplierId,
            out supplier);
    }

    /// <summary>
    /// Resolves a supplier by its identifier.
    /// </summary>
    public Supplier GetRequiredSupplier(int supplierId)
    {
        return GetRequired(
            _suppliers,
            supplierId,
            $"Supplier {supplierId} does not exist.");
    }

    #endregion

    #region Distribution-center resolution

    /// <summary>
    /// Attempts to resolve a distribution center
    /// by its identifier.
    /// </summary>
    public bool TryGetDistributionCenter(
        int distributionCenterId,
        out DistributionCenter? distributionCenter)
    {
        return _distributionCenters.TryGetValue(
            distributionCenterId,
            out distributionCenter);
    }

    /// <summary>
    /// Resolves a distribution center by its identifier.
    /// </summary>
    public DistributionCenter GetRequiredDistributionCenter(
        int distributionCenterId)
    {
        return GetRequired(
            _distributionCenters,
            distributionCenterId,
            "Distribution center " +
            $"{distributionCenterId} does not exist.");
    }

    #endregion

    #region Transport-resource resolution

    /// <summary>
    /// Attempts to resolve a transport resource
    /// by its identifier.
    /// </summary>
    public bool TryGetTransportResource(
        int transportResourceId,
        out TransportResource? transportResource)
    {
        return _transportResources.TryGetValue(
            transportResourceId,
            out transportResource);
    }

    /// <summary>
    /// Resolves a transport resource by its identifier.
    /// </summary>
    public TransportResource GetRequiredTransportResource(
        int transportResourceId)
    {
        return GetRequired(
            _transportResources,
            transportResourceId,
            "Transport resource " +
            $"{transportResourceId} does not exist.");
    }

    #endregion

    #region Index construction

    private void IndexItems()
    {
        foreach (Item item in SupplyChain.Items)
        {
            AddUnique(
                _items,
                item.Id,
                item,
                "item");
        }
    }

    private void IndexPlantsAndWorkCenters()
    {
        foreach (Plant plant in SupplyChain.Plants)
        {
            AddUnique(
                _plants,
                plant.Id,
                plant,
                "plant");

            // Index the plant's warehouse by (PlantWarehouse, plantId).
            AddUnique(
                _warehouses,
                (
                    WarehouseReferenceKind.PlantWarehouse,
                    plant.Id
                ),
                plant.Warehouse,
                "plant warehouse");

            // Index each work center by (plantId, workCenterId).
            foreach (WorkCenter workCenter
                     in plant.WorkCenters)
            {
                AddUnique(
                    _workCenters,
                    (
                        plant.Id,
                        workCenter.Id
                    ),
                    workCenter,
                    "work center");
            }
        }
    }

    private void IndexStandaloneWarehouses()
    {
        foreach (StandaloneWarehouse warehouse
                 in SupplyChain.StandaloneWarehouses)
        {
            // Add to the standalone warehouse index by ID.
            AddUnique(
                _standaloneWarehouses,
                warehouse.Id,
                warehouse,
                "standalone warehouse");

            // Also add to the unified warehouse index by (StandaloneWarehouse, warehouseId).
            AddUnique(
                _warehouses,
                (
                    WarehouseReferenceKind.StandaloneWarehouse,
                    warehouse.Id
                ),
                warehouse,
                "warehouse");
        }
    }

    private void IndexSuppliers()
    {
        foreach (Supplier supplier in SupplyChain.Suppliers)
        {
            AddUnique(
                _suppliers,
                supplier.Id,
                supplier,
                "supplier");
        }
    }

    private void IndexDistributionCenters()
    {
        foreach (DistributionCenter distributionCenter
                 in SupplyChain.DistributionCenters)
        {
            AddUnique(
                _distributionCenters,
                distributionCenter.Id,
                distributionCenter,
                "distribution center");
        }
    }

    private void IndexTransportResources()
    {
        foreach (TransportResource transportResource
                 in SupplyChain.TransportResources)
        {
            AddUnique(
                _transportResources,
                transportResource.Id,
                transportResource,
                "transport resource");
        }
    }

    #endregion

    #region Helpers

    private static (
        WarehouseReferenceKind Kind,
        int ReferenceId)
        CreateWarehouseKey(WarehouseReference reference)
    {
        return (
            reference.Kind,
            reference.ReferenceId
        );
    }

    private static string FormatWarehouseKey(
        (
            WarehouseReferenceKind Kind,
            int ReferenceId
        ) key)
    {
        return $"{key.Kind}:{key.ReferenceId}";
    }

    private static void AddUnique<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value,
        string entityName)
        where TKey : notnull
        where TValue : class
    {
        ArgumentNullException.ThrowIfNull(value);

        // Fail fast if the key already exists in the index.
        if (!dictionary.TryAdd(key, value))
        {
            throw new InvalidOperationException(
                $"Cannot build the supply-chain index because " +
                $"the {entityName} key '{key}' is duplicated.");
        }
    }

    private static TValue GetRequired<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key,
        string errorMessage)
        where TKey : notnull
        where TValue : class
    {
        if (dictionary.TryGetValue(
                key,
                out TValue? value))
        {
            return value;
        }

        // Throw a descriptive exception when the key is not found.
        throw new KeyNotFoundException(errorMessage);
    }

    #endregion
}