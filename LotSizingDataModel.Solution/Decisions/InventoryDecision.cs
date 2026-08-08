using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the inventory decisions associated with one item
/// and one warehouse over the complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// This class stores item-specific inventory decisions.
/// Global warehouse activation and global additional capacity
/// are stored separately in a warehouse-capacity decision.
/// </remarks>
[Serializable]
[XmlType(TypeName = "inventoryDecision")]
public sealed class InventoryDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _itemId;

    private WarehouseReference _warehouse =
        new();

    private DoubleTimeSeries _levels =
        new();

    private DoubleTimeSeries _safetyStockViolations =
        new();

    private IntegerTimeSeries _setups =
        new();

    private DoubleTimeSeries _additionalCapacityUsed =
        new();

    /// <summary>
    /// Initializes an empty inventory decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public InventoryDecision()
    {
        SubscribeToObject(_warehouse);
        SubscribeToObject(_levels);
        SubscribeToObject(_safetyStockViolations);
        SubscribeToObject(_setups);
        SubscribeToObject(_additionalCapacityUsed);
    }

    /// <summary>
    /// Initializes an inventory decision for an item,
    /// a warehouse and a planning horizon.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the stored item.
    /// </param>
    /// <param name="warehouse">
    /// Reference to the warehouse in which the item is stored.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public InventoryDecision(
        int itemId,
        WarehouseReference warehouse,
        int planningHorizon)
        : this()
    {
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemId),
                itemId,
                "The item identifier must be strictly positive.");
        }

        ArgumentNullException.ThrowIfNull(warehouse);

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        ItemId = itemId;
        Warehouse = warehouse;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the stored item.
    /// </summary>
    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set
        {
            /*
             * Zero is tolerated for an empty object created
             * by XmlSerializer. The solution validator will
             * require a strictly positive identifier.
             */
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The item identifier cannot be negative.");
            }

            SetProperty(
                ref _itemId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the reference to the warehouse
    /// associated with the inventory decision.
    /// </summary>
    [XmlElement("warehouse")]
    public WarehouseReference Warehouse
    {
        get => _warehouse;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _warehouse,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_warehouse);

            SetProperty(
                ref _warehouse,
                value);

            SubscribeToObject(_warehouse);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the inventory levels recorded
    /// during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    /// </remarks>
    [XmlElement("levels")]
    public DoubleTimeSeries Levels
    {
        get => _levels;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _levels,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_levels);

            SetProperty(
                ref _levels,
                value);

            SubscribeToObject(_levels);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the quantities by which the safety-stock
    /// requirement is violated during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// A zero value means that no safety-stock violation
    /// is recorded for the corresponding period.
    /// </remarks>
    [XmlElement("safetyStockViolations")]
    public DoubleTimeSeries SafetyStockViolations
    {
        get => _safetyStockViolations;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _safetyStockViolations,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _safetyStockViolations);

            SetProperty(
                ref _safetyStockViolations,
                value);

            SubscribeToObject(
                _safetyStockViolations);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the binary item-specific
    /// inventory-activation decisions.
    /// </summary>
    /// <remarks>
    /// Each value must be zero or one.
    /// </remarks>
    [XmlElement("setups")]
    public IntegerTimeSeries Setups
    {
        get => _setups;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _setups,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_setups);

            SetProperty(
                ref _setups,
                value);

            SubscribeToObject(_setups);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the item-specific additional capacity
    /// used during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// Global additional capacity used by the complete warehouse
    /// is not stored in this series.
    /// </remarks>
    [XmlElement("additionalCapacityUsed")]
    public DoubleTimeSeries AdditionalCapacityUsed
    {
        get => _additionalCapacityUsed;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _additionalCapacityUsed,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _additionalCapacityUsed);

            SetProperty(
                ref _additionalCapacityUsed,
                value);

            SubscribeToObject(
                _additionalCapacityUsed);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by the inventory-level series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Levels.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether every decision series
    /// uses the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        SafetyStockViolations.PeriodCount ==
            PlanningHorizon &&
        Setups.PeriodCount ==
            PlanningHorizon &&
        AdditionalCapacityUsed.PeriodCount ==
            PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether every inventory level
    /// is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidLevels =>
        Levels.All(
            level =>
                double.IsFinite(level) &&
                level >= 0.0);

    /// <summary>
    /// Gets a value indicating whether every safety-stock
    /// violation is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidSafetyStockViolations =>
        SafetyStockViolations.All(
            violation =>
                double.IsFinite(violation) &&
                violation >= 0.0);

    /// <summary>
    /// Gets a value indicating whether every setup value
    /// is equal to zero or one.
    /// </summary>
    [XmlIgnore]
    public bool HasValidSetupValues =>
        Setups.All(
            setup =>
                setup is 0 or 1);

    /// <summary>
    /// Gets a value indicating whether every additional-capacity
    /// value is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidAdditionalCapacityValues =>
        AdditionalCapacityUsed.All(
            capacity =>
                double.IsFinite(capacity) &&
                capacity >= 0.0);

    /// <summary>
    /// Gets a value indicating whether the inventory decision
    /// is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the item and warehouse
    /// exist in a particular supply-chain instance.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        ItemId > 0 &&
        PlanningHorizon > 0 &&
        HasConsistentPlanningHorizon &&
        HasValidLevels &&
        HasValidSafetyStockViolations &&
        HasValidSetupValues &&
        HasValidAdditionalCapacityValues;

    /// <summary>
    /// Gets the inventory level for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Inventory level recorded for the period.
    /// </returns>
    public double GetLevel(int period)
    {
        return Levels[period];
    }

    /// <summary>
    /// Sets the inventory level for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="level">
    /// Finite and non-negative inventory level.
    /// </param>
    public void SetLevel(
        int period,
        double level)
    {
        ValidateNonNegativeFiniteValue(
            level,
            nameof(level));

        Levels[period] = level;
    }

    /// <summary>
    /// Gets the safety-stock violation for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Non-negative safety-stock violation quantity.
    /// </returns>
    public double GetSafetyStockViolation(
        int period)
    {
        return SafetyStockViolations[period];
    }

    /// <summary>
    /// Sets the safety-stock violation for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="violation">
    /// Finite and non-negative violation quantity.
    /// </param>
    public void SetSafetyStockViolation(
        int period,
        double violation)
    {
        ValidateNonNegativeFiniteValue(
            violation,
            nameof(violation));

        SafetyStockViolations[period] =
            violation;
    }

    /// <summary>
    /// Determines whether the item-specific inventory activity
    /// is activated during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// True when the setup value is one; otherwise, false.
    /// </returns>
    public bool IsSetupActivated(int period)
    {
        return Setups[period] == 1;
    }

    /// <summary>
    /// Sets the item-specific inventory activation
    /// for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="isActivated">
    /// True to activate the inventory activity;
    /// otherwise, false.
    /// </param>
    public void SetSetupActivated(
        int period,
        bool isActivated)
    {
        Setups[period] =
            isActivated ? 1 : 0;
    }

    /// <summary>
    /// Gets the item-specific additional capacity used
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Non-negative additional-capacity quantity.
    /// </returns>
    public double GetAdditionalCapacityUsed(
        int period)
    {
        return AdditionalCapacityUsed[period];
    }

    /// <summary>
    /// Sets the item-specific additional capacity used
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="capacity">
    /// Finite and non-negative additional-capacity quantity.
    /// </param>
    public void SetAdditionalCapacityUsed(
        int period,
        double capacity)
    {
        ValidateNonNegativeFiniteValue(
            capacity,
            nameof(capacity));

        AdditionalCapacityUsed[period] =
            capacity;
    }

    /// <summary>
    /// Resizes every decision series to the specified
    /// planning horizon.
    /// </summary>
    /// <param name="periodCount">
    /// Non-negative number of planning periods.
    /// </param>
    /// <remarks>
    /// Existing values are preserved whenever possible.
    /// New periods are initialized with zero.
    /// </remarks>
    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The period count cannot be negative.");
        }

        Levels.Resize(
            periodCount,
            defaultValue: 0.0);

        SafetyStockViolations.Resize(
            periodCount,
            defaultValue: 0.0);

        Setups.Resize(
            periodCount,
            defaultValue: 0);

        AdditionalCapacityUsed.Resize(
            periodCount,
            defaultValue: 0.0);

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Resets every inventory decision value to zero.
    /// </summary>
    public void Clear()
    {
        Levels.Fill(0.0);
        SafetyStockViolations.Fill(0.0);
        Setups.Fill(0);
        AdditionalCapacityUsed.Fill(0.0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        double maximumLevel =
            Levels.Any()
                ? Levels.Max()
                : 0.0;

        double totalSafetyStockViolation =
            SafetyStockViolations.Sum();

        return
            $"Item {ItemId}, warehouse " +
            $"{Warehouse.Kind}:{Warehouse.ReferenceId}: " +
            $"maximum level {maximumLevel}; " +
            $"safety-stock violation " +
            $"{totalSafetyStockViolation}";
    }

    private void SubscribeToObject(
        ModelObject modelObject)
    {
        modelObject.PropertyChanged +=
            OnNestedPropertyChanged;
    }

    private void UnsubscribeFromObject(
        ModelObject modelObject)
    {
        modelObject.PropertyChanged -=
            OnNestedPropertyChanged;
    }

    private void OnNestedPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        NotifyDerivedProperties();
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(PlanningHorizon));

        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));

        OnPropertyChanged(
            nameof(HasValidLevels));

        OnPropertyChanged(
            nameof(HasValidSafetyStockViolations));

        OnPropertyChanged(
            nameof(HasValidSetupValues));

        OnPropertyChanged(
            nameof(HasValidAdditionalCapacityValues));

        OnPropertyChanged(
            nameof(IsInternallyValid));
    }

    private static void ValidateNonNegativeFiniteValue(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) ||
            value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value must be finite and non-negative.");
        }
    }
}