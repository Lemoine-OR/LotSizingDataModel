using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the distribution decisions associated with one
/// distribution center, one item and one warehouse over the
/// complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// Demand values, selling prices, shortage costs and backlog
/// costs belong to the supply-chain instance and are not
/// duplicated in this solution object.
/// </remarks>
[Serializable]
[XmlType(TypeName = "distributionDecision")]
public sealed class DistributionDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _distributionCenterId;
    private int _itemId;

    private WarehouseReference _warehouse =
        new();

    private DoubleTimeSeries _deliveredQuantities =
        new();

    private DoubleTimeSeries _backlogLevels =
        new();

    private DoubleTimeSeries _shortageQuantities =
        new();

    /// <summary>
    /// Initializes an empty distribution decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public DistributionDecision()
    {
        SubscribeToObject(_warehouse);
        SubscribeToObject(_deliveredQuantities);
        SubscribeToObject(_backlogLevels);
        SubscribeToObject(_shortageQuantities);
    }

    /// <summary>
    /// Initializes a distribution decision for a distribution
    /// center, an item, a warehouse and a planning horizon.
    /// </summary>
    /// <param name="distributionCenterId">
    /// Identifier of the distribution center.
    /// </param>
    /// <param name="itemId">
    /// Identifier of the distributed item.
    /// </param>
    /// <param name="warehouse">
    /// Warehouse from which the item is delivered.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public DistributionDecision(
        int distributionCenterId,
        int itemId,
        WarehouseReference warehouse,
        int planningHorizon)
        : this()
    {
        if (distributionCenterId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distributionCenterId),
                distributionCenterId,
                "The distribution-center identifier must be " +
                "strictly positive.");
        }

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

        DistributionCenterId =
            distributionCenterId;

        ItemId = itemId;
        Warehouse = warehouse;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the distribution center.
    /// </summary>
    [XmlAttribute("distributionCenterId")]
    public int DistributionCenterId
    {
        get => _distributionCenterId;
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
                    "The distribution-center identifier " +
                    "cannot be negative.");
            }

            SetProperty(
                ref _distributionCenterId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the distributed item.
    /// </summary>
    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
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
                    "The item identifier cannot be negative.");
            }

            SetProperty(
                ref _itemId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the warehouse from which the item
    /// is delivered.
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
    /// Gets or sets the quantities delivered to customers
    /// during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    /// </remarks>
    [XmlElement("deliveredQuantities")]
    public DoubleTimeSeries DeliveredQuantities
    {
        get => _deliveredQuantities;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _deliveredQuantities,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _deliveredQuantities);

            SetProperty(
                ref _deliveredQuantities,
                value);

            SubscribeToObject(
                _deliveredQuantities);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the outstanding demand at the end
    /// of each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// A backlog is assumed to remain eligible for fulfillment
    /// during a later planning period.
    /// </remarks>
    [XmlElement("backlogLevels")]
    public DoubleTimeSeries BacklogLevels
    {
        get => _backlogLevels;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _backlogLevels,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _backlogLevels);

            SetProperty(
                ref _backlogLevels,
                value);

            SubscribeToObject(
                _backlogLevels);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the quantities of demand that are
    /// definitively not fulfilled during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// Unlike a backlog, a shortage quantity is not carried
    /// forward to a later planning period. When lost sales are
    /// not permitted by the model, this series remains zero.
    /// </remarks>
    [XmlElement("shortageQuantities")]
    public DoubleTimeSeries ShortageQuantities
    {
        get => _shortageQuantities;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _shortageQuantities,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _shortageQuantities);

            SetProperty(
                ref _shortageQuantities,
                value);

            SubscribeToObject(
                _shortageQuantities);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by the delivered-quantity series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        DeliveredQuantities.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether all decision series
    /// use the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        BacklogLevels.PeriodCount ==
            PlanningHorizon &&
        ShortageQuantities.PeriodCount ==
            PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether every delivered
    /// quantity is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidDeliveredQuantities =>
        DeliveredQuantities.All(
            quantity =>
                double.IsFinite(quantity) &&
                quantity >= 0.0);

    /// <summary>
    /// Gets a value indicating whether every backlog level
    /// is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidBacklogLevels =>
        BacklogLevels.All(
            backlog =>
                double.IsFinite(backlog) &&
                backlog >= 0.0);

    /// <summary>
    /// Gets a value indicating whether every shortage
    /// quantity is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidShortageQuantities =>
        ShortageQuantities.All(
            shortage =>
                double.IsFinite(shortage) &&
                shortage >= 0.0);

    /// <summary>
    /// Gets a value indicating whether the warehouse
    /// reference is initialized.
    /// </summary>
    [XmlIgnore]
    public bool HasValidWarehouse =>
        Warehouse.ReferenceId > 0;

    /// <summary>
    /// Gets a value indicating whether the distribution
    /// decision is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the distribution
    /// center, item and warehouse exist in a particular
    /// supply-chain instance.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        DistributionCenterId > 0 &&
        ItemId > 0 &&
        PlanningHorizon > 0 &&
        HasValidWarehouse &&
        HasConsistentPlanningHorizon &&
        HasValidDeliveredQuantities &&
        HasValidBacklogLevels &&
        HasValidShortageQuantities;

    /// <summary>
    /// Gets the quantity delivered during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Delivered quantity recorded for the period.
    /// </returns>
    public double GetDeliveredQuantity(int period)
    {
        return DeliveredQuantities[period];
    }

    /// <summary>
    /// Sets the quantity delivered during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="quantity">
    /// Finite and non-negative delivered quantity.
    /// </param>
    public void SetDeliveredQuantity(
        int period,
        double quantity)
    {
        ValidateNonNegativeFiniteValue(
            quantity,
            nameof(quantity));

        DeliveredQuantities[period] =
            quantity;
    }

    /// <summary>
    /// Gets the backlog level at the end of a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Non-negative backlog level.
    /// </returns>
    public double GetBacklogLevel(int period)
    {
        return BacklogLevels[period];
    }

    /// <summary>
    /// Sets the backlog level at the end of a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="backlog">
    /// Finite and non-negative backlog level.
    /// </param>
    public void SetBacklogLevel(
        int period,
        double backlog)
    {
        ValidateNonNegativeFiniteValue(
            backlog,
            nameof(backlog));

        BacklogLevels[period] =
            backlog;
    }

    /// <summary>
    /// Gets the shortage quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Non-negative shortage quantity.
    /// </returns>
    public double GetShortageQuantity(int period)
    {
        return ShortageQuantities[period];
    }

    /// <summary>
    /// Sets the shortage quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="shortage">
    /// Finite and non-negative shortage quantity.
    /// </param>
    public void SetShortageQuantity(
        int period,
        double shortage)
    {
        ValidateNonNegativeFiniteValue(
            shortage,
            nameof(shortage));

        ShortageQuantities[period] =
            shortage;
    }

    /// <summary>
    /// Determines whether this decision identifies the
    /// specified distribution center, item and warehouse.
    /// </summary>
    /// <param name="distributionCenterId">
    /// Distribution-center identifier to compare.
    /// </param>
    /// <param name="itemId">
    /// Item identifier to compare.
    /// </param>
    /// <param name="warehouse">
    /// Warehouse reference to compare.
    /// </param>
    /// <returns>
    /// True when all key elements match; otherwise, false.
    /// </returns>
    public bool Matches(
        int distributionCenterId,
        int itemId,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        return DistributionCenterId ==
                   distributionCenterId &&
               ItemId == itemId &&
               SameWarehouse(
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

        DeliveredQuantities.Resize(
            periodCount,
            defaultValue: 0.0);

        BacklogLevels.Resize(
            periodCount,
            defaultValue: 0.0);

        ShortageQuantities.Resize(
            periodCount,
            defaultValue: 0.0);

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Resets every distribution decision value to zero.
    /// </summary>
    public void Clear()
    {
        DeliveredQuantities.Fill(0.0);
        BacklogLevels.Fill(0.0);
        ShortageQuantities.Fill(0.0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        double totalDeliveredQuantity =
            DeliveredQuantities.Sum();

        double finalBacklog =
            PlanningHorizon > 0
                ? BacklogLevels[PlanningHorizon]
                : 0.0;

        double totalShortage =
            ShortageQuantities.Sum();

        return
            $"Distribution center {DistributionCenterId}, " +
            $"item {ItemId}, warehouse " +
            $"{FormatWarehouse(Warehouse)}: " +
            $"delivered {totalDeliveredQuantity}; " +
            $"final backlog {finalBacklog}; " +
            $"shortage {totalShortage}";
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
            nameof(HasValidDeliveredQuantities));

        OnPropertyChanged(
            nameof(HasValidBacklogLevels));

        OnPropertyChanged(
            nameof(HasValidShortageQuantities));

        OnPropertyChanged(
            nameof(HasValidWarehouse));

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