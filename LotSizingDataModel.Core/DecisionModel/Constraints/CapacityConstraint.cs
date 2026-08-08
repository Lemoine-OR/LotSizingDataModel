using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents a maximum-capacity constraint defined for each
/// planning period.
///
/// Corresponds to the UML class "Capacite".
/// Its values correspond to CMax[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "capacityConstraint")]
public sealed class CapacityConstraint :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty capacity constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public CapacityConstraint()
    {
    }

    /// <summary>
    /// Initializes a capacity constraint for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a maximum capacity
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public CapacityConstraint(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero capacity
    {
    }

    /// <summary>
    /// Initializes a capacity constraint for the specified
    /// planning horizon and initial maximum capacity.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultMaximumCapacity">
    /// Maximum capacity initially assigned to every period.
    /// </param>
    public CapacityConstraint(
        int planningHorizon,
        double defaultMaximumCapacity)
        : base(
            planningHorizon,
            defaultMaximumCapacity)  // Initialize all periods with the specified capacity
    {
    }

    /// <summary>
    /// Gets the complete maximum-capacity time series.
    ///
    /// This is a convenience alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>.
    /// It is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries MaximumCapacity =>
        Values;

    /// <summary>
    /// Gets the maximum capacity available during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetMaximumCapacity(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the maximum capacity available during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="capacity">
    /// Non-negative finite maximum capacity.
    /// </param>
    public void SetMaximumCapacity(
        int period,
        double capacity)
    {
        SetValue(period, capacity);
    }

    /// <summary>
    /// Validates a maximum-capacity value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the capacity is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum capacity cannot be negative.");
        }
    }
}