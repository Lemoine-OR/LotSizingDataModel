using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.DecisionModel.Rules;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Adds the decision-model parameters associated with
/// storing an item in a warehouse.
///
/// The Inventory class represents the relationship between
/// one item and one warehouse.
/// </summary>
public sealed partial class Inventory :
    IPlanningHorizonAware
{
    private InventoryBalanceRule _inventoryBalanceRule = new();

    private ScheduledReceipt? _scheduledReceipt;
    private SafetyStock? _safetyStock;
    private SafetyStockViolationCost? _safetyStockViolationCost;

    private CapacityConstraint? _capacityConstraint;
    private AdditionalCapacity? _additionalCapacity;
    private AdditionalCapacityCost? _additionalCapacityCost;

    private UnitCapacityConsumption? _unitCapacityConsumption;
    private SetupTime? _setupTime;
    private FixedSetupCost? _fixedSetupCost;
    private UnitUsageCost? _unitUsageCost;

    /// <summary>
    /// Gets or sets the inventory-balance rule.
    ///
    /// Every inventory must have exactly one balance rule.
    /// </summary>
    [XmlElement("inventoryBalanceRule")]
    public InventoryBalanceRule InventoryBalanceRule
    {
        get => _inventoryBalanceRule;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(_inventoryBalanceRule, value))
            {
                return;
            }

            _inventoryBalanceRule = value;

            OnPropertyChanged(nameof(InventoryBalanceRule));
        }
    }

    /// <summary>
    /// Gets or sets the quantities from orders already in progress
    /// and expected to be received during the planning horizon.
    ///
    /// Corresponds to QEnc[v,i,t].
    /// </summary>
    [XmlElement("scheduledReceipt")]
    public ScheduledReceipt? ScheduledReceipt
    {
        get => _scheduledReceipt;
        set => SetDecisionParameter(
            ref _scheduledReceipt,
            value,
            nameof(ScheduledReceipt));
    }

    /// <summary>
    /// Gets or sets the required safety-stock level
    /// for each planning period.
    ///
    /// Corresponds to Isecu[v,i,t].
    /// </summary>
    [XmlElement("safetyStock")]
    public SafetyStock? SafetyStock
    {
        get => _safetyStock;
        set => SetDecisionParameter(
            ref _safetyStock,
            value,
            nameof(SafetyStock));
    }

    /// <summary>
    /// Gets or sets the cost incurred for each unit
    /// missing from the required safety-stock level.
    ///
    /// Corresponds to CSecu[v,i,t].
    /// </summary>
    [XmlElement("safetyStockViolationCost")]
    public SafetyStockViolationCost?
        SafetyStockViolationCost
    {
        get => _safetyStockViolationCost;
        set => SetDecisionParameter(
            ref _safetyStockViolationCost,
            value,
            nameof(SafetyStockViolationCost));
    }

    /// <summary>
    /// Gets or sets the maximum storage capacity reserved
    /// for this item during each planning period.
    ///
    /// Corresponds to CMax[v,i,t].
    ///
    /// This item-specific constraint is independent from the
    /// optional global warehouse capacity constraint.
    /// </summary>
    [XmlElement("capacityConstraint")]
    public CapacityConstraint? CapacityConstraint
    {
        get => _capacityConstraint;
        set => SetDecisionParameter(
            ref _capacityConstraint,
            value,
            nameof(CapacityConstraint));
    }

    /// <summary>
    /// Gets or sets the maximum additional storage capacity
    /// available specifically for this item.
    ///
    /// Corresponds to CSuppl[v,i,t].
    ///
    /// This parameter requires an item-specific
    /// <see cref="CapacityConstraint"/>.
    /// </summary>
    [XmlElement("additionalCapacity")]
    public AdditionalCapacity? AdditionalCapacity
    {
        get => _additionalCapacity;
        set => SetDecisionParameter(
            ref _additionalCapacity,
            value,
            nameof(AdditionalCapacity));
    }

    /// <summary>
    /// Gets or sets the cost of using one unit of additional
    /// storage capacity for this item.
    ///
    /// Corresponds to COV[v,i,t].
    ///
    /// This parameter requires an item-specific
    /// <see cref="AdditionalCapacity"/>.
    /// </summary>
    [XmlElement("additionalCapacityCost")]
    public AdditionalCapacityCost? AdditionalCapacityCost
    {
        get => _additionalCapacityCost;
        set => SetDecisionParameter(
            ref _additionalCapacityCost,
            value,
            nameof(AdditionalCapacityCost));
    }

    /// <summary>
    /// Gets or sets the storage capacity consumed
    /// by one unit of the item.
    ///
    /// Corresponds to CUnit[v,i,t].
    ///
    /// This parameter may contribute either to an item-specific
    /// capacity constraint or to the global warehouse capacity.
    /// </summary>
    [XmlElement("unitCapacityConsumption")]
    public UnitCapacityConsumption? UnitCapacityConsumption
    {
        get => _unitCapacityConsumption;
        set => SetDecisionParameter(
            ref _unitCapacityConsumption,
            value,
            nameof(UnitCapacityConsumption));
    }

    /// <summary>
    /// Gets or sets the capacity consumed to prepare
    /// the warehouse storage resource for this item.
    ///
    /// Corresponds to CSetup[v,i,t].
    /// </summary>
    [XmlElement("setupTime")]
    public SetupTime? SetupTime
    {
        get => _setupTime;
        set => SetDecisionParameter(
            ref _setupTime,
            value,
            nameof(SetupTime));
    }

    /// <summary>
    /// Gets or sets the fixed preparation cost associated
    /// with storing this item.
    ///
    /// Corresponds to Cprep[v,i,t].
    /// </summary>
    [XmlElement("fixedSetupCost")]
    public FixedSetupCost? FixedSetupCost
    {
        get => _fixedSetupCost;
        set => SetDecisionParameter(
            ref _fixedSetupCost,
            value,
            nameof(FixedSetupCost));
    }

    /// <summary>
    /// Gets or sets the cost of storing one unit of the item
    /// during each planning period.
    ///
    /// Corresponds to Cutil[v,i,t].
    /// </summary>
    [XmlElement("unitUsageCost")]
    public UnitUsageCost? UnitUsageCost
    {
        get => _unitUsageCost;
        set => SetDecisionParameter(
            ref _unitUsageCost,
            value,
            nameof(UnitUsageCost));
    }

    /// <summary>
    /// Gets the planning horizon represented by the first
    /// active period-dependent parameter.
    ///
    /// Returns zero when no period-dependent parameter is active.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon
    {
        get
        {
            // Return the first active parameter's horizon.
            foreach (IPlanningHorizonAware parameter
                     in GetDecisionParameters())
            {
                return parameter.PlanningHorizon;
            }

            // No active parameter → horizon is zero.
            return 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether at least one optional
    /// decision-model parameter is active.
    ///
    /// The mandatory inventory-balance rule is not counted
    /// as an optional decision parameter.
    /// </summary>
    [XmlIgnore]
    public bool HasDecisionParameters =>
        ScheduledReceipt is not null ||
        SafetyStock is not null ||
        SafetyStockViolationCost is not null ||
        CapacityConstraint is not null ||
        AdditionalCapacity is not null ||
        AdditionalCapacityCost is not null ||
        UnitCapacityConsumption is not null ||
        SetupTime is not null ||
        FixedSetupCost is not null ||
        UnitUsageCost is not null;

    /// <summary>
    /// Gets a value indicating whether all active parameters
    /// use the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon
    {
        get
        {
            int? expectedHorizon = null;

            // Compare each active parameter's horizon against the first.
            foreach (IPlanningHorizonAware parameter
                     in GetDecisionParameters())
            {
                if (expectedHorizon is null)
                {
                    expectedHorizon =
                        parameter.PlanningHorizon;

                    continue;
                }

                if (parameter.PlanningHorizon !=
                    expectedHorizon.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the direct dependencies
    /// between the inventory parameters are respected.
    ///
    /// Cross-object dependencies involving the referenced warehouse
    /// are checked by the global supply-chain validator.
    /// </summary>
    [XmlIgnore]
    public bool HasValidDecisionConfiguration =>
        (AdditionalCapacity is null ||
         CapacityConstraint is not null) &&

        (AdditionalCapacityCost is null ||
         AdditionalCapacity is not null) &&

        (SafetyStockViolationCost is null ||
         SafetyStock is not null);

    /// <summary>
    /// Resizes every active period-dependent parameter.
    ///
    /// Existing values are preserved. Values assigned to newly
    /// created periods depend on the corresponding parameter class.
    /// </summary>
    /// <param name="periodCount">
    /// New number of planning periods.
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

        // Resize every active decision parameter.
        foreach (IPlanningHorizonAware parameter
                 in GetDecisionParameters())
        {
            parameter.ResizeTimeSeries(periodCount);
        }

        // Notify dependent computed properties.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Removes all optional decision-model parameters.
    ///
    /// The mandatory inventory-balance rule, item reference,
    /// warehouse reference and initial inventory are preserved.
    /// </summary>
    public void ClearDecisionParameters()
    {
        ScheduledReceipt = null;

        SafetyStockViolationCost = null;
        SafetyStock = null;

        AdditionalCapacityCost = null;
        AdditionalCapacity = null;
        CapacityConstraint = null;

        UnitCapacityConsumption = null;
        SetupTime = null;
        FixedSetupCost = null;
        UnitUsageCost = null;
    }

    /// <summary>
    /// Assigns a decision parameter and manages its
    /// PropertyChanged event subscription.
    /// </summary>
    private void SetDecisionParameter<T>(
        ref T? field,
        T? value,
        string propertyName)
        where T : ModelObject, IPlanningHorizonAware
    {
        if (ReferenceEquals(field, value))
        {
            return;
        }

        // Unsubscribe from old parameter.
        if (field is not null)
        {
            field.PropertyChanged -=
                OnDecisionParameterPropertyChanged;
        }

        field = value;

        // Subscribe to new parameter.
        if (field is not null)
        {
            field.PropertyChanged +=
                OnDecisionParameterPropertyChanged;
        }

        // Notify the specific property plus all dependent ones.
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasDecisionParameters));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(
            nameof(HasValidDecisionConfiguration));
    }

    /// <summary>
    /// Enumerates every active period-dependent parameter
    /// associated with this inventory.
    /// </summary>
    private IEnumerable<IPlanningHorizonAware>
        GetDecisionParameters()
    {
        if (ScheduledReceipt is not null)
        {
            yield return ScheduledReceipt;
        }

        if (SafetyStock is not null)
        {
            yield return SafetyStock;
        }

        if (SafetyStockViolationCost is not null)
        {
            yield return SafetyStockViolationCost;
        }

        if (CapacityConstraint is not null)
        {
            yield return CapacityConstraint;
        }

        if (AdditionalCapacity is not null)
        {
            yield return AdditionalCapacity;
        }

        if (AdditionalCapacityCost is not null)
        {
            yield return AdditionalCapacityCost;
        }

        if (UnitCapacityConsumption is not null)
        {
            yield return UnitCapacityConsumption;
        }

        if (SetupTime is not null)
        {
            yield return SetupTime;
        }

        if (FixedSetupCost is not null)
        {
            yield return FixedSetupCost;
        }

        if (UnitUsageCost is not null)
        {
            yield return UnitUsageCost;
        }
    }

    /// <summary>
    /// Propagates changes raised by nested decision parameters.
    /// </summary>
    private void OnDecisionParameterPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Forward notification for the specific parameter that changed.
        if (ReferenceEquals(sender, ScheduledReceipt))
        {
            OnPropertyChanged(nameof(ScheduledReceipt));
        }
        else if (ReferenceEquals(sender, SafetyStock))
        {
            OnPropertyChanged(nameof(SafetyStock));
        }
        else if (ReferenceEquals(
                     sender,
                     SafetyStockViolationCost))
        {
            OnPropertyChanged(
                nameof(SafetyStockViolationCost));
        }
        else if (ReferenceEquals(
                     sender,
                     CapacityConstraint))
        {
            OnPropertyChanged(nameof(CapacityConstraint));
        }
        else if (ReferenceEquals(
                     sender,
                     AdditionalCapacity))
        {
            OnPropertyChanged(nameof(AdditionalCapacity));
        }
        else if (ReferenceEquals(
                     sender,
                     AdditionalCapacityCost))
        {
            OnPropertyChanged(
                nameof(AdditionalCapacityCost));
        }
        else if (ReferenceEquals(
                     sender,
                     UnitCapacityConsumption))
        {
            OnPropertyChanged(
                nameof(UnitCapacityConsumption));
        }
        else if (ReferenceEquals(sender, SetupTime))
        {
            OnPropertyChanged(nameof(SetupTime));
        }
        else if (ReferenceEquals(sender, FixedSetupCost))
        {
            OnPropertyChanged(nameof(FixedSetupCost));
        }
        else if (ReferenceEquals(sender, UnitUsageCost))
        {
            OnPropertyChanged(nameof(UnitUsageCost));
        }

        // Update all computed properties that may be affected.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }
}