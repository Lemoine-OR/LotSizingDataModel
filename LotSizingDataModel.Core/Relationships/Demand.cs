using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents the demand for an item issued by a distribution center.
///
/// Corresponds to the UML association class "Demande"
/// between Article and Centre de distribution.
///
/// Demand quantities correspond to D[c,i,t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "demand")]
public sealed partial class Demand :
    ModelObject,
    IPlanningHorizonAware
{
    private int _itemId;
    private int _distributionCenterId;
    private DoubleTimeSeries _quantities = new();

    /// <summary>
    /// Initializes an empty demand.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public Demand()
    {
        // Subscribe to changes in the quantities time series
        SubscribeToQuantities(_quantities);
    }

    /// <summary>
    /// Initializes a demand for an item and a distribution center.
    /// </summary>
    /// <param name="itemId">
    /// Identifier of the requested item.
    /// </param>
    /// <param name="distributionCenterId">
    /// Identifier of the distribution center issuing the demand.
    /// </param>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public Demand(
        int itemId,
        int distributionCenterId,
        int planningHorizon)
        : this()  // Call default constructor to subscribe to quantities
    {
        // Initialize demand properties (validation occurs in setters)
        ItemId = itemId;
        DistributionCenterId = distributionCenterId;
        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the requested item.
    ///
    /// Corresponds to index i in D[c,i,t].
    /// </summary>
    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set
        {
            // Validate that the item identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The item identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _itemId, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the distribution center
    /// issuing the demand.
    ///
    /// Corresponds to index c in D[c,i,t].
    /// </summary>
    [XmlAttribute("distributionCenterId")]
    public int DistributionCenterId
    {
        get => _distributionCenterId;
        set
        {
            // Validate that the distribution center identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The distribution-center identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _distributionCenterId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the quantities requested for each planning period.
    ///
    /// The value for period t corresponds to D[c,i,t]
    /// in the source model.
    /// </summary>
    [XmlElement("quantities")]
    public DoubleTimeSeries Quantities
    {
        get => _quantities;
        set
        {
            // Ensure the time series is never null
            DoubleTimeSeries newValue =
                value ?? new DoubleTimeSeries();

            // Avoid unnecessary updates if the reference is the same
            if (ReferenceEquals(_quantities, newValue))
            {
                return;
            }

            // Unsubscribe from the old time series
            UnsubscribeFromQuantities(_quantities);

            _quantities = newValue;

            // Subscribe to the new time series
            SubscribeToQuantities(_quantities);

            // Notify dependent properties
            OnPropertyChanged(nameof(Quantities));
            OnPropertyChanged(nameof(PlanningHorizon));
        }
    }

    /// <summary>
    /// Gets the number of periods represented by the demand.
    ///
    /// This calculated property is not serialized because the information
    /// is already contained in <see cref="Quantities"/>.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Quantities.PeriodCount;

    /// <summary>
    /// Gets the demand quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetQuantity(int period)
    {
        return Quantities[period];
    }

    /// <summary>
    /// Sets the demand quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="quantity">
    /// Non-negative finite demand quantity.
    /// </param>
    public void SetQuantity(
        int period,
        double quantity)
    {
        // Validate the quantity before setting
        ValidateQuantity(quantity, nameof(quantity));

        Quantities[period] = quantity;
    }

    /// <summary>
    /// Assigns the same demand quantity to every period.
    /// </summary>
    public void Fill(double quantity)
    {
        // Validate the quantity before filling
        ValidateQuantity(quantity, nameof(quantity));

        Quantities.Fill(quantity);
    }

    /// <summary>
    /// Resizes the period-dependent demand data.
    ///
    /// Existing values are preserved. Newly created periods receive
    /// a demand quantity of zero.
    /// </summary>
    public void ResizeTimeSeries(int periodCount)
    {
        // Validate that the period count is non-negative
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
        }

        // Resize quantities with zero as default value for new periods
        Quantities.Resize(
            periodCount,
            defaultValue: 0.0);
    }

    /// <summary>
    /// Determines whether this object represents the same
    /// item/distribution-center association as another demand.
    /// </summary>
    public bool RefersToSameDemand(Demand? other)
    {
        // Compare both item ID and distribution center ID for equality
        return other is not null
               && ItemId == other.ItemId
               && DistributionCenterId ==
                  other.DistributionCenterId;
    }

    /// <summary>
    /// Determines whether this demand concerns the specified item.
    /// </summary>
    public bool ConcernsItem(int itemId)
    {
        return ItemId == itemId;
    }

    /// <summary>
    /// Determines whether this demand concerns
    /// the specified distribution center.
    /// </summary>
    public bool ConcernsDistributionCenter(
        int distributionCenterId)
    {
        // Check if the distribution center ID matches
        return DistributionCenterId ==
               distributionCenterId;
    }

    /// <summary>
    /// Checks whether all currently stored demand quantities
    /// are finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidQuantities =>
        Quantities.All(
            quantity =>
                !double.IsNaN(quantity)
                && !double.IsInfinity(quantity)
                && quantity >= 0.0);

    /// <summary>
    /// Subscribes to property change notifications from the quantities time series.
    /// </summary>
    private void SubscribeToQuantities(
        DoubleTimeSeries quantities)
    {
        // Listen to property changes in the time series
        quantities.PropertyChanged +=
            OnQuantitiesPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from property change notifications from the quantities time series.
    /// </summary>
    private void UnsubscribeFromQuantities(
        DoubleTimeSeries quantities)
    {
        // Stop listening to property changes in the time series
        quantities.PropertyChanged -=
            OnQuantitiesPropertyChanged;
    }

    /// <summary>
    /// Handles property change notifications from the quantities time series
    /// and propagates relevant changes to dependent properties.
    /// </summary>
    private void OnQuantitiesPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Always notify that Quantities changed
        OnPropertyChanged(nameof(Quantities));

        // Notify dependent properties when period count or values change
        if (e.PropertyName ==
                nameof(DoubleTimeSeries.PeriodCount) ||
            e.PropertyName ==
                nameof(DoubleTimeSeries.Values))
        {
            OnPropertyChanged(nameof(PlanningHorizon));
            OnPropertyChanged(nameof(HasValidQuantities));
        }
    }

    /// <summary>
    /// Validates that a demand quantity is finite and non-negative.
    /// </summary>
    /// <param name="quantity">The quantity to validate.</param>
    /// <param name="parameterName">The parameter name for error messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the quantity is NaN, infinite, or negative.
    /// </exception>
    private static void ValidateQuantity(
        double quantity,
        string parameterName)
    {
        // Check if the quantity is finite (not NaN or Infinity)
        if (double.IsNaN(quantity) ||
            double.IsInfinity(quantity))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                quantity,
                "A demand quantity must be a finite number.");
        }

        // Check if the quantity is non-negative
        if (quantity < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                quantity,
                "A demand quantity cannot be negative.");
        }
    }
}