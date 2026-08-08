using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the maximum backlog level allowed for each
/// planning period.
///
/// Backlog corresponds to demand that is not satisfied during
/// its original period but remains to be fulfilled later.
///
/// Corresponds to the UML class "Backlog".
/// Its values correspond to Blmax[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "backlogConstraint")]
public sealed class BacklogConstraint :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty backlog constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public BacklogConstraint()
    {
    }

    /// <summary>
    /// Initializes a backlog constraint for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a maximum backlog
    /// level of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public BacklogConstraint(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero maximum backlog
    {
    }

    /// <summary>
    /// Initializes a backlog constraint and assigns the same
    /// maximum backlog level to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultMaximumBacklog">
    /// Maximum backlog initially allowed in every period.
    /// </param>
    public BacklogConstraint(
        int planningHorizon,
        double defaultMaximumBacklog)
        : base(
            planningHorizon,
            defaultMaximumBacklog)  // Initialize all periods with the specified maximum backlog
    {
    }

    /// <summary>
    /// Gets the complete maximum-backlog time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries MaximumBacklogByPeriod =>
        Values;

    /// <summary>
    /// Gets the maximum backlog allowed during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetMaximumBacklog(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the maximum backlog allowed during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="maximumBacklog">
    /// Non-negative finite maximum backlog level.
    /// </param>
    public void SetMaximumBacklog(
        int period,
        double maximumBacklog)
    {
        SetValue(period, maximumBacklog);
    }

    /// <summary>
    /// Validates a maximum-backlog value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the maximum backlog level is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum backlog level cannot be negative.");
        }
    }
}