using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Describes the factual structural and modeling features
/// detected in a lot-sizing supply-chain instance.
/// </summary>
/// <remarks>
/// This class does not assign a known problem-family code.
///
/// Its properties form the input of the automatic
/// lot-sizing problem classifier.
/// </remarks>
[Serializable]
[XmlType(TypeName = "lotSizingProblemFeatures")]
public sealed class LotSizingProblemFeatures : ModelObject
{
    private int _itemCount;
    private int _planningHorizon;
    private int _plantCount;
    private int _workCenterCount;
    private int _warehouseCount;
    private int _supplierCount;
    private int _distributionCenterCount;
    private int _transportResourceCount;

    private int _productStructureRelationshipCount;
    private int _maximumProductStructureDepth;

    private ProductStructureType _productStructureType =
        ProductStructureType.Unknown;

    private bool _hasDemand;
    private bool _hasDeterministicDemand;
    private bool _hasTimeVaryingDemand;

    private bool _hasInitialInventory;
    private bool _hasSafetyStockRequirements;
    private bool _hasBacklogging;
    private bool _hasLostSales;

    private bool _hasProduction;
    private bool _hasProductionCapacityConstraints;
    private bool _hasSharedProductionCapacity;
    private bool _hasTimeVaryingProductionCapacity;

    private bool _hasSetupCosts;
    private bool _hasSetupTimes;
    private bool _hasStartUpCosts;
    private bool _hasStartUpTimes;

    private bool _hasProductionLeadTimes;
    private bool _hasMinimumLotSizes;
    private bool _hasMaximumLotSizes;
    private bool _hasLotSizeMultiples;

    private bool _hasAdditionalProductionCapacity;
    private bool _hasAdditionalWarehouseCapacity;
    private bool _hasAdditionalTransportCapacity;

    private bool _hasPurchasing;
    private bool _hasSupplierCapacityConstraints;
    private bool _hasSupplierLeadTimes;

    private bool _hasTransportation;
    private bool _hasTransportCapacityConstraints;
    private bool _hasTransportLeadTimes;

    private bool _hasDistribution;
    private bool _hasWarehouseCapacityConstraints;

    private bool _isMultiSite;
    private bool _hasFinancialConstraints;
    private bool _hasMultipleObjectives;
    private int _objectiveCriterionCount = 1;
    private OptimizationObjectiveKind _primaryObjectiveKind =
        OptimizationObjectiveKind.Economic;
    private ObjectiveAggregationMode _objectiveAggregationMode =
        ObjectiveAggregationMode.Single;
    private bool _hasIntegratedScheduling;
    private SchedulingBucketMode _schedulingBucketMode;
    private bool _hasInitialSetupState;
    private bool _hasSetupCarryOver;
    private bool _hasSequenceDependentChangeoverTimes;
    private bool _hasSequenceDependentChangeoverCosts;
    private bool _hasMaximumSetupCountConstraints;
    private SmallBucketProductionMode _smallBucketProductionMode;
    private int _schedulingResourceCount;
    private bool _hasMaximumProducedItemCountConstraint;
    private int _maximumProducedItemCountPerBucket;
    private int _maximumSetupTransitionsPerBucket;

    /// <summary>
    /// Initializes an empty lot-sizing problem-feature profile.
    /// </summary>
    public LotSizingProblemFeatures()
    {
    }

    /// <summary>
    /// Gets or sets the number of items in the instance.
    /// </summary>
    [XmlAttribute("itemCount")]
    public int ItemCount
    {
        get => _itemCount;
        set => SetNonNegativeCount(
            ref _itemCount,
            value,
            nameof(ItemCount));
    }

    /// <summary>
    /// Gets or sets the number of planning periods.
    /// </summary>
    [XmlAttribute("planningHorizon")]
    public int PlanningHorizon
    {
        get => _planningHorizon;
        set => SetNonNegativeCount(
            ref _planningHorizon,
            value,
            nameof(PlanningHorizon));
    }

    /// <summary>
    /// Gets or sets the number of production plants.
    /// </summary>
    [XmlAttribute("plantCount")]
    public int PlantCount
    {
        get => _plantCount;
        set => SetNonNegativeCount(
            ref _plantCount,
            value,
            nameof(PlantCount));
    }

    /// <summary>
    /// Gets or sets the total number of work centers.
    /// </summary>
    [XmlAttribute("workCenterCount")]
    public int WorkCenterCount
    {
        get => _workCenterCount;
        set => SetNonNegativeCount(
            ref _workCenterCount,
            value,
            nameof(WorkCenterCount));
    }

    /// <summary>
    /// Gets or sets the total number of warehouses.
    /// </summary>
    /// <remarks>
    /// This count may include plant warehouses and
    /// standalone warehouses.
    /// </remarks>
    [XmlAttribute("warehouseCount")]
    public int WarehouseCount
    {
        get => _warehouseCount;
        set => SetNonNegativeCount(
            ref _warehouseCount,
            value,
            nameof(WarehouseCount));
    }

    /// <summary>
    /// Gets or sets the number of suppliers.
    /// </summary>
    [XmlAttribute("supplierCount")]
    public int SupplierCount
    {
        get => _supplierCount;
        set => SetNonNegativeCount(
            ref _supplierCount,
            value,
            nameof(SupplierCount));
    }

    /// <summary>
    /// Gets or sets the number of distribution centers.
    /// </summary>
    [XmlAttribute("distributionCenterCount")]
    public int DistributionCenterCount
    {
        get => _distributionCenterCount;
        set => SetNonNegativeCount(
            ref _distributionCenterCount,
            value,
            nameof(DistributionCenterCount));
    }

    /// <summary>
    /// Gets or sets the number of transport resources.
    /// </summary>
    [XmlAttribute("transportResourceCount")]
    public int TransportResourceCount
    {
        get => _transportResourceCount;
        set => SetNonNegativeCount(
            ref _transportResourceCount,
            value,
            nameof(TransportResourceCount));
    }

    /// <summary>
    /// Gets or sets the number of distinct bill-of-materials
    /// relationships.
    /// </summary>
    [XmlAttribute("productStructureRelationshipCount")]
    public int ProductStructureRelationshipCount
    {
        get => _productStructureRelationshipCount;
        set => SetNonNegativeCount(
            ref _productStructureRelationshipCount,
            value,
            nameof(ProductStructureRelationshipCount));
    }

    /// <summary>
    /// Gets or sets the maximum number of bill-of-materials
    /// relationships on a directed product-structure path.
    /// </summary>
    [XmlAttribute("maximumProductStructureDepth")]
    public int MaximumProductStructureDepth
    {
        get => _maximumProductStructureDepth;
        set => SetNonNegativeCount(
            ref _maximumProductStructureDepth,
            value,
            nameof(MaximumProductStructureDepth));
    }

    /// <summary>
    /// Gets or sets the detected product-structure category.
    /// </summary>
    [XmlAttribute("productStructureType")]
    public ProductStructureType ProductStructureType
    {
        get => _productStructureType;
        set
        {
            if (SetProperty(
                    ref _productStructureType,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the instance
    /// contains demand data.
    /// </summary>
    [XmlAttribute("hasDemand")]
    public bool HasDemand
    {
        get => _hasDemand;
        set
        {
            if (SetProperty(
                    ref _hasDemand,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether demand
    /// is deterministic.
    /// </summary>
    [XmlAttribute("hasDeterministicDemand")]
    public bool HasDeterministicDemand
    {
        get => _hasDeterministicDemand;
        set => SetProperty(
            ref _hasDeterministicDemand,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether demand
    /// varies between planning periods.
    /// </summary>
    [XmlAttribute("hasTimeVaryingDemand")]
    public bool HasTimeVaryingDemand
    {
        get => _hasTimeVaryingDemand;
        set
        {
            if (SetProperty(
                    ref _hasTimeVaryingDemand,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether at least one
    /// positive initial-inventory value exists.
    /// </summary>
    [XmlAttribute("hasInitialInventory")]
    public bool HasInitialInventory
    {
        get => _hasInitialInventory;
        set => SetProperty(
            ref _hasInitialInventory,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the instance
    /// contains safety-stock requirements.
    /// </summary>
    [XmlAttribute("hasSafetyStockRequirements")]
    public bool HasSafetyStockRequirements
    {
        get => _hasSafetyStockRequirements;
        set => SetProperty(
            ref _hasSafetyStockRequirements,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether unmet demand
    /// may be carried to later periods.
    /// </summary>
    [XmlAttribute("hasBacklogging")]
    public bool HasBacklogging
    {
        get => _hasBacklogging;
        set => SetProperty(
            ref _hasBacklogging,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether demand may
    /// be permanently lost.
    /// </summary>
    [XmlAttribute("hasLostSales")]
    public bool HasLostSales
    {
        get => _hasLostSales;
        set => SetProperty(
            ref _hasLostSales,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the instance
    /// contains production decisions.
    /// </summary>
    [XmlAttribute("hasProduction")]
    public bool HasProduction
    {
        get => _hasProduction;
        set => SetProperty(
            ref _hasProduction,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether production
    /// is subject to capacity constraints.
    /// </summary>
    [XmlAttribute("hasProductionCapacityConstraints")]
    public bool HasProductionCapacityConstraints
    {
        get => _hasProductionCapacityConstraints;
        set
        {
            if (SetProperty(
                    ref _hasProductionCapacityConstraints,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether several
    /// production activities share at least one capacity.
    /// </summary>
    [XmlAttribute("hasSharedProductionCapacity")]
    public bool HasSharedProductionCapacity
    {
        get => _hasSharedProductionCapacity;
        set => SetProperty(
            ref _hasSharedProductionCapacity,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether at least one
    /// production capacity varies between periods.
    /// </summary>
    [XmlAttribute("hasTimeVaryingProductionCapacity")]
    public bool HasTimeVaryingProductionCapacity
    {
        get => _hasTimeVaryingProductionCapacity;
        set
        {
            if (SetProperty(
                    ref _hasTimeVaryingProductionCapacity,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether at least one
    /// setup or fixed production cost exists.
    /// </summary>
    [XmlAttribute("hasSetupCosts")]
    public bool HasSetupCosts
    {
        get => _hasSetupCosts;
        set => SetProperty(
            ref _hasSetupCosts,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether setups
    /// consume production-resource capacity.
    /// </summary>
    [XmlAttribute("hasSetupTimes")]
    public bool HasSetupTimes
    {
        get => _hasSetupTimes;
        set => SetProperty(
            ref _hasSetupTimes,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the model
    /// contains start-up costs distinct from setup costs.
    /// </summary>
    [XmlAttribute("hasStartUpCosts")]
    public bool HasStartUpCosts
    {
        get => _hasStartUpCosts;
        set => SetProperty(
            ref _hasStartUpCosts,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the model
    /// contains start-up times distinct from setup times.
    /// </summary>
    [XmlAttribute("hasStartUpTimes")]
    public bool HasStartUpTimes
    {
        get => _hasStartUpTimes;
        set => SetProperty(
            ref _hasStartUpTimes,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether production
    /// lead times are represented.
    /// </summary>
    [XmlAttribute("hasProductionLeadTimes")]
    public bool HasProductionLeadTimes
    {
        get => _hasProductionLeadTimes;
        set => SetProperty(
            ref _hasProductionLeadTimes,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether at least one
    /// positive minimum lot size is imposed.
    /// </summary>
    [XmlAttribute("hasMinimumLotSizes")]
    public bool HasMinimumLotSizes
    {
        get => _hasMinimumLotSizes;
        set
        {
            if (SetProperty(
                    ref _hasMinimumLotSizes,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether at least one
    /// explicit maximum lot size is imposed.
    /// </summary>
    [XmlAttribute("hasMaximumLotSizes")]
    public bool HasMaximumLotSizes
    {
        get => _hasMaximumLotSizes;
        set
        {
            if (SetProperty(
                    ref _hasMaximumLotSizes,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether at least one
    /// lot size must be an integer multiple of a base size.
    /// </summary>
    [XmlAttribute("hasLotSizeMultiples")]
    public bool HasLotSizeMultiples
    {
        get => _hasLotSizeMultiples;
        set
        {
            if (SetProperty(
                    ref _hasLotSizeMultiples,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether additional
    /// production capacity can be acquired or activated.
    /// </summary>
    [XmlAttribute("hasAdditionalProductionCapacity")]
    public bool HasAdditionalProductionCapacity
    {
        get => _hasAdditionalProductionCapacity;
        set => SetProperty(
            ref _hasAdditionalProductionCapacity,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether additional
    /// warehouse capacity can be acquired or activated.
    /// </summary>
    [XmlAttribute("hasAdditionalWarehouseCapacity")]
    public bool HasAdditionalWarehouseCapacity
    {
        get => _hasAdditionalWarehouseCapacity;
        set => SetProperty(
            ref _hasAdditionalWarehouseCapacity,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether additional
    /// transport capacity can be acquired or activated.
    /// </summary>
    [XmlAttribute("hasAdditionalTransportCapacity")]
    public bool HasAdditionalTransportCapacity
    {
        get => _hasAdditionalTransportCapacity;
        set => SetProperty(
            ref _hasAdditionalTransportCapacity,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether external
    /// purchasing decisions are represented.
    /// </summary>
    [XmlAttribute("hasPurchasing")]
    public bool HasPurchasing
    {
        get => _hasPurchasing;
        set
        {
            if (SetProperty(
                    ref _hasPurchasing,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether suppliers
    /// are subject to capacity constraints.
    /// </summary>
    [XmlAttribute("hasSupplierCapacityConstraints")]
    public bool HasSupplierCapacityConstraints
    {
        get => _hasSupplierCapacityConstraints;
        set => SetProperty(
            ref _hasSupplierCapacityConstraints,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether supplier
    /// delivery lead times are represented.
    /// </summary>
    [XmlAttribute("hasSupplierLeadTimes")]
    public bool HasSupplierLeadTimes
    {
        get => _hasSupplierLeadTimes;
        set => SetProperty(
            ref _hasSupplierLeadTimes,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether transportation
    /// decisions are represented.
    /// </summary>
    [XmlAttribute("hasTransportation")]
    public bool HasTransportation
    {
        get => _hasTransportation;
        set
        {
            if (SetProperty(
                    ref _hasTransportation,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether transportation
    /// is subject to capacity constraints.
    /// </summary>
    [XmlAttribute("hasTransportCapacityConstraints")]
    public bool HasTransportCapacityConstraints
    {
        get => _hasTransportCapacityConstraints;
        set => SetProperty(
            ref _hasTransportCapacityConstraints,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether transport
    /// lead times are represented.
    /// </summary>
    [XmlAttribute("hasTransportLeadTimes")]
    public bool HasTransportLeadTimes
    {
        get => _hasTransportLeadTimes;
        set => SetProperty(
            ref _hasTransportLeadTimes,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether distribution
    /// decisions are represented.
    /// </summary>
    [XmlAttribute("hasDistribution")]
    public bool HasDistribution
    {
        get => _hasDistribution;
        set
        {
            if (SetProperty(
                    ref _hasDistribution,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether storage
    /// is subject to warehouse-capacity constraints.
    /// </summary>
    [XmlAttribute("hasWarehouseCapacityConstraints")]
    public bool HasWarehouseCapacityConstraints
    {
        get => _hasWarehouseCapacityConstraints;
        set => SetProperty(
            ref _hasWarehouseCapacityConstraints,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the instance
    /// contains several physical decision sites.
    /// </summary>
    [XmlAttribute("isMultiSite")]
    public bool IsMultiSite
    {
        get => _isMultiSite;
        set
        {
            if (SetProperty(
                    ref _isMultiSite,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether explicit
    /// financial constraints are represented.
    /// </summary>
    [XmlAttribute("hasFinancialConstraints")]
    public bool HasFinancialConstraints
    {
        get => _hasFinancialConstraints;
        set => SetProperty(
            ref _hasFinancialConstraints,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether several
    /// objective criteria are represented.
    /// </summary>
    [XmlAttribute("hasMultipleObjectives")]
    public bool HasMultipleObjectives
    {
        get => _hasMultipleObjectives;
        set => SetProperty(
            ref _hasMultipleObjectives,
            value);
    }

    /// <summary>
    /// Gets a value indicating whether the instance contains
    /// exactly one item.
    /// </summary>
    [XmlIgnore]
    public bool IsSingleItem =>
        ItemCount == 1;

    /// <summary>
    /// Gets a value indicating whether the instance contains
    /// several items.
    /// </summary>
    [XmlIgnore]
    public bool IsMultiItem =>
        ItemCount > 1;

    /// <summary>
    /// Gets a value indicating whether the planning horizon
    /// contains exactly one period.
    /// </summary>
    [XmlIgnore]
    public bool IsSinglePeriod =>
        PlanningHorizon == 1;

    /// <summary>
    /// Gets a value indicating whether the planning horizon
    /// contains several periods.
    /// </summary>
    [XmlIgnore]
    public bool IsMultiPeriod =>
        PlanningHorizon > 1;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// bill-of-materials relationship exists.
    /// </summary>
    [XmlIgnore]
    public bool HasProductStructure =>
        ProductStructureRelationshipCount > 0;

    /// <summary>
    /// Gets a value indicating whether the items are
    /// independent from a bill-of-materials perspective.
    /// </summary>
    /// <remarks>
    /// In classical lot-sizing terminology, this corresponds
    /// to a single-level problem.
    /// </remarks>
    [XmlIgnore]
    public bool IsSingleLevel =>
        !HasProductStructure;

    /// <summary>
    /// Gets a value indicating whether the instance contains
    /// component-to-parent relationships.
    /// </summary>
    /// <remarks>
    /// Any bill-of-materials dependency makes the lot-sizing
    /// problem multi-level.
    /// </remarks>
    [XmlIgnore]
    public bool IsMultiLevel =>
        HasProductStructure;

    /// <summary>
    /// Gets a value indicating whether production capacity
    /// is constant over time.
    /// </summary>
    [XmlIgnore]
    public bool HasConstantProductionCapacity =>
        HasProductionCapacityConstraints &&
        !HasTimeVaryingProductionCapacity;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// capacity family constrains the problem.
    /// </summary>
    [XmlIgnore]
    public bool IsCapacitated =>
        HasProductionCapacityConstraints ||
        HasSupplierCapacityConstraints ||
        HasTransportCapacityConstraints ||
        HasWarehouseCapacityConstraints;

    /// <summary>
    /// Gets a value indicating whether at least one special
    /// lot-size restriction is present.
    /// </summary>
    [XmlIgnore]
    public bool HasLotSizeRestrictions =>
        HasMinimumLotSizes ||
        HasMaximumLotSizes ||
        HasLotSizeMultiples;

    /// <summary>
    /// Gets a value indicating whether at least one form
    /// of additional capacity is available.
    /// </summary>
    [XmlIgnore]
    public bool HasAdditionalCapacity =>
        HasAdditionalProductionCapacity ||
        HasAdditionalWarehouseCapacity ||
        HasAdditionalTransportCapacity;

    /// <summary>
    /// Gets a value indicating whether the problem contains
    /// purchasing, transport or distribution decisions.
    /// </summary>
    [XmlIgnore]
    public bool HasSupplyChainNetworkDecisions =>
        HasPurchasing ||
        HasTransportation ||
        HasDistribution;

    /// <summary>
    /// Gets a value indicating whether demand is constant
    /// across periods.
    /// </summary>
    [XmlIgnore]
    public bool HasStationaryDemand =>
        HasDemand &&
        !HasTimeVaryingDemand;

    /// <summary>
    /// Gets a value indicating whether the extracted feature
    /// profile contains the minimum structural information
    /// needed by the classifier.
    /// </summary>
    [XmlIgnore]
    public bool IsStructurallyUsable =>
        ItemCount > 0 &&
        PlanningHorizon > 0 &&
        ProductStructureType !=
            ProductStructureType.Unknown;

    /// <inheritdoc/>
    public override string ToString()
    {
        string levelDescription =
            IsMultiLevel
                ? "multi-level"
                : "single-level";

        string capacityDescription =
            IsCapacitated
                ? "capacitated"
                : "uncapacitated";

        return
            $"{ItemCount} item(s), " +
            $"{PlanningHorizon} period(s), " +
            $"{levelDescription}, " +
            $"{capacityDescription}, " +
            $"{ProductStructureType}";
    }

    private void SetNonNegativeCount(
        ref int storage,
        int value,
        string propertyName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                propertyName,
                value,
                "The value cannot be negative.");
        }

        if (SetProperty(
                ref storage,
                value,
                propertyName))
        {
            NotifyDerivedProperties();
        }
    }

    /// <summary>Gets or sets the number of enabled objective criteria.</summary>
    [XmlAttribute("objectiveCriterionCount")]
    public int ObjectiveCriterionCount
    {
        get => _objectiveCriterionCount;
        set => SetNonNegativeCount(
            ref _objectiveCriterionCount,
            value,
            nameof(ObjectiveCriterionCount));
    }

    [XmlAttribute("primaryObjectiveKind")]
    public OptimizationObjectiveKind PrimaryObjectiveKind
    {
        get => _primaryObjectiveKind;
        set => SetProperty(
            ref _primaryObjectiveKind,
            value);
    }

    [XmlAttribute("objectiveAggregationMode")]
    public ObjectiveAggregationMode ObjectiveAggregationMode
    {
        get => _objectiveAggregationMode;
        set => SetProperty(
            ref _objectiveAggregationMode,
            value);
    }

    [XmlAttribute("hasIntegratedScheduling")]
    public bool HasIntegratedScheduling
    {
        get => _hasIntegratedScheduling;
        set => SetProperty(ref _hasIntegratedScheduling, value);
    }

    [XmlAttribute("schedulingBucketMode")]
    public SchedulingBucketMode SchedulingBucketMode
    {
        get => _schedulingBucketMode;
        set => SetProperty(ref _schedulingBucketMode, value);
    }

    [XmlAttribute("hasInitialSetupState")]
    public bool HasInitialSetupState
    {
        get => _hasInitialSetupState;
        set => SetProperty(ref _hasInitialSetupState, value);
    }

    [XmlAttribute("hasSetupCarryOver")]
    public bool HasSetupCarryOver
    {
        get => _hasSetupCarryOver;
        set => SetProperty(ref _hasSetupCarryOver, value);
    }

    [XmlAttribute("hasSequenceDependentChangeoverTimes")]
    public bool HasSequenceDependentChangeoverTimes
    {
        get => _hasSequenceDependentChangeoverTimes;
        set => SetProperty(
            ref _hasSequenceDependentChangeoverTimes,
            value);
    }

    [XmlAttribute("hasSequenceDependentChangeoverCosts")]
    public bool HasSequenceDependentChangeoverCosts
    {
        get => _hasSequenceDependentChangeoverCosts;
        set => SetProperty(
            ref _hasSequenceDependentChangeoverCosts,
            value);
    }

    [XmlAttribute("hasMaximumSetupCountConstraints")]
    public bool HasMaximumSetupCountConstraints
    {
        get => _hasMaximumSetupCountConstraints;
        set => SetProperty(
            ref _hasMaximumSetupCountConstraints,
            value);
    }

    [XmlAttribute("smallBucketProductionMode")]
    public SmallBucketProductionMode SmallBucketProductionMode
    {
        get => _smallBucketProductionMode;
        set => SetProperty(ref _smallBucketProductionMode, value);
    }

    [XmlAttribute("schedulingResourceCount")]
    public int SchedulingResourceCount
    {
        get => _schedulingResourceCount;
        set => SetNonNegativeCount(
            ref _schedulingResourceCount,
            value,
            nameof(SchedulingResourceCount));
    }

    [XmlAttribute("hasMaximumProducedItemCountConstraint")]
    public bool HasMaximumProducedItemCountConstraint
    {
        get => _hasMaximumProducedItemCountConstraint;
        set => SetProperty(
            ref _hasMaximumProducedItemCountConstraint,
            value);
    }

    [XmlAttribute("maximumProducedItemCountPerBucket")]
    public int MaximumProducedItemCountPerBucket
    {
        get => _maximumProducedItemCountPerBucket;
        set => SetNonNegativeCount(
            ref _maximumProducedItemCountPerBucket,
            value,
            nameof(MaximumProducedItemCountPerBucket));
    }

    [XmlAttribute("maximumSetupTransitionsPerBucket")]
    public int MaximumSetupTransitionsPerBucket
    {
        get => _maximumSetupTransitionsPerBucket;
        set => SetNonNegativeCount(
            ref _maximumSetupTransitionsPerBucket,
            value,
            nameof(MaximumSetupTransitionsPerBucket));
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(IsSingleItem));

        OnPropertyChanged(
            nameof(IsMultiItem));

        OnPropertyChanged(
            nameof(IsSinglePeriod));

        OnPropertyChanged(
            nameof(IsMultiPeriod));

        OnPropertyChanged(
            nameof(HasProductStructure));

        OnPropertyChanged(
            nameof(IsSingleLevel));

        OnPropertyChanged(
            nameof(IsMultiLevel));

        OnPropertyChanged(
            nameof(HasConstantProductionCapacity));

        OnPropertyChanged(
            nameof(IsCapacitated));

        OnPropertyChanged(
            nameof(HasLotSizeRestrictions));

        OnPropertyChanged(
            nameof(HasAdditionalCapacity));

        OnPropertyChanged(
            nameof(HasSupplyChainNetworkDecisions));

        OnPropertyChanged(
            nameof(HasStationaryDemand));

        OnPropertyChanged(
            nameof(IsStructurallyUsable));
    }
}