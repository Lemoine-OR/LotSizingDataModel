using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the standard lot size of a production routing
/// for each planning period.
///
/// When this constraint is active, the produced quantity must
/// be an integer multiple of the corresponding standard lot size.
///
/// Corresponds to the UML class "Lot Multiple".
/// Its values correspond to Qmult[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "lotSizeMultiple")]
public sealed class LotSizeMultiple :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty lot-size-multiple constraint.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public LotSizeMultiple()
    {
    }

    /// <summary>
    /// Initializes a lot-size-multiple constraint for the
    /// specified planning horizon.
    ///
    /// Every period is initially assigned a standard lot size
    /// of one unit.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public LotSizeMultiple(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 1.0)  // Initialize all periods with a lot size multiple of one
    {
    }

    /// <summary>
    /// Initializes a lot-size-multiple constraint for the
    /// specified planning horizon and assigns the same standard
    /// lot size to every period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultLotSizeMultiple">
    /// Strictly positive standard lot size initially assigned
    /// to every period.
    /// </param>
    public LotSizeMultiple(
        int planningHorizon,
        double defaultLotSizeMultiple)
        : base(
            planningHorizon,
            defaultLotSizeMultiple)  // Initialize all periods with the specified lot size multiple
    {
    }

    /// <summary>
    /// Gets the complete standard-lot-size time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries LotSizeMultipleByPeriod =>
        Values;

    /// <summary>
    /// Gets the standard lot size for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetLotSizeMultiple(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the standard lot size for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="lotSizeMultiple">
    /// Strictly positive finite standard lot size.
    /// </param>
    public void SetLotSizeMultiple(
        int period,
        double lotSizeMultiple)
    {
        SetValue(period, lotSizeMultiple);
    }

    /// <summary>
    /// Gets the default value assigned to periods created
    /// when the planning horizon grows.
    ///
    /// A value of one preserves a valid multiple constraint.
    /// </summary>
    [XmlIgnore]
    protected override double DefaultValueForNewPeriods =>
        1.0;

    /// <summary>
    /// Validates a standard-lot-size value.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the lot size multiple is strictly positive
        if (value <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A lot-size multiple must be strictly positive.");
        }
    }
}