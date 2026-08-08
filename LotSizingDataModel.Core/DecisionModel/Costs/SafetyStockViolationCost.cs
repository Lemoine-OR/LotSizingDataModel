using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit cost incurred when the inventory level
/// falls below the required safety-stock level.
///
/// The cost is applied to each unit missing from the safety stock
/// during the corresponding planning period.
///
/// Corresponds to the UML class "Cout stock securite".
/// Its values correspond to CSecu[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "safetyStockViolationCost")]
public sealed class SafetyStockViolationCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty safety-stock-violation-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public SafetyStockViolationCost()
    {
    }

    /// <summary>
    /// Initializes a safety-stock-violation-cost parameter
    /// for the specified planning horizon.
    ///
    /// Every planning period is initially assigned a unit cost
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public SafetyStockViolationCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero violation cost
    {
    }

    /// <summary>
    /// Initializes a safety-stock-violation-cost parameter and
    /// assigns the same unit cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitViolationCost">
    /// Unit cost initially assigned to every planning period.
    /// </param>
    public SafetyStockViolationCost(
        int planningHorizon,
        double defaultUnitViolationCost)
        : base(
            planningHorizon,
            defaultUnitViolationCost)  // Initialize all periods with the specified unit violation cost
    {
    }

    /// <summary>
    /// Gets the complete safety-stock-violation-cost time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitViolationCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the cost incurred for one unit missing
    /// from the safety stock during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitViolationCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the cost incurred for one unit missing
    /// from the safety stock during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="cost">
    /// Non-negative finite cost per missing unit.
    /// </param>
    public void SetUnitViolationCost(
        int period,
        double cost)
    {
        SetValue(period, cost);
    }

    /// <summary>
    /// Validates a safety-stock-violation unit cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the safety-stock-violation cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A safety-stock-violation cost cannot be negative.");
        }
    }
}