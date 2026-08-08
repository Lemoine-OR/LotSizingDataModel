using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the global capacity decisions associated with one
/// warehouse over the complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// Item-specific inventory levels, setups and additional-capacity
/// decisions are stored separately in inventory decisions.
/// </remarks>
[Serializable]
[XmlType(TypeName = "warehouseCapacityDecision")]
public sealed class WarehouseCapacityDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private WarehouseReference _warehouse =
        new();

    private IntegerTimeSeries _activations =
        new();

    private DoubleTimeSeries _additionalCapacityUsed =
        new();

    /// <summary>
    /// Initializes an empty warehouse-capacity decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public WarehouseCapacityDecision()
    {
        SubscribeToObject(_warehouse);
        SubscribeToObject(_activations);
        SubscribeToObject(_additionalCapacityUsed);
    }

    /// <summary>
    /// Initializes a capacity decision for a warehouse
    /// and a planning horizon.
    /// </summary>
    /// <param name="warehouse">
    /// Reference to the warehouse.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public WarehouseCapacityDecision(
        WarehouseReference warehouse,
        int planningHorizon)
        : this()
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        if (warehouse.ReferenceId <= 0)
        {
            throw new ArgumentException(
                "The warehouse reference identifier must be " +
                "strictly positive.",
                nameof(warehouse));
        }

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        Warehouse = warehouse;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the warehouse associated with
    /// the capacity decision.
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
    /// Gets or sets the binary global warehouse
    /// activation decisions.
    /// </summary>
    /// <remarks>
    /// Each value must be zero or one.
    ///
    /// When the supply-chain model does not contain a fixed
    /// warehouse-activation decision, this series may remain
    /// filled with zeros.
    /// </remarks>
    [XmlElement("activations")]
    public IntegerTimeSeries Activations
    {
        get => _activations;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _activations,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_activations);

            SetProperty(
                ref _activations,
                value);

            SubscribeToObject(_activations);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the global additional capacity used
    /// by the warehouse during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// The maximum available additional capacity belongs
    /// to the supply-chain instance and is not duplicated
    /// in this solution object.
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
    /// by the activation series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Activations.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether every decision series
    /// uses the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        AdditionalCapacityUsed.PeriodCount ==
            PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether the warehouse
    /// reference is initialized.
    /// </summary>
    [XmlIgnore]
    public bool HasValidWarehouseReference =>
        Warehouse.ReferenceId > 0;

    /// <summary>
    /// Gets a value indicating whether every activation
    /// value is equal to zero or one.
    /// </summary>
    [XmlIgnore]
    public bool HasValidActivationValues =>
        Activations.All(
            activation =>
                activation is 0 or 1);

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
    /// Gets a value indicating whether the warehouse-capacity
    /// decision is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the warehouse exists
    /// in a particular supply-chain instance or that the used
    /// capacity remains below the available capacity.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        PlanningHorizon > 0 &&
        HasValidWarehouseReference &&
        HasConsistentPlanningHorizon &&
        HasValidActivationValues &&
        HasValidAdditionalCapacityValues;

    /// <summary>
    /// Determines whether the warehouse is activated
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// True when the activation value is one;
    /// otherwise, false.
    /// </returns>
    public bool IsActivated(int period)
    {
        return Activations[period] == 1;
    }

    /// <summary>
    /// Sets the global warehouse activation decision
    /// for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="isActivated">
    /// True to activate the warehouse;
    /// otherwise, false.
    /// </param>
    public void SetActivated(
        int period,
        bool isActivated)
    {
        Activations[period] =
            isActivated ? 1 : 0;
    }

    /// <summary>
    /// Gets the global additional warehouse capacity used
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
    /// Sets the global additional warehouse capacity used
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
        if (!double.IsFinite(capacity) ||
            capacity < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                "The additional capacity must be finite " +
                "and non-negative.");
        }

        AdditionalCapacityUsed[period] =
            capacity;
    }

    /// <summary>
    /// Determines whether this decision refers to
    /// the specified warehouse.
    /// </summary>
    /// <param name="warehouse">
    /// Warehouse reference to compare.
    /// </param>
    /// <returns>
    /// True when the warehouse kind and reference identifier
    /// match; otherwise, false.
    /// </returns>
    public bool Matches(
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        return SameWarehouse(
            Warehouse,
            warehouse);
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

        Activations.Resize(
            periodCount,
            defaultValue: 0);

        AdditionalCapacityUsed.Resize(
            periodCount,
            defaultValue: 0.0);

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Resets every warehouse-capacity decision value
    /// to zero.
    /// </summary>
    public void Clear()
    {
        Activations.Fill(0);
        AdditionalCapacityUsed.Fill(0.0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        int activatedPeriodCount =
            Activations.Count(
                activation =>
                    activation == 1);

        double totalAdditionalCapacity =
            AdditionalCapacityUsed.Sum();

        return
            $"Warehouse {Warehouse.Kind}:" +
            $"{Warehouse.ReferenceId}: " +
            $"activated periods {activatedPeriodCount}; " +
            $"additional capacity " +
            $"{totalAdditionalCapacity}";
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
            nameof(HasValidWarehouseReference));

        OnPropertyChanged(
            nameof(HasValidActivationValues));

        OnPropertyChanged(
            nameof(HasValidAdditionalCapacityValues));

        OnPropertyChanged(
            nameof(IsInternallyValid));
    }

    private static bool SameWarehouse(
        WarehouseReference first,
        WarehouseReference second)
    {
        return first.Kind ==
                   second.Kind &&
               first.ReferenceId ==
                   second.ReferenceId;
    }
}