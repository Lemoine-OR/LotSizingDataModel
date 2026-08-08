using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the minimum production-lot size for each
/// planning period.
///
/// Corresponds to the UML class "Lot Minimum".
/// Its values correspond to Qmin[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "minimumLotSize")]
public sealed class MinimumLotSize :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty minimum-lot-size constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public MinimumLotSize()
    {
    }

    /// <summary>
    /// Initializes a minimum-lot-size constraint for the
    /// specified planning horizon.
    ///
    /// Every period is initially assigned a minimum lot size
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public MinimumLotSize(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero minimum lot size
    {
    }

    /// <summary>
    /// Initializes a minimum-lot-size constraint for the
    /// specified planning horizon and assigns the same value
    /// to every period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultMinimumLotSize">
    /// Minimum lot size initially assigned to every period.
    /// </param>
    public MinimumLotSize(
        int planningHorizon,
        double defaultMinimumLotSize)
        : base(
            planningHorizon,
            defaultMinimumLotSize)  // Initialize all periods with the specified minimum lot size
    {
    }

    /// <summary>
    /// Gets the complete minimum-lot-size time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries MinimumLotSizeByPeriod =>
        Values;

    /// <summary>
    /// Gets the minimum lot size for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetMinimumLotSize(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the minimum lot size for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="minimumLotSize">
    /// Non-negative finite minimum lot size.
    /// </param>
    public void SetMinimumLotSize(
        int period,
        double minimumLotSize)
    {
        SetValue(period, minimumLotSize);
    }

    /// <summary>
    /// Validates a minimum-lot-size value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the minimum lot size is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A minimum lot size cannot be negative.");
        }
    }
}