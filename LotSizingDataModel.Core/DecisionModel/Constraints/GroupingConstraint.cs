using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents a production-flow grouping rule defined
/// for each planning period.
///
/// When this constraint is active, its value indicates the number
/// of consecutive periods covered by the grouping window.
///
/// Corresponds to the UML class "Regroupement".
/// Its values correspond to Regroup[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "groupingConstraint")]
public sealed class GroupingConstraint :
    IntegerTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty grouping constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public GroupingConstraint()
    {
    }

    /// <summary>
    /// Initializes a grouping constraint for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a grouping window
    /// of one period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public GroupingConstraint(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 1)  // Initialize all periods with a grouping window of one period
    {
    }

    /// <summary>
    /// Initializes a grouping constraint and assigns the same
    /// grouping-window length to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultGroupingPeriodCount">
    /// Strictly positive number of periods initially assigned
    /// to every planning period.
    /// </param>
    public GroupingConstraint(
        int planningHorizon,
        int defaultGroupingPeriodCount)
        : base(
            planningHorizon,
            defaultGroupingPeriodCount)  // Initialize all periods with the specified grouping window
    {
    }

    /// <summary>
    /// Gets the complete grouping-window time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.IntegerTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public IntegerTimeSeries GroupingPeriodCountByPeriod =>
        Values;

    /// <summary>
    /// Gets the grouping-window length for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <returns>
    /// Strictly positive number of periods in the grouping window.
    /// </returns>
    public int GetGroupingPeriodCount(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the grouping-window length for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="groupingPeriodCount">
    /// Strictly positive number of periods in the grouping window.
    /// </param>
    public void SetGroupingPeriodCount(
        int period,
        int groupingPeriodCount)
    {
        SetValue(period, groupingPeriodCount);
    }

    /// <summary>
    /// Gets the value assigned to periods created when
    /// the planning horizon grows.
    ///
    /// A value of one preserves a valid grouping constraint.
    /// </summary>
    [XmlIgnore]
    protected override int DefaultValueForNewPeriods => 1;

    /// <summary>
    /// Validates a grouping-window length.
    /// </summary>
    protected override void ValidateValue(
        int value,
        string parameterName)
    {
        // Call base class validation (no-op for integer parameters)
        base.ValidateValue(value, parameterName);

        // Validate that the grouping period count is strictly positive
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A grouping-period count must be strictly positive.");
        }
    }
}