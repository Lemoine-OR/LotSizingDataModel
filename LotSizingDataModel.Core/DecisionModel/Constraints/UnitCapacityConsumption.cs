using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the amount of capacity consumed by one unit
/// of an item during each planning period.
///
/// It can describe the capacity required to manufacture,
/// store or transport one unit of an item.
///
/// Corresponds to the UML class "Capacite unitaire".
/// Its values correspond to CUnit[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "unitCapacityConsumption")]
public sealed class UnitCapacityConsumption :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty unit-capacity-consumption parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public UnitCapacityConsumption()
    {
    }

    /// <summary>
    /// Initializes the parameter for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a unit-capacity
    /// consumption of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public UnitCapacityConsumption(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero unit consumption
    {
    }

    /// <summary>
    /// Initializes the parameter for the specified planning horizon
    /// and assigns the same unit-capacity consumption to every period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultConsumption">
    /// Unit-capacity consumption initially assigned
    /// to every planning period.
    /// </param>
    public UnitCapacityConsumption(
        int planningHorizon,
        double defaultConsumption)
        : base(
            planningHorizon,
            defaultConsumption)  // Initialize all periods with the specified unit consumption
    {
    }

    /// <summary>
    /// Gets the complete unit-capacity-consumption time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries ConsumptionPerUnit =>
        Values;

    /// <summary>
    /// Gets the capacity consumed by one unit of the item
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetConsumption(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the capacity consumed by one unit of the item
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="consumption">
    /// Non-negative finite capacity consumption.
    /// </param>
    public void SetConsumption(
        int period,
        double consumption)
    {
        SetValue(period, consumption);
    }

    /// <summary>
    /// Validates a unit-capacity-consumption value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the unit consumption is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A unit-capacity consumption cannot be negative.");
        }
    }
}