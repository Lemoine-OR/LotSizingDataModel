using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Maximum number of setup transitions authorized for a work center in each
/// planning period.
/// </summary>
[Serializable]
[XmlType(TypeName = "maximumSetupCount")]
public sealed class MaximumSetupCount :
    IntegerTimeSeriesParameter
{
    public MaximumSetupCount()
    {
    }

    public MaximumSetupCount(int planningHorizon)
        : base(planningHorizon, defaultValue: 0)
    {
    }

    public MaximumSetupCount(
        int planningHorizon,
        int defaultMaximumSetupCount)
        : base(
            planningHorizon,
            defaultMaximumSetupCount)
    {
    }

    [XmlIgnore]
    public IntegerTimeSeries CountByPeriod =>
        Values;

    public int GetCount(int period) =>
        GetValue(period);

    public void SetCount(
        int period,
        int count) =>
            SetValue(period, count);

    protected override void ValidateValue(
        int value,
        string parameterName)
    {
        base.ValidateValue(value, parameterName);

        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum setup count cannot be negative.");
        }
    }
}
