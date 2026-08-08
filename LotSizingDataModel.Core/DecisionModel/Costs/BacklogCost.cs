using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit cost incurred for each item unit
/// remaining in backlog during a planning period.
///
/// Backlog corresponds to demand postponed to a later period.
/// The cost may therefore be incurred during every period in
/// which the corresponding demand remains unfulfilled.
///
/// Corresponds to the UML class "Cout Backlog".
/// Its values correspond to CBack[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "backlogCost")]
public sealed class BacklogCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty backlog-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public BacklogCost()
    {
    }

    /// <summary>
    /// Initializes a backlog-cost parameter for the specified
    /// planning horizon.
    ///
    /// Every planning period is initially assigned a unit
    /// backlog cost of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public BacklogCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero unit backlog cost
    {
    }

    /// <summary>
    /// Initializes a backlog-cost parameter and assigns
    /// the same unit cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitBacklogCost">
    /// Unit backlog cost initially assigned to every period.
    /// </param>
    public BacklogCost(
        int planningHorizon,
        double defaultUnitBacklogCost)
        : base(
            planningHorizon,
            defaultUnitBacklogCost)  // Initialize all periods with the specified unit backlog cost
    {
    }

    /// <summary>
    /// Gets the complete unit-backlog-cost time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitBacklogCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the cost of keeping one item unit in backlog
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitBacklogCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the cost of keeping one item unit in backlog
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="cost">
    /// Non-negative finite cost per backlog unit.
    /// </param>
    public void SetUnitBacklogCost(
        int period,
        double cost)
    {
        SetValue(period, cost);
    }

    /// <summary>
    /// Validates a unit backlog cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the unit backlog cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A unit backlog cost cannot be negative.");
        }
    }
}