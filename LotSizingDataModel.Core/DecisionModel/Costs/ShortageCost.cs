using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit cost incurred for each item unit
/// permanently lost as shortage during a planning period.
///
/// Unlike backlog, shortage demand is abandoned and will not
/// be fulfilled during a later planning period.
///
/// Corresponds to the UML class "Cout Shortage".
/// Its values correspond to CShort[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "shortageCost")]
public sealed class ShortageCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty shortage-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public ShortageCost()
    {
    }

    /// <summary>
    /// Initializes a shortage-cost parameter for the specified
    /// planning horizon.
    ///
    /// Every planning period is initially assigned a unit
    /// shortage cost of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public ShortageCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero unit shortage cost
    {
    }

    /// <summary>
    /// Initializes a shortage-cost parameter and assigns
    /// the same unit cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitShortageCost">
    /// Unit shortage cost initially assigned to every period.
    /// </param>
    public ShortageCost(
        int planningHorizon,
        double defaultUnitShortageCost)
        : base(
            planningHorizon,
            defaultUnitShortageCost)  // Initialize all periods with the specified unit shortage cost
    {
    }

    /// <summary>
    /// Gets the complete unit-shortage-cost time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitShortageCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the cost of permanently losing one item unit
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitShortageCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the cost of permanently losing one item unit
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="cost">
    /// Non-negative finite cost per shortage unit.
    /// </param>
    public void SetUnitShortageCost(
        int period,
        double cost)
    {
        SetValue(period, cost);
    }

    /// <summary>
    /// Validates a unit shortage cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the unit shortage cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A unit shortage cost cannot be negative.");
        }
    }
}