using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents quantities from production or purchase orders
/// already in progress at the beginning of the planning horizon
/// and expected to be received during that horizon.
///
/// Corresponds to the UML class "Reception Prevue".
/// Its values correspond to QEnc[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "scheduledReceipt")]
public sealed class ScheduledReceipt :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty scheduled-receipt parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public ScheduledReceipt()
    {
    }

    /// <summary>
    /// Initializes scheduled receipts for the specified
    /// planning horizon.
    ///
    /// Every period is initially assigned a receipt quantity
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public ScheduledReceipt(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero scheduled receipts
    {
    }

    /// <summary>
    /// Initializes scheduled receipts for the specified
    /// planning horizon and assigns the same quantity
    /// to every period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultReceiptQuantity">
    /// Receipt quantity initially assigned to every period.
    /// </param>
    public ScheduledReceipt(
        int planningHorizon,
        double defaultReceiptQuantity)
        : base(
            planningHorizon,
            defaultReceiptQuantity)  // Initialize all periods with the specified receipt quantity
    {
    }

    /// <summary>
    /// Gets the complete scheduled-receipt time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries ReceiptQuantities =>
        Values;

    /// <summary>
    /// Gets the quantity expected to be received
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetReceiptQuantity(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the quantity expected to be received
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="quantity">
    /// Non-negative finite receipt quantity.
    /// </param>
    public void SetReceiptQuantity(
        int period,
        double quantity)
    {
        SetValue(period, quantity);
    }

    /// <summary>
    /// Validates a scheduled-receipt quantity.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the scheduled receipt quantity is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A scheduled-receipt quantity cannot be negative.");
        }
    }
}