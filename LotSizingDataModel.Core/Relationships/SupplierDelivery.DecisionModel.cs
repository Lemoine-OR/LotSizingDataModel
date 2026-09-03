using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Adds the optional purchase-price parameter associated
/// with the delivery of an item from a supplier to a warehouse.
/// </summary>
public sealed partial class SupplierDelivery :
    IPlanningHorizonAware
{
    private PurchasePrice? _purchasePrice;

    private CapacityConstraint? _capacityConstraint;
    /// <summary>
    /// Gets or sets the unit purchase price of the item
    /// for each planning period.
    ///
    /// A null value means that no purchase price is defined
    /// for this supplier-item-warehouse relationship.
    /// </summary>
    [XmlElement("purchasePrice")]
    public PurchasePrice? PurchasePrice
    {
        get => _purchasePrice;
        set
        {
            if (ReferenceEquals(_purchasePrice, value))
            {
                return;
            }

            // Unsubscribe from old parameter.
            if (_purchasePrice is not null)
            {
                _purchasePrice.PropertyChanged -=
                    OnPurchasePricePropertyChanged;
            }

            _purchasePrice = value;

            // Subscribe to new parameter.
            if (_purchasePrice is not null)
            {
                _purchasePrice.PropertyChanged +=
                    OnPurchasePricePropertyChanged;
            }

            // Notify the specific property plus all dependent ones.
            OnPropertyChanged(nameof(PurchasePrice));
            OnPropertyChanged(nameof(PlanningHorizon));
            OnPropertyChanged(nameof(HasDecisionParameters));
        }
    }

    /// <summary>
    /// Gets the planning horizon represented by the
    /// purchase-price time series.
    ///
    /// Returns zero when no purchase price is defined.
    /// </summary>
    /// <summary>
    /// Gets or sets the maximum procurement quantity available
    /// from this supplier for this item and destination.
    /// </summary>
    [XmlElement("capacityConstraint")]
    public CapacityConstraint? CapacityConstraint
    {
        get => _capacityConstraint;
        set
        {
            if (ReferenceEquals(_capacityConstraint, value))
            {
                return;
            }

            if (_capacityConstraint is not null)
            {
                _capacityConstraint.PropertyChanged -=
                    OnCapacityConstraintPropertyChanged;
            }

            _capacityConstraint = value;

            if (_capacityConstraint is not null)
            {
                _capacityConstraint.PropertyChanged +=
                    OnCapacityConstraintPropertyChanged;
            }

            OnPropertyChanged(nameof(CapacityConstraint));
            OnPropertyChanged(nameof(PlanningHorizon));
            OnPropertyChanged(nameof(HasDecisionParameters));
            OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
        }
    }

    [XmlIgnore]
    public int PlanningHorizon =>
        PurchasePrice?.PlanningHorizon ?? CapacityConstraint?.PlanningHorizon ??
        0;

    /// <summary>
    /// Gets a value indicating whether this delivery
    /// contains at least one decision-model parameter.
    /// </summary>
    [XmlIgnore]
    public bool HasDecisionParameters =>
        PurchasePrice is not null ||
        CapacityConstraint is not null;

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        PurchasePrice is null ||
        CapacityConstraint is null ||
        PurchasePrice.PlanningHorizon ==
            CapacityConstraint.PlanningHorizon;

    /// <summary>
    /// Resizes the purchase-price time series.
    ///
    /// Existing values are preserved and newly created
    /// periods are initialized according to PurchasePrice.
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

        // Resize the purchase price if present.
        PurchasePrice?.ResizeTimeSeries(periodCount);

        CapacityConstraint?.ResizeTimeSeries(periodCount);
        // Notify dependent computed property.
        OnPropertyChanged(nameof(PlanningHorizon));
    }

    /// <summary>
    /// Removes the purchase-price parameter from
    /// this supplier delivery.
    /// </summary>
    public void ClearDecisionParameters()
    {
        PurchasePrice = null;
        CapacityConstraint = null;
    }

    private void OnCapacityConstraintPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CapacityConstraint));
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    private void OnPurchasePricePropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Forward notification and update dependent computed property.
        OnPropertyChanged(nameof(PurchasePrice));
        OnPropertyChanged(nameof(PlanningHorizon));
    }
}