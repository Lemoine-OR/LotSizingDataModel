using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the fixed cost incurred when a resource is used
/// during a planning period.
///
/// This cost does not depend on the quantity processed by the
/// resource. It is incurred once when the resource is activated
/// during the corresponding period.
///
/// Corresponds to the UML class "Cout fixe d'utilisation".
/// Its values correspond to CFU[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "fixedUsageCost")]
public sealed class FixedUsageCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty fixed-usage-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public FixedUsageCost()
    {
    }

    /// <summary>
    /// Initializes a fixed-usage-cost parameter for the
    /// specified planning horizon.
    ///
    /// Every planning period is initially assigned a cost
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public FixedUsageCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero fixed usage cost
    {
    }

    /// <summary>
    /// Initializes a fixed-usage-cost parameter and assigns
    /// the same cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultFixedUsageCost">
    /// Fixed usage cost initially assigned to every period.
    /// </param>
    public FixedUsageCost(
        int planningHorizon,
        double defaultFixedUsageCost)
        : base(
            planningHorizon,
            defaultFixedUsageCost)  // Initialize all periods with the specified fixed usage cost
    {
    }

    /// <summary>
    /// Gets the complete fixed-usage-cost time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries FixedCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the fixed usage cost for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetFixedUsageCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the fixed usage cost for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="cost">
    /// Non-negative finite fixed usage cost.
    /// </param>
    public void SetFixedUsageCost(
        int period,
        double cost)
    {
        SetValue(period, cost);
    }

    /// <summary>
    /// Validates a fixed usage cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the fixed usage cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A fixed usage cost cannot be negative.");
        }
    }
}