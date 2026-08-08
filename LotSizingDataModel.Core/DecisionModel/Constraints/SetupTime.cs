using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the capacity consumed to prepare a resource
/// for manufacturing, storing or transporting an item.
///
/// Corresponds to the UML class "Temps de preparation".
/// Its values correspond to CSetup[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "setupTime")]
public sealed class SetupTime : DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty setup-time parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public SetupTime()
    {
    }

    /// <summary>
    /// Initializes a setup-time parameter for the specified
    /// planning horizon.
    ///
    /// Every planning period is initially assigned
    /// a setup time of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public SetupTime(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero setup time
    {
    }

    /// <summary>
    /// Initializes a setup-time parameter for the specified
    /// planning horizon and assigns the same setup time
    /// to every period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultSetupTime">
    /// Setup time initially assigned to every planning period.
    /// </param>
    public SetupTime(
        int planningHorizon,
        double defaultSetupTime)
        : base(
            planningHorizon,
            defaultSetupTime)  // Initialize all periods with the specified setup time
    {
    }

    /// <summary>
    /// Gets the complete setup-time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries SetupTimeByPeriod => Values;

    /// <summary>
    /// Gets the setup time for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetSetupTime(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the setup time for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="setupTime">
    /// Non-negative finite setup time.
    /// </param>
    public void SetSetupTime(
        int period,
        double setupTime)
    {
        SetValue(period, setupTime);
    }

    /// <summary>
    /// Validates a setup-time value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the setup time is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A setup time cannot be negative.");
        }
    }
}