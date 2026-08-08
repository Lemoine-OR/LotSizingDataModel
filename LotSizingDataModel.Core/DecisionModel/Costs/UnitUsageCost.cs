using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit cost incurred when a resource is used
/// to manufacture, store or transport one unit of an item
/// during a planning period.
///
/// Corresponds to the UML class "Cout d'utilisation".
/// Its values correspond to Cutil[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "unitUsageCost")]
public sealed class UnitUsageCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty unit-usage-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public UnitUsageCost()
    {
    }

    /// <summary>
    /// Initializes a unit-usage-cost parameter for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a unit cost of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public UnitUsageCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero unit usage cost
    {
    }

    /// <summary>
    /// Initializes a unit-usage-cost parameter and assigns
    /// the same unit cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitUsageCost">
    /// Unit usage cost initially assigned to every period.
    /// </param>
    public UnitUsageCost(
        int planningHorizon,
        double defaultUnitUsageCost)
        : base(
            planningHorizon,
            defaultUnitUsageCost)  // Initialize all periods with the specified unit usage cost
    {
    }

    /// <summary>
    /// Gets the complete unit-usage-cost time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitUsageCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the unit usage cost for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitUsageCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the unit usage cost for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="cost">
    /// Non-negative finite cost per processed item unit.
    /// </param>
    public void SetUnitUsageCost(
        int period,
        double cost)
    {
        SetValue(period, cost);
    }

    /// <summary>
    /// Validates a unit usage cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the unit usage cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A unit usage cost cannot be negative.");
        }
    }
}