using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the transport decisions associated with one item,
/// one transport resource and one directed warehouse lane
/// over the complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// This class stores item-specific transport decisions.
/// Global transport-resource activation and global additional
/// capacity are stored in a separate capacity decision.
/// </remarks>
[Serializable]
[XmlType(TypeName = "transportDecision")]
public sealed class TransportDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _itemId;
    private int _transportResourceId;

    private WarehouseReference _origin =
        new();

    private WarehouseReference _destination =
        new();

    private DoubleTimeSeries _transportedQuantities =
        new();

    private IntegerTimeSeries _setups =
        new();

    private DoubleTimeSeries _additionalCapacityUsed =
        new();

    /// <summary>
    /// Initializes an empty transport decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public TransportDecision()
    {
        SubscribeToObject(_origin);
        SubscribeToObject(_destination);
        SubscribeToObject(_transportedQuantities);
        SubscribeToObject(_setups);
        SubscribeToObject(_additionalCapacityUsed);
    }

    /// <summary>
    /// Initializes a transport decision for an item,
    /// a transport resource and a directed warehouse lane.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the transported item.
    /// </param>
    /// <param name="transportResourceId">
    /// Identifier of the transport resource.
    /// </param>
    /// <param name="origin">
    /// Origin warehouse reference.
    /// </param>
    /// <param name="destination">
    /// Destination warehouse reference.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public TransportDecision(
        int itemId,
        int transportResourceId,
        WarehouseReference origin,
        WarehouseReference destination,
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

        if (transportResourceId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(transportResourceId),
                transportResourceId,
                "The transport-resource identifier must be " +
                "strictly positive.");
        }

        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        if (SameWarehouse(
                origin,
                destination))
        {
            throw new ArgumentException(
                "The origin and destination warehouses " +
                "must be different.",
                nameof(destination));
        }

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        ItemId = itemId;

        TransportResourceId =
            transportResourceId;

        Origin = origin;
        Destination = destination;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the transported item.
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
    /// Gets or sets the identifier of the transport resource
    /// used by the decision.
    /// </summary>
    [XmlAttribute("transportResourceId")]
    public int TransportResourceId
    {
        get => _transportResourceId;
        set
        {
            /*
             * Zero is tolerated during XML deserialization.
             */
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The transport-resource identifier " +
                    "cannot be negative.");
            }

            SetProperty(
                ref _transportResourceId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the origin warehouse reference.
    /// </summary>
    [XmlElement("origin")]
    public WarehouseReference Origin
    {
        get => _origin;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _origin,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_origin);

            SetProperty(
                ref _origin,
                value);

            SubscribeToObject(_origin);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the destination warehouse reference.
    /// </summary>
    [XmlElement("destination")]
    public WarehouseReference Destination
    {
        get => _destination;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _destination,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_destination);

            SetProperty(
                ref _destination,
                value);

            SubscribeToObject(_destination);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the quantities transported during
    /// each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    /// </remarks>
    [XmlElement("transportedQuantities")]
    public DoubleTimeSeries TransportedQuantities
    {
        get => _transportedQuantities;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _transportedQuantities,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _transportedQuantities);

            SetProperty(
                ref _transportedQuantities,
                value);

            SubscribeToObject(
                _transportedQuantities);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the binary item-specific transport
    /// activation decisions.
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
    /// Gets or sets the item-specific additional transport
    /// capacity used during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// Global additional capacity used by the transport resource
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
    /// by the transported-quantity series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        TransportedQuantities.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether all decision series
    /// use the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        Setups.PeriodCount ==
            PlanningHorizon &&
        AdditionalCapacityUsed.PeriodCount ==
            PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether every transported
    /// quantity is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidTransportedQuantities =>
        TransportedQuantities.All(
            quantity =>
                double.IsFinite(quantity) &&
                quantity >= 0.0);

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
    /// Gets a value indicating whether the origin and
    /// destination identify different warehouses.
    /// </summary>
    [XmlIgnore]
    public bool HasValidLane =>
        Origin.ReferenceId > 0 &&
        Destination.ReferenceId > 0 &&
        !SameWarehouse(
            Origin,
            Destination);

    /// <summary>
    /// Gets a value indicating whether the transport decision
    /// is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the item, transport
    /// resource or transport lane exists in a particular
    /// supply-chain instance.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        ItemId > 0 &&
        TransportResourceId > 0 &&
        PlanningHorizon > 0 &&
        HasValidLane &&
        HasConsistentPlanningHorizon &&
        HasValidTransportedQuantities &&
        HasValidSetupValues &&
        HasValidAdditionalCapacityValues;

    /// <summary>
    /// Gets the transported quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Transported quantity recorded for the period.
    /// </returns>
    public double GetTransportedQuantity(int period)
    {
        return TransportedQuantities[period];
    }

    /// <summary>
    /// Sets the transported quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="quantity">
    /// Finite and non-negative transported quantity.
    /// </param>
    public void SetTransportedQuantity(
        int period,
        double quantity)
    {
        ValidateNonNegativeFiniteValue(
            quantity,
            nameof(quantity));

        TransportedQuantities[period] =
            quantity;
    }

    /// <summary>
    /// Determines whether item-specific transport is activated
    /// during a planning period.
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
    /// Sets the item-specific transport activation
    /// for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="isActivated">
    /// True to activate transport; otherwise, false.
    /// </param>
    public void SetSetupActivated(
        int period,
        bool isActivated)
    {
        Setups[period] =
            isActivated ? 1 : 0;
    }

    /// <summary>
    /// Gets the item-specific additional transport capacity
    /// used during a planning period.
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
    /// Sets the item-specific additional transport capacity
    /// used during a planning period.
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
    /// Determines whether this decision identifies the
    /// specified item, resource and directed warehouse lane.
    /// </summary>
    /// <param name="itemId">
    /// Item identifier to compare.
    /// </param>
    /// <param name="transportResourceId">
    /// Transport-resource identifier to compare.
    /// </param>
    /// <param name="origin">
    /// Origin warehouse to compare.
    /// </param>
    /// <param name="destination">
    /// Destination warehouse to compare.
    /// </param>
    /// <returns>
    /// True when all key elements match; otherwise, false.
    /// </returns>
    public bool Matches(
        int itemId,
        int transportResourceId,
        WarehouseReference origin,
        WarehouseReference destination)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        return ItemId == itemId &&
               TransportResourceId ==
                   transportResourceId &&
               SameWarehouse(
                   Origin,
                   origin) &&
               SameWarehouse(
                   Destination,
                   destination);
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

        TransportedQuantities.Resize(
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
    /// Resets every transport decision value to zero.
    /// </summary>
    public void Clear()
    {
        TransportedQuantities.Fill(0.0);
        Setups.Fill(0);
        AdditionalCapacityUsed.Fill(0.0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        double totalQuantity =
            TransportedQuantities.Sum();

        return
            $"Item {ItemId}, transport resource " +
            $"{TransportResourceId}, " +
            $"{FormatWarehouse(Origin)} -> " +
            $"{FormatWarehouse(Destination)}: " +
            $"total quantity {totalQuantity}";
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
            nameof(HasValidTransportedQuantities));

        OnPropertyChanged(
            nameof(HasValidSetupValues));

        OnPropertyChanged(
            nameof(HasValidAdditionalCapacityValues));

        OnPropertyChanged(
            nameof(HasValidLane));

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

    private static string FormatWarehouse(
        WarehouseReference warehouse)
    {
        return
            $"{warehouse.Kind}:{warehouse.ReferenceId}";
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