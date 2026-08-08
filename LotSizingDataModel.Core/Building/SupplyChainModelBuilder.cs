using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Core.Validation;

namespace LotSizingDataModel.Core.Building;

/// <summary>
/// Provides a fluent and controlled way to construct
/// a complete supply-chain model.
///
/// The builder:
/// - preserves the global planning horizon;
/// - checks references before adding relationships;
/// - prevents the use of unknown entities;
/// - delegates duplicate detection to SupplyChain;
/// - validates the complete model before returning it.
/// </summary>
public sealed class SupplyChainModelBuilder
{
    private readonly SupplyChainValidator _validator;
    private readonly SupplyChainIndex _index;

    /// <summary>
    /// Initializes an empty supply-chain builder.
    /// </summary>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public SupplyChainModelBuilder(int planningHorizon)
        : this(
            new SupplyChain(planningHorizon),
            new SupplyChainValidator())
    {
    }

    /// <summary>
    /// Initializes a builder for an existing supply chain.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain to extend.
    /// </param>
    public SupplyChainModelBuilder(
        SupplyChain supplyChain)
        : this(
            supplyChain,
            new SupplyChainValidator())
    {
    }

    /// <summary>
    /// Initializes a builder with a supply chain
    /// and a custom validator.
    /// </summary>
    public SupplyChainModelBuilder(
        SupplyChain supplyChain,
        SupplyChainValidator validator)
    {
        SupplyChain = supplyChain ??
            throw new ArgumentNullException(
                nameof(supplyChain));

        _validator = validator ??
            throw new ArgumentNullException(
                nameof(validator));

        if (SupplyChain.PlanningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(supplyChain),
                SupplyChain.PlanningHorizon,
                "The planning horizon must be strictly positive.");
        }

        _index = new SupplyChainIndex(SupplyChain);
    }

    /// <summary>
    /// Gets the supply chain currently being constructed.
    /// </summary>
    public SupplyChain SupplyChain { get; }

    /// <summary>
    /// Gets the current global planning horizon.
    /// </summary>
    public int PlanningHorizon =>
        SupplyChain.PlanningHorizon;

    /// <summary>
    /// Changes the global planning horizon.
    ///
    /// Every active time series is automatically resized.
    /// </summary>
    public SupplyChainModelBuilder SetPlanningHorizon(
        int planningHorizon)
    {
        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        SupplyChain.PlanningHorizon =
            planningHorizon;

        return this;
    }

    #region Logical subsystem

    /// <summary>
    /// Adds an item to the logical subsystem.
    /// </summary>
    public SupplyChainModelBuilder AddItem(
        Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        SupplyChain.AddItem(item);
        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds an item.
    /// </summary>
    public SupplyChainModelBuilder AddItem(
        int id,
        string name,
        int billOfMaterialsLevel = 0)
    {
        return AddItem(
            new Item(
                id,
                name,
                billOfMaterialsLevel));
    }

    /// <summary>
    /// Adds a bill-of-material component requirement.
    ///
    /// Both items must already exist.
    /// </summary>
    public SupplyChainModelBuilder
        AddComponentRequirement(
            ComponentRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(requirement);

        _index.GetRequiredItem(
            requirement.ParentItemId);

        _index.GetRequiredItem(
            requirement.ComponentItemId);

        if (requirement.ParentItemId ==
            requirement.ComponentItemId)
        {
            throw new InvalidOperationException(
                "An item cannot be its own component.");
        }

        if (requirement.Quantity <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requirement),
                requirement.Quantity,
                "A component quantity must be strictly positive.");
        }

        SupplyChain.AddComponentRequirement(
            requirement);

        return this;
    }

    /// <summary>
    /// Creates and adds a bill-of-material requirement.
    /// </summary>
    public SupplyChainModelBuilder AddComponentRequirement(
        int parentItemId,
        int componentItemId,
        int quantity)
    {
        return AddComponentRequirement(
            new ComponentRequirement
            {
                ParentItemId = parentItemId,
                ComponentItemId = componentItemId,
                Quantity = quantity
            });
    }

    #endregion

    #region Physical subsystem

    /// <summary>
    /// Adds a plant.
    ///
    /// Work centers may already be contained in the plant
    /// or may be added later through AddWorkCenter.
    /// </summary>
    public SupplyChainModelBuilder AddPlant(
        Plant plant)
    {
        ArgumentNullException.ThrowIfNull(plant);

        SupplyChain.AddPlant(plant);
        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds an empty plant.
    /// </summary>
    public SupplyChainModelBuilder AddPlant(
        int id,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The plant name cannot be empty.",
                nameof(name));
        }

        return AddPlant(
            new Plant
            {
                Id = id,
                Name = name
            });
    }

    /// <summary>
    /// Adds a work center to an existing plant.
    /// </summary>
    public SupplyChainModelBuilder AddWorkCenter(
        int plantId,
        WorkCenter workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        Plant plant =
            _index.GetRequiredPlant(plantId);

        plant.AddWorkCenter(workCenter);

        workCenter.ResizeTimeSeries(
            PlanningHorizon);

        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds a work center to an existing plant.
    /// </summary>
    public SupplyChainModelBuilder AddWorkCenter(
        int plantId,
        int workCenterId,
        string name)
    {
        return AddWorkCenter(
            plantId,
            new WorkCenter(
                workCenterId,
                name));
    }

    /// <summary>
    /// Adds a standalone warehouse.
    /// </summary>
    public SupplyChainModelBuilder
        AddStandaloneWarehouse(
            StandaloneWarehouse warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        SupplyChain.AddStandaloneWarehouse(
            warehouse);

        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds a standalone warehouse.
    /// </summary>
    public SupplyChainModelBuilder
        AddStandaloneWarehouse(
            int id,
            string name)
    {
        return AddStandaloneWarehouse(
            new StandaloneWarehouse(id, name));
    }

    /// <summary>
    /// Adds a supplier.
    /// </summary>
    public SupplyChainModelBuilder AddSupplier(
        Supplier supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);

        SupplyChain.AddSupplier(supplier);
        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds a supplier.
    /// </summary>
    public SupplyChainModelBuilder AddSupplier(
        int id,
        string name)
    {
        return AddSupplier(
            new Supplier(id, name));
    }

    /// <summary>
    /// Adds a distribution center.
    /// </summary>
    public SupplyChainModelBuilder
        AddDistributionCenter(
            DistributionCenter distributionCenter)
    {
        ArgumentNullException.ThrowIfNull(
            distributionCenter);

        SupplyChain.AddDistributionCenter(
            distributionCenter);

        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds a distribution center.
    /// </summary>
    public SupplyChainModelBuilder
        AddDistributionCenter(
            int id,
            string name)
    {
        return AddDistributionCenter(
            new DistributionCenter(id, name));
    }

    /// <summary>
    /// Adds a transport resource.
    ///
    /// Existing lanes are checked before the resource is added.
    /// </summary>
    public SupplyChainModelBuilder
        AddTransportResource(
            TransportResource transportResource)
    {
        ArgumentNullException.ThrowIfNull(
            transportResource);

        foreach (TransportLane lane
                 in transportResource.Lanes)
        {
            ValidateTransportLane(lane);
        }

        SupplyChain.AddTransportResource(
            transportResource);

        RebuildIndex();

        return this;
    }

    /// <summary>
    /// Creates and adds an empty transport resource.
    /// </summary>
    public SupplyChainModelBuilder
        AddTransportResource(
            int id,
            string name)
    {
        return AddTransportResource(
            new TransportResource(id, name));
    }

    /// <summary>
    /// Adds a lane to an existing transport resource.
    ///
    /// Both warehouses must already exist.
    /// </summary>
    public SupplyChainModelBuilder AddTransportLane(
        int transportResourceId,
        TransportLane lane)
    {
        ArgumentNullException.ThrowIfNull(lane);

        ValidateTransportLane(lane);

        TransportResource transportResource =
            _index.GetRequiredTransportResource(
                transportResourceId);

        transportResource.AddLane(lane);

        return this;
    }

    #endregion

    #region Production relationships

    /// <summary>
    /// Adds a production routing.
    ///
    /// The item, plant and every referenced work center
    /// must already exist.
    /// </summary>
    public SupplyChainModelBuilder
        AddProductionRouting(
            ProductionRouting routing)
    {
        ArgumentNullException.ThrowIfNull(routing);

        _index.GetRequiredItem(routing.ItemId);

        _index.GetRequiredPlant(routing.PlantId);

        foreach (WorkCenterReference reference
                 in routing.WorkCenters)
        {
            if (reference.PlantId != routing.PlantId)
            {
                throw new InvalidOperationException(
                    "Every work center referenced by a routing " +
                    "must belong to the routing plant.");
            }

            _index.GetRequiredWorkCenter(reference);
        }

        SupplyChain.AddProductionRouting(routing);

        return this;
    }

    /// <summary>
    /// Adds an item-work-center production characteristic.
    ///
    /// The item and work center must already exist.
    /// </summary>
    public SupplyChainModelBuilder
        AddProductionCharacteristic(
            ProductionCharacteristic characteristic)
    {
        ArgumentNullException.ThrowIfNull(
            characteristic);

        _index.GetRequiredItem(
            characteristic.ItemId);

        if (characteristic.WorkCenter is null)
        {
            throw new InvalidOperationException(
                "A production characteristic must reference " +
                "a work center.");
        }

        _index.GetRequiredWorkCenter(
            characteristic.WorkCenter);

        SupplyChain.AddProductionCharacteristic(
            characteristic);

        return this;
    }

    #endregion

    #region Inventory relationships

    /// <summary>
    /// Adds an item-warehouse inventory.
    ///
    /// The item and warehouse must already exist.
    /// </summary>
    public SupplyChainModelBuilder AddInventory(
        Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        _index.GetRequiredItem(inventory.ItemId);

        ValidateWarehouseReference(
            inventory.Warehouse);

        SupplyChain.AddInventory(inventory);

        return this;
    }

    #endregion

    #region Transport relationships

    /// <summary>
    /// Adds an item-transport-resource characteristic.
    ///
    /// The item and transport resource must already exist.
    /// </summary>
    public SupplyChainModelBuilder
        AddTransportCharacteristic(
            TransportCharacteristic characteristic)
    {
        ArgumentNullException.ThrowIfNull(
            characteristic);

        _index.GetRequiredItem(
            characteristic.ItemId);

        _index.GetRequiredTransportResource(
            characteristic.TransportResourceId);

        SupplyChain.AddTransportCharacteristic(
            characteristic);

        return this;
    }

    #endregion

    #region Demand and distribution relationships

    /// <summary>
    /// Adds a demand.
    ///
    /// The item and distribution center must already exist.
    /// </summary>
    public SupplyChainModelBuilder AddDemand(
        Demand demand)
    {
        ArgumentNullException.ThrowIfNull(demand);

        _index.GetRequiredItem(demand.ItemId);

        _index.GetRequiredDistributionCenter(
            demand.DistributionCenterId);

        SupplyChain.AddDemand(demand);

        return this;
    }

    /// <summary>
    /// Adds a distribution-center sourcing relationship.
    ///
    /// The item, distribution center, warehouse and corresponding
    /// inventory must already exist.
    /// </summary>
    public SupplyChainModelBuilder
        AddDistributionCenterSourcing(
            DistributionCenterSourcing sourcing)
    {
        ArgumentNullException.ThrowIfNull(sourcing);

        _index.GetRequiredItem(sourcing.ItemId);

        _index.GetRequiredDistributionCenter(
            sourcing.DistributionCenterId);

        ValidateWarehouseReference(
            sourcing.Warehouse);

        EnsureInventoryExists(
            sourcing.ItemId,
            sourcing.Warehouse);

        SupplyChain.AddDistributionCenterSourcing(
            sourcing);

        return this;
    }

    #endregion

    #region Supplier relationships

    /// <summary>
    /// Adds a supplier-delivery relationship.
    ///
    /// The supplier, item, warehouse and corresponding inventory
    /// must already exist.
    /// </summary>
    public SupplyChainModelBuilder
        AddSupplierDelivery(
            SupplierDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        _index.GetRequiredSupplier(
            delivery.SupplierId);

        _index.GetRequiredItem(
            delivery.ItemId);

        ValidateWarehouseReference(
            delivery.Warehouse);

        EnsureInventoryExists(
            delivery.ItemId,
            delivery.Warehouse);

        SupplyChain.AddSupplierDelivery(
            delivery);

        return this;
    }

    #endregion

    #region Validation and build

    /// <summary>
    /// Validates the current state of the model.
    /// </summary>
    public IReadOnlyList<
        SupplyChainValidator.ValidationIssue> Validate()
    {
        return _validator.Validate(SupplyChain);
    }

    /// <summary>
    /// Gets a value indicating whether the current model
    /// contains no validation error.
    /// </summary>
    public bool IsValid()
    {
        return _validator.IsValid(SupplyChain);
    }

    /// <summary>
    /// Synchronizes all time series and returns the model.
    ///
    /// The model is validated by default.
    /// </summary>
    /// <param name="validate">
    /// Indicates whether validation must be performed.
    /// </param>
    public SupplyChain Build(bool validate = true)
    {
        SupplyChain.SynchronizePlanningHorizon();

        RebuildIndex();

        if (validate)
        {
            _validator.ThrowIfInvalid(SupplyChain);
        }

        return SupplyChain;
    }

    /// <summary>
    /// Synchronizes, validates and returns the model.
    /// </summary>
    public SupplyChain BuildValidated()
    {
        return Build(validate: true);
    }

    #endregion

    #region Private helpers

    private void RebuildIndex()
    {
        _index.Rebuild();
    }

    private void ValidateTransportLane(
        TransportLane lane)
    {
        ArgumentNullException.ThrowIfNull(lane);

        ValidateWarehouseReference(lane.Origin);
        ValidateWarehouseReference(lane.Destination);

        if (SameWarehouse(
                lane.Origin,
                lane.Destination))
        {
            throw new InvalidOperationException(
                "A transport lane must connect two " +
                "different warehouses.");
        }

        if (lane.LeadTime < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lane),
                lane.LeadTime,
                "A transport lead time cannot be negative.");
        }
    }

    private void ValidateWarehouseReference(
        WarehouseReference? reference)
    {
        if (reference is null)
        {
            throw new InvalidOperationException(
                "A warehouse reference is required.");
        }

        _index.GetRequiredWarehouse(reference);
    }

    private void EnsureInventoryExists(
        int itemId,
        WarehouseReference warehouse)
    {
        bool inventoryExists =
            SupplyChain.Inventories.Any(
                inventory =>
                    inventory.ItemId == itemId &&
                    SameWarehouse(
                        inventory.Warehouse,
                        warehouse));

        if (!inventoryExists)
        {
            throw new InvalidOperationException(
                $"No inventory exists for item {itemId} " +
                $"and warehouse {FormatWarehouse(warehouse)}.");
        }
    }

    private static bool SameWarehouse(
        WarehouseReference? first,
        WarehouseReference? second)
    {
        if (first is null || second is null)
        {
            return first is null &&
                   second is null;
        }

        return first.Kind == second.Kind &&
               first.ReferenceId == second.ReferenceId;
    }

    private static string FormatWarehouse(
        WarehouseReference warehouse)
    {
        return
            $"{warehouse.Kind}:{warehouse.ReferenceId}";
    }

    #endregion
}