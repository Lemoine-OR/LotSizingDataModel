using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Number of scheduling micro-periods contained in each planning macro-period.
/// </summary>
[Serializable]
[XmlType(TypeName = "microPeriodCount")]
public sealed class MicroPeriodCount :
    IntegerTimeSeriesParameter
{
    public MicroPeriodCount()
    {
    }

    public MicroPeriodCount(int planningHorizon)
        : base(planningHorizon, defaultValue: 1)
    {
    }

    public MicroPeriodCount(
        int planningHorizon,
        int defaultMicroPeriodCount)
        : base(
            planningHorizon,
            defaultMicroPeriodCount)
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

    [XmlIgnore]
    protected override int DefaultValueForNewPeriods =>
        1;

    protected override void ValidateValue(
        int value,
        string parameterName)
    {
        base.ValidateValue(value, parameterName);

        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A micro-period count must be strictly positive.");
        }
    }
}
