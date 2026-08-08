using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the purchase decisions associated with one supplier,
/// one item and one destination warehouse over the complete
/// planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// Supplier capacities, purchase costs and delivery lead times
/// belong to the supply-chain instance and are not duplicated
/// in this solution object.
/// </remarks>
[Serializable]
[XmlType(TypeName = "purchaseDecision")]
public sealed class PurchaseDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _supplierId;
    private int _itemId;

    private WarehouseReference _destinationWarehouse =
        new();

    private DoubleTimeSeries _purchasedQuantities =
        new();

    /// <summary>
    /// Initializes an empty purchase decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public PurchaseDecision()
    {
        SubscribeToObject(_destinationWarehouse);
        SubscribeToObject(_purchasedQuantities);
    }

    /// <summary>
    /// Initializes a purchase decision for a supplier,
    /// an item, a destination warehouse and a planning horizon.
    /// </summary>
    /// <param name="supplierId">
    /// Identifier of the supplier.
    /// </param>
    /// <param name="itemId">
    /// Identifier of the purchased item.
    /// </param>
    /// <param name="destinationWarehouse">
    /// Warehouse to which the purchased item is delivered.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public PurchaseDecision(
        int supplierId,
        int itemId,
        WarehouseReference destinationWarehouse,
        int planningHorizon)
        : this()
    {
        if (supplierId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(supplierId),
                supplierId,
                "The supplier identifier must be strictly positive.");
        }

        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemId),
                itemId,
                "The item identifier must be strictly positive.");
        }

        ArgumentNullException.ThrowIfNull(
            destinationWarehouse);

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        SupplierId = supplierId;
        ItemId = itemId;

        DestinationWarehouse =
            destinationWarehouse;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the supplier
    /// associated with the purchase decision.
    /// </summary>
    [XmlAttribute("supplierId")]
    public int SupplierId
    {
        get => _supplierId;
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
                    "The supplier identifier cannot be negative.");
            }

            SetProperty(
                ref _supplierId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the purchased item.
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
    /// Gets or sets the warehouse to which the purchased
    /// item is delivered.
    /// </summary>
    [XmlElement("destinationWarehouse")]
    public WarehouseReference DestinationWarehouse
    {
        get => _destinationWarehouse;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _destinationWarehouse,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _destinationWarehouse);

            SetProperty(
                ref _destinationWarehouse,
                value);

            SubscribeToObject(
                _destinationWarehouse);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the quantities purchased during
    /// each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// The period represents the purchase-decision period.
    /// The corresponding receipt period may differ when
    /// a positive supplier lead time is defined in the instance.
    /// </remarks>
    [XmlElement("purchasedQuantities")]
    public DoubleTimeSeries PurchasedQuantities
    {
        get => _purchasedQuantities;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _purchasedQuantities,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _purchasedQuantities);

            SetProperty(
                ref _purchasedQuantities,
                value);

            SubscribeToObject(
                _purchasedQuantities);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by the purchased-quantity series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        PurchasedQuantities.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether every purchased
    /// quantity is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidPurchasedQuantities =>
        PurchasedQuantities.All(
            quantity =>
                double.IsFinite(quantity) &&
                quantity >= 0.0);

    /// <summary>
    /// Gets a value indicating whether the destination
    /// warehouse reference is initialized.
    /// </summary>
    [XmlIgnore]
    public bool HasValidDestinationWarehouse =>
        DestinationWarehouse.ReferenceId > 0;

    /// <summary>
    /// Gets a value indicating whether the purchase decision
    /// is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the supplier,
    /// item or warehouse exists in a particular
    /// supply-chain instance.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        SupplierId > 0 &&
        ItemId > 0 &&
        PlanningHorizon > 0 &&
        HasValidDestinationWarehouse &&
        HasValidPurchasedQuantities;

    /// <summary>
    /// Gets the purchased quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Purchased quantity recorded for the period.
    /// </returns>
    public double GetPurchasedQuantity(int period)
    {
        return PurchasedQuantities[period];
    }

    /// <summary>
    /// Sets the purchased quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="quantity">
    /// Finite and non-negative purchased quantity.
    /// </param>
    public void SetPurchasedQuantity(
        int period,
        double quantity)
    {
        ValidateNonNegativeFiniteValue(
            quantity,
            nameof(quantity));

        PurchasedQuantities[period] =
            quantity;
    }

    /// <summary>
    /// Determines whether this decision identifies the
    /// specified supplier, item and destination warehouse.
    /// </summary>
    /// <param name="supplierId">
    /// Supplier identifier to compare.
    /// </param>
    /// <param name="itemId">
    /// Item identifier to compare.
    /// </param>
    /// <param name="destinationWarehouse">
    /// Destination warehouse to compare.
    /// </param>
    /// <returns>
    /// True when all key elements match; otherwise, false.
    /// </returns>
    public bool Matches(
        int supplierId,
        int itemId,
        WarehouseReference destinationWarehouse)
    {
        ArgumentNullException.ThrowIfNull(
            destinationWarehouse);

        return SupplierId == supplierId &&
               ItemId == itemId &&
               SameWarehouse(
                   DestinationWarehouse,
                   destinationWarehouse);
    }

    /// <summary>
    /// Resizes the purchased-quantity series to the specified
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

        PurchasedQuantities.Resize(
            periodCount,
            defaultValue: 0.0);

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Resets every purchased quantity to zero.
    /// </summary>
    public void Clear()
    {
        PurchasedQuantities.Fill(0.0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        double totalPurchasedQuantity =
            PurchasedQuantities.Sum();

        return
            $"Supplier {SupplierId}, item {ItemId}, " +
            $"destination " +
            $"{FormatWarehouse(DestinationWarehouse)}: " +
            $"total purchased quantity " +
            $"{totalPurchasedQuantity}";
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
            nameof(HasValidPurchasedQuantities));

        OnPropertyChanged(
            nameof(HasValidDestinationWarehouse));

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