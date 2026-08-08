using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the maximum additional capacity that may be used
/// during each planning period.
///
/// Corresponds to the UML class "Capacite Supplementaire".
/// Its values correspond to CSuppl[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "additionalCapacity")]
public sealed class AdditionalCapacity :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty additional-capacity parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public AdditionalCapacity()
    {
    }

    /// <summary>
    /// Initializes an additional-capacity parameter for
    /// the specified planning horizon.
    ///
    /// Every period is initially assigned an additional
    /// capacity of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public AdditionalCapacity(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero additional capacity
    {
    }

    /// <summary>
    /// Initializes an additional-capacity parameter for
    /// the specified planning horizon.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultMaximumAdditionalCapacity">
    /// Maximum additional capacity initially assigned
    /// to every planning period.
    /// </param>
    public AdditionalCapacity(
        int planningHorizon,
        double defaultMaximumAdditionalCapacity)
        : base(
            planningHorizon,
            defaultMaximumAdditionalCapacity)  // Initialize all periods with the specified additional capacity
    {
    }

    /// <summary>
    /// Gets the complete maximum-additional-capacity time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries MaximumAdditionalCapacity =>
        Values;

    /// <summary>
    /// Gets the maximum additional capacity available
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetMaximumAdditionalCapacity(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the maximum additional capacity available
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="capacity">
    /// Non-negative finite additional capacity.
    /// </param>
    public void SetMaximumAdditionalCapacity(
        int period,
        double capacity)
    {
        SetValue(period, capacity);
    }

    /// <summary>
    /// Validates an additional-capacity value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the additional capacity is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum additional capacity cannot be negative.");
        }
    }
}