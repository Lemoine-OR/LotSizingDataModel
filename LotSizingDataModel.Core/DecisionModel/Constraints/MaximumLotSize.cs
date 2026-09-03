using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the maximum production-lot size for each planning period.
/// </summary>
/// <remarks>
/// Null at routing level means "not represented". A represented value of zero
/// is active and forbids production in the corresponding period.
/// </remarks>
[Serializable]
[XmlType(TypeName = "maximumLotSize")]
public sealed class MaximumLotSize :
    DoubleTimeSeriesParameter
{
    public MaximumLotSize()
    {
    }

    public MaximumLotSize(int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public MaximumLotSize(
        int planningHorizon,
        double defaultMaximumLotSize)
        : base(
            planningHorizon,
            defaultMaximumLotSize)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries MaximumLotSizeByPeriod =>
        Values;

    public double GetMaximumLotSize(int period) =>
        GetValue(period);

    public void SetMaximumLotSize(
        int period,
        double maximumLotSize) =>
            SetValue(period, maximumLotSize);

    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        base.ValidateValue(value, parameterName);

        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum lot size cannot be negative.");
        }
    }
}
