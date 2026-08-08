using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit purchase price of an item bought
/// from a supplier during each planning period.
///
/// In the complete model, the purchase price depends on:
/// - the supplier;
/// - the destination warehouse;
/// - the item;
/// - the planning period.
///
/// Corresponds to the UML class "Prix d'achat".
/// Its values correspond to PA[t] in the knowledge model.
/// </summary>
[Serializable]
[XmlType(TypeName = "purchasePrice")]
public sealed class PurchasePrice :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty purchase-price parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public PurchasePrice()
    {
    }

    /// <summary>
    /// Initializes a purchase-price parameter for the specified
    /// planning horizon.
    ///
    /// Every planning period is initially assigned
    /// a purchase price of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public PurchasePrice(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero purchase price
    {
    }

    /// <summary>
    /// Initializes a purchase-price parameter and assigns
    /// the same unit price to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitPurchasePrice">
    /// Unit purchase price initially assigned to every period.
    /// </param>
    public PurchasePrice(
        int planningHorizon,
        double defaultUnitPurchasePrice)
        : base(
            planningHorizon,
            defaultUnitPurchasePrice)  // Initialize all periods with the specified unit purchase price
    {
    }

    /// <summary>
    /// Gets the complete unit-purchase-price time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitPurchasePriceByPeriod =>
        Values;

    /// <summary>
    /// Gets the unit purchase price for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitPurchasePrice(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the unit purchase price for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="price">
    /// Non-negative finite unit purchase price.
    /// </param>
    public void SetUnitPurchasePrice(
        int period,
        double price)
    {
        SetValue(period, price);
    }

    /// <summary>
    /// Validates a unit purchase price.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the unit purchase price is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A unit purchase price cannot be negative.");
        }
    }
}