using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel.Finance;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core;

/// <summary>
/// Represents the complete supply-chain data model.
///
/// This class is the XML root of the model. It contains:
/// - the global planning horizon;
/// - the logical subsystem;
/// - the physical subsystem;
/// - the relationships between both subsystems;
/// - the associated decision-model parameters.
/// </summary>
[Serializable]
[XmlRoot("supplyChain")]
[XmlType(TypeName = "supplyChain")]
public sealed partial class SupplyChain :
    ModelObject,
    IPlanningHorizonAware
{
    private int _planningHorizon;
    private PeriodicOperatingExpenditureBudget?
        _periodicOperatingExpenditureBudget;
    private OptimizationObjectivePolicy?
        _objectivePolicy;

    /// <summary>
    /// Initializes an empty supply chain.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public SupplyChain()
    {
    }

    /// <summary>
    /// Initializes an empty supply chain with the specified
    /// planning horizon.
    /// </summary>
    /// <param name="planningHorizon">
    /// Non-negative number of planning periods.
    /// </param>
    public SupplyChain(int planningHorizon)
    {
        PlanningHorizon = planningHorizon;
    }

    /// <summary>
    /// Gets or sets the number of periods in the global
    /// planning horizon.
    ///
    /// Changing this value resizes every period-dependent
    /// parameter already contained in the supply chain.
    /// </summary>
    [XmlAttribute("planningHorizon")]
    public int PlanningHorizon
    {
        get => _planningHorizon;
        set => ResizeTimeSeries(value);
    }

    /// <summary>
    /// Gets or sets the period operating-expenditure budget.
    /// </summary>
    [XmlElement("periodicOperatingExpenditureBudget")]
    public PeriodicOperatingExpenditureBudget?
        PeriodicOperatingExpenditureBudget
    {
        get => _periodicOperatingExpenditureBudget;
        set
        {
            if (ReferenceEquals(
                    _periodicOperatingExpenditureBudget,
                    value))
            {
                return;
            }

            if (_periodicOperatingExpenditureBudget is not null)
            {
                _periodicOperatingExpenditureBudget.PropertyChanged -=
                    OnFinancialParameterPropertyChanged;
            }

            _periodicOperatingExpenditureBudget = value;

            if (_periodicOperatingExpenditureBudget is not null)
            {
                if (PlanningHorizon > 0)
                {
                    _periodicOperatingExpenditureBudget
                        .ResizeTimeSeries(PlanningHorizon);
                }

                _periodicOperatingExpenditureBudget.PropertyChanged +=
                    OnFinancialParameterPropertyChanged;
            }

            OnPropertyChanged(
                nameof(PeriodicOperatingExpenditureBudget));
            OnPropertyChanged(
                nameof(HasConsistentPlanningHorizon));
        }
    }

    /// <summary>
    /// Gets or sets the explicit business objective policy.
    /// Null preserves the historical single economic objective.
    /// </summary>
    [XmlElement("objectivePolicy")]
    public OptimizationObjectivePolicy? ObjectivePolicy
    {
        get => _objectivePolicy;
        set
        {
            if (ReferenceEquals(_objectivePolicy, value))
            {
                return;
            }

            value?.EnsureValid();
            _objectivePolicy = value;
            OnPropertyChanged(nameof(ObjectivePolicy));
        }
    }

    #region Logical subsystem

    /// <summary>
    /// Gets the items present in the supply chain.
    /// </summary>
    [XmlArray("items")]
    [XmlArrayItem("item")]
    public List<Item> Items { get; } = new();

    /// <summary>
    /// Gets the bill-of-material component requirements.
    /// </summary>
    [XmlArray("componentRequirements")]
    [XmlArrayItem("componentRequirement")]
    public List<ComponentRequirement> ComponentRequirements
    {
        get;
    } = new();

    #endregion

    #region Physical subsystem

    /// <summary>
    /// Gets the plants in the supply chain.
    ///
    /// Each plant contains its own warehouse and work centers.
    /// </summary>
    [XmlArray("plants")]
    [XmlArrayItem("plant")]
    public List<Plant> Plants { get; } = new();

    /// <summary>
    /// Gets the warehouses that are not attached to a plant.
    /// </summary>
    [XmlArray("standaloneWarehouses")]
    [XmlArrayItem("standaloneWarehouse")]
    public List<StandaloneWarehouse> StandaloneWarehouses
    {
        get;
    } = new();

    /// <summary>
    /// Gets the suppliers in the supply chain.
    /// </summary>
    [XmlArray("suppliers")]
    [XmlArrayItem("supplier")]
    public List<Supplier> Suppliers { get; } = new();

    /// <summary>
    /// Gets the distribution centers in the supply chain.
    /// </summary>
    [XmlArray("distributionCenters")]
    [XmlArrayItem("distributionCenter")]
    public List<DistributionCenter> DistributionCenters
    {
        get;
    } = new();

    /// <summary>
    /// Gets the transport resources in the supply chain.
    ///
    /// Each transport resource contains its own transport lanes.
    /// </summary>
    [XmlArray("transportResources")]
    [XmlArrayItem("transportResource")]
    public List<TransportResource> TransportResources
    {
        get;
    } = new();

    #endregion

    #region Logical-physical relationships

    /// <summary>
    /// Gets the production routings.
    /// </summary>
    [XmlArray("productionRoutings")]
    [XmlArrayItem("productionRouting")]
    public List<ProductionRouting> ProductionRoutings
    {
        get;
    } = new();

    /// <summary>
    /// Gets the item-work-center production characteristics.
    /// </summary>
    [XmlArray("productionCharacteristics")]
    [XmlArrayItem("productionCharacteristic")]
    public List<ProductionCharacteristic> ProductionCharacteristics
    {
        get;
    } = new();

    /// <summary>
    /// Gets the item-warehouse inventories.
    /// </summary>
    [XmlArray("inventories")]
    [XmlArrayItem("inventory")]
    public List<Inventory> Inventories { get; } = new();

    /// <summary>
    /// Gets the item-transport-resource characteristics.
    /// </summary>
    [XmlArray("transportCharacteristics")]
    [XmlArrayItem("transportCharacteristic")]
    public List<TransportCharacteristic> TransportCharacteristics
    {
        get;
    } = new();

    /// <summary>
    /// Gets the demands expressed by distribution centers.
    /// </summary>
    [XmlArray("demands")]
    [XmlArrayItem("demand")]
    public List<Demand> Demands { get; } = new();
    /// <summary>
    /// Gets optional additional-sales opportunities.
    /// </summary>
    [XmlArray("salesOptions")]
    [XmlArrayItem("salesOption")]
    public List<SalesOption> SalesOptions { get; } = new();

    /// <summary>
    /// Gets the warehouse sourcing options available
    /// to distribution centers.
    /// </summary>
    [XmlArray("distributionCenterSourcings")]
    [XmlArrayItem("distributionCenterSourcing")]
    public List<DistributionCenterSourcing>
        DistributionCenterSourcings
    {
        get;
    } = new();

    /// <summary>
    /// Gets the supplier delivery possibilities.
    /// </summary>
    [XmlArray("supplierDeliveries")]
    [XmlArrayItem("supplierDelivery")]
    public List<SupplierDelivery> SupplierDeliveries
    {
        get;
    } = new();

    #endregion

    #region Calculated properties

    /// <summary>
    /// Enumerates every warehouse in the supply chain,
    /// including plant warehouses and standalone warehouses.
    /// </summary>
    [XmlIgnore]
    public IEnumerable<Warehouse> Warehouses
    {
        get
        {
            foreach (Plant plant in Plants)
            {
                yield return plant.Warehouse;
            }

            foreach (StandaloneWarehouse warehouse
                     in StandaloneWarehouses)
            {
                yield return warehouse;
            }
        }
    }

    /// <summary>
    /// Enumerates every work center in the supply chain.
    /// </summary>
    [XmlIgnore]
    public IEnumerable<WorkCenter> WorkCenters =>
        Plants.SelectMany(plant => plant.WorkCenters);

    /// <summary>
    /// Gets a value indicating whether all active time series
    /// use the global planning horizon.
    ///
    /// A local horizon of zero means that the corresponding
    /// object does not currently contain any active time series.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        GetPlanningHorizonAwareObjects().All(
            modelObject =>
                modelObject.PlanningHorizon == 0 ||
                modelObject.PlanningHorizon ==
                    PlanningHorizon);

    #endregion

    #region Add methods

    /// <summary>
    /// Adds an item and rejects duplicate item identifiers.
    /// </summary>
    public void AddItem(Item item)
    {
        AddUnique(
            Items,
            item,
            existing => existing.Id == item.Id,
            nameof(Items),
            $"An item with identifier {item.Id} already exists.");
    }

    /// <summary>
    /// Adds a bill-of-material component requirement.
    /// </summary>
    public void AddComponentRequirement(
        ComponentRequirement requirement)
    {
        AddUnique(
            ComponentRequirements,
            requirement,
            existing =>
                existing.ParentItemId ==
                    requirement.ParentItemId &&
                existing.ComponentItemId ==
                    requirement.ComponentItemId,
            nameof(ComponentRequirements),
            "A component requirement already exists for " +
            "this parent-item/component-item pair.");
    }

    /// <summary>
    /// Adds a plant and synchronizes its time-dependent data
    /// with the global planning horizon.
    /// </summary>
    public void AddPlant(Plant plant)
    {
        AddUnique(
            Plants,
            plant,
            existing => existing.Id == plant.Id,
            nameof(Plants),
            $"A plant with identifier {plant.Id} already exists.");
    }

    /// <summary>
    /// Adds a standalone warehouse.
    /// </summary>
    public void AddStandaloneWarehouse(
        StandaloneWarehouse warehouse)
    {
        AddUnique(
            StandaloneWarehouses,
            warehouse,
            existing => existing.Id == warehouse.Id,
            nameof(StandaloneWarehouses),
            "A standalone warehouse with identifier " +
            $"{warehouse.Id} already exists.");
    }

    /// <summary>
    /// Adds a supplier.
    /// </summary>
    public void AddSupplier(Supplier supplier)
    {
        AddUnique(
            Suppliers,
            supplier,
            existing => existing.Id == supplier.Id,
            nameof(Suppliers),
            $"A supplier with identifier {supplier.Id} already exists.");
    }

    /// <summary>
    /// Adds a distribution center.
    /// </summary>
    public void AddDistributionCenter(
        DistributionCenter distributionCenter)
    {
        AddUnique(
            DistributionCenters,
            distributionCenter,
            existing =>
                existing.Id == distributionCenter.Id,
            nameof(DistributionCenters),
            "A distribution center with identifier " +
            $"{distributionCenter.Id} already exists.");
    }

    /// <summary>
    /// Adds a transport resource.
    /// </summary>
    public void AddTransportResource(
        TransportResource transportResource)
    {
        AddUnique(
            TransportResources,
            transportResource,
            existing =>
                existing.Id == transportResource.Id,
            nameof(TransportResources),
            "A transport resource with identifier " +
            $"{transportResource.Id} already exists.");
    }

    /// <summary>
    /// Adds a production routing.
    /// </summary>
    public void AddProductionRouting(
        ProductionRouting routing)
    {
        AddUnique(
            ProductionRoutings,
            routing,
            existing => existing.Id == routing.Id,
            nameof(ProductionRoutings),
            "A production routing with identifier " +
            $"{routing.Id} already exists.");
    }

    /// <summary>
    /// Adds an item-work-center production characteristic.
    /// </summary>
    public void AddProductionCharacteristic(
        ProductionCharacteristic characteristic)
    {
        AddUnique(
            ProductionCharacteristics,
            characteristic,
            existing =>
                existing.ItemId == characteristic.ItemId &&
                SameWorkCenter(
                    existing.WorkCenter,
                    characteristic.WorkCenter),
            nameof(ProductionCharacteristics),
            "A production characteristic already exists " +
            "for this item and work center.");
    }

    /// <summary>
    /// Adds an inventory relationship.
    /// </summary>
    public void AddInventory(Inventory inventory)
    {
        AddUnique(
            Inventories,
            inventory,
            existing =>
                existing.ItemId == inventory.ItemId &&
                SameWarehouse(
                    existing.Warehouse,
                    inventory.Warehouse),
            nameof(Inventories),
            "An inventory already exists for this item " +
            "and warehouse.");
    }

    /// <summary>
    /// Adds an item-transport-resource characteristic.
    /// </summary>
    public void AddTransportCharacteristic(
        TransportCharacteristic characteristic)
    {
        AddUnique(
            TransportCharacteristics,
            characteristic,
            existing =>
                existing.ItemId == characteristic.ItemId &&
                existing.TransportResourceId ==
                    characteristic.TransportResourceId,
            nameof(TransportCharacteristics),
            "A transport characteristic already exists " +
            "for this item and transport resource.");
    }

    /// <summary>
    /// Adds a demand relationship.
    /// </summary>
    public void AddDemand(Demand demand)
    {
        AddUnique(
            Demands,
            demand,
            existing =>
                existing.ItemId == demand.ItemId &&
                existing.DistributionCenterId ==
                    demand.DistributionCenterId,
            nameof(Demands),
            "A demand already exists for this item " +
            "and distribution center.");
    }
    /// <summary>
    /// Adds an optional additional-sales relationship.
    /// </summary>
    public void AddSalesOption(SalesOption salesOption)
    {
        AddUnique(
            SalesOptions,
            salesOption,
            existing =>
                existing.ItemId == salesOption.ItemId &&
                existing.DistributionCenterId ==
                    salesOption.DistributionCenterId,
            nameof(SalesOptions),
            "A sales option already exists for this item " +
            "and distribution center.");
    }

    /// <summary>
    /// Adds a distribution-center sourcing relationship.
    /// </summary>
    public void AddDistributionCenterSourcing(
        DistributionCenterSourcing sourcing)
    {
        AddUnique(
            DistributionCenterSourcings,
            sourcing,
            existing =>
                existing.DistributionCenterId ==
                    sourcing.DistributionCenterId &&
                existing.ItemId == sourcing.ItemId &&
                SameWarehouse(
                    existing.Warehouse,
                    sourcing.Warehouse),
            nameof(DistributionCenterSourcings),
            "A sourcing relationship already exists for " +
            "this distribution center, item and warehouse.");
    }

    /// <summary>
    /// Adds a supplier delivery relationship.
    /// </summary>
    public void AddSupplierDelivery(
        SupplierDelivery delivery)
    {
        AddUnique(
            SupplierDeliveries,
            delivery,
            existing =>
                existing.SupplierId == delivery.SupplierId &&
                existing.ItemId == delivery.ItemId &&
                SameWarehouse(
                    existing.Warehouse,
                    delivery.Warehouse),
            nameof(SupplierDeliveries),
            "A supplier delivery already exists for this " +
            "supplier, item and warehouse.");
    }

    #endregion

    #region Planning-horizon management

    /// <summary>
    /// Changes the global planning horizon and resizes every
    /// active time series contained in the supply chain.
    ///
    /// Existing values are preserved whenever possible.
    /// </summary>
    /// <param name="periodCount">
    /// New non-negative number of planning periods.
    /// </param>
    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
        }

        // Cascade the resize to every period-dependent object.
        foreach (IPlanningHorizonAware modelObject
                 in GetPlanningHorizonAwareObjects())
        {
            modelObject.ResizeTimeSeries(periodCount);
        }

        bool horizonChanged =
            _planningHorizon != periodCount;

        // Update the internal field.
        _planningHorizon = periodCount;

        if (horizonChanged)
        {
            OnPropertyChanged(nameof(PlanningHorizon));
        }

        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Forces every active time series to use the current
    /// global planning horizon.
    ///
    /// This method is useful after XML deserialization or after
    /// direct modifications of the exposed collections.
    /// </summary>
    public void SynchronizePlanningHorizon()
    {
        foreach (IPlanningHorizonAware modelObject
                 in GetPlanningHorizonAwareObjects())
        {
            modelObject.ResizeTimeSeries(
                PlanningHorizon);
        }

        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Enumerates every object containing period-dependent data.
    /// </summary>
    private IEnumerable<IPlanningHorizonAware>
        GetPlanningHorizonAwareObjects()
    {
        if (PeriodicOperatingExpenditureBudget is not null)
        {
            yield return PeriodicOperatingExpenditureBudget;
        }

        foreach (Plant plant in Plants)
        {
            yield return plant.Warehouse;

            foreach (WorkCenter workCenter
                     in plant.WorkCenters)
            {
                yield return workCenter;
            }
        }

        foreach (StandaloneWarehouse warehouse
                 in StandaloneWarehouses)
        {
            yield return warehouse;
        }

        foreach (TransportResource resource
                 in TransportResources)
        {
            yield return resource;
        }

        foreach (ProductionSetupFamily setupFamily
                 in ProductionSetupFamilies)
        {
            yield return setupFamily;
        }

        foreach (ProductionRouting routing
                 in ProductionRoutings)
        {
            yield return routing;
        }

        foreach (ProductionCharacteristic characteristic
                 in ProductionCharacteristics)
        {
            yield return characteristic;
        }

        foreach (Inventory inventory in Inventories)
        {
            yield return inventory;
        }

        foreach (TransportCharacteristic characteristic
                 in TransportCharacteristics)
        {
            yield return characteristic;
        }

        foreach (Demand demand in Demands)
        {
            yield return demand;
        }
        foreach (SalesOption salesOption in SalesOptions)
        {
            yield return salesOption;
        }
        if (CashFlowPolicy is not null)
        {
            yield return CashFlowPolicy;
        }

        foreach (DistributionCenterSourcing sourcing
                 in DistributionCenterSourcings)
        {
            yield return sourcing;
        }

        foreach (SupplierDelivery delivery
                 in SupplierDeliveries)
        {
            yield return delivery;
        }
    }

    #endregion

    #region Private helpers

    private void OnFinancialParameterPropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(
            nameof(PeriodicOperatingExpenditureBudget));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }



    /// <summary>
    /// Adds an object to a collection after checking uniqueness
    /// and synchronizing its period-dependent data.
    /// </summary>
    private void AddUnique<T>(
        List<T> collection,
        T value,
        Func<T, bool> duplicatePredicate,
        string propertyName,
        string duplicateMessage)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        // Reject duplicates based on the supplied predicate.
        if (collection.Any(duplicatePredicate))
        {
            throw new InvalidOperationException(
                duplicateMessage);
        }

        // Synchronize period-dependent data with the global horizon.
        SynchronizeNewObject(value);

        collection.Add(value);

        OnPropertyChanged(propertyName);
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Synchronizes a newly added object with the current
    /// planning horizon.
    /// </summary>
    private void SynchronizeNewObject(object value)
    {
        /*
         * A zero horizon represents an uninitialized model.
         * Existing parameter values are therefore preserved
         * until a positive horizon is explicitly assigned.
         */
        if (PlanningHorizon == 0)
        {
            return;
        }

        // Special handling for Plant: resize its warehouse and all work centers.
        if (value is Plant plant)
        {
            plant.Warehouse.ResizeTimeSeries(
                PlanningHorizon);

            foreach (WorkCenter workCenter
                     in plant.WorkCenters)
            {
                workCenter.ResizeTimeSeries(
                    PlanningHorizon);
            }

            return;
        }

        // General case: resize any IPlanningHorizonAware object.
        if (value is IPlanningHorizonAware
            planningHorizonAware)
        {
            planningHorizonAware.ResizeTimeSeries(
                PlanningHorizon);
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

    #endregion
}