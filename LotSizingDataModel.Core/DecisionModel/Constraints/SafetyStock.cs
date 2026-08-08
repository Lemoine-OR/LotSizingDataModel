using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the required safety-stock level for each
/// planning period.
///
/// Corresponds to the UML class "Stock Securite".
/// Its values correspond to Isecu[t] in the source model.
///
/// When associated with an inventory, these values define
/// the stock levels below which the inventory should not fall.
/// </summary>
[Serializable]
[XmlType(TypeName = "safetyStock")]
public sealed class SafetyStock :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty safety-stock constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public SafetyStock()
    {
    }

    /// <summary>
    /// Initializes a safety-stock constraint for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a safety-stock level
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public SafetyStock(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero safety stock
    {
    }

    /// <summary>
    /// Initializes a safety-stock constraint and assigns
    /// the same required level to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultSafetyStockLevel">
    /// Safety-stock level initially assigned to every period.
    /// </param>
    public SafetyStock(
        int planningHorizon,
        double defaultSafetyStockLevel)
        : base(
            planningHorizon,
            defaultSafetyStockLevel)  // Initialize all periods with the specified safety stock level
    {
    }

    /// <summary>
    /// Gets the complete safety-stock time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries RequiredLevels =>
        Values;

    /// <summary>
    /// Gets the required safety-stock level for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetRequiredLevel(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the required safety-stock level for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="level">
    /// Non-negative finite safety-stock level.
    /// </param>
    public void SetRequiredLevel(
        int period,
        double level)
    {
        SetValue(period, level);
    }

    /// <summary>
    /// Validates a safety-stock level.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the safety stock level is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A safety-stock level cannot be negative.");
        }
    }
}