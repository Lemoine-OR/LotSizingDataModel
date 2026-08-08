using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit selling price of an item supplied
/// to a distribution center during each planning period.
///
/// In the complete model, the selling price depends on:
/// - the distribution center;
/// - the supplying warehouse;
/// - the item;
/// - the planning period.
///
/// Corresponds to the UML class "Prix Vente".
/// Its values correspond to PV[t] in the knowledge model.
/// </summary>
[Serializable]
[XmlType(TypeName = "sellingPrice")]
public sealed class SellingPrice :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty selling-price parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public SellingPrice()
    {
    }

    /// <summary>
    /// Initializes a selling-price parameter for the specified
    /// planning horizon.
    ///
    /// Every planning period is initially assigned
    /// a selling price of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public SellingPrice(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero selling price
    {
    }

    /// <summary>
    /// Initializes a selling-price parameter and assigns
    /// the same unit price to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitSellingPrice">
    /// Unit selling price initially assigned to every period.
    /// </param>
    public SellingPrice(
        int planningHorizon,
        double defaultUnitSellingPrice)
        : base(
            planningHorizon,
            defaultUnitSellingPrice)  // Initialize all periods with the specified unit selling price
    {
    }

    /// <summary>
    /// Gets the complete unit-selling-price time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitSellingPriceByPeriod =>
        Values;

    /// <summary>
    /// Gets the unit selling price for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitSellingPrice(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the unit selling price for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="price">
    /// Non-negative finite unit selling price.
    /// </param>
    public void SetUnitSellingPrice(
        int period,
        double price)
    {
        SetValue(period, price);
    }

    /// <summary>
    /// Validates a unit selling price.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the unit selling price is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A unit selling price cannot be negative.");
        }
    }
}