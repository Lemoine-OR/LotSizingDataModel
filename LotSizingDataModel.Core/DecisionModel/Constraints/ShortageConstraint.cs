using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the maximum quantity of demand that may be
/// permanently lost during each planning period.
///
/// Unlike backlog, shortage demand is not postponed and will
/// not be fulfilled during a later planning period.
///
/// Corresponds to the UML class "Shortage".
/// Its values correspond to Shmax[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "shortageConstraint")]
public sealed class ShortageConstraint :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty shortage constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public ShortageConstraint()
    {
    }

    /// <summary>
    /// Initializes a shortage constraint for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a maximum shortage
    /// quantity of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public ShortageConstraint(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero maximum shortage
    {
    }

    /// <summary>
    /// Initializes a shortage constraint and assigns the same
    /// maximum shortage quantity to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultMaximumShortage">
    /// Maximum shortage quantity initially allowed
    /// during every planning period.
    /// </param>
    public ShortageConstraint(
        int planningHorizon,
        double defaultMaximumShortage)
        : base(
            planningHorizon,
            defaultMaximumShortage)  // Initialize all periods with the specified maximum shortage
    {
    }

    /// <summary>
    /// Gets the complete maximum-shortage time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries MaximumShortageByPeriod =>
        Values;

    /// <summary>
    /// Gets the maximum shortage quantity allowed
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetMaximumShortage(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the maximum shortage quantity allowed
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="maximumShortage">
    /// Non-negative finite shortage quantity.
    /// </param>
    public void SetMaximumShortage(
        int period,
        double maximumShortage)
    {
        SetValue(period, maximumShortage);
    }

    /// <summary>
    /// Validates a maximum-shortage quantity.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the maximum shortage quantity is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum shortage quantity cannot be negative.");
        }
    }
}