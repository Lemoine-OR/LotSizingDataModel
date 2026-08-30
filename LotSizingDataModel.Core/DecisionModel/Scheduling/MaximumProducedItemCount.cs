using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Maximum number of distinct items that may be produced during each
/// scheduling bucket.
/// </summary>
[Serializable]
[XmlType(TypeName = "maximumProducedItemCount")]
public sealed class MaximumProducedItemCount :
    IntegerTimeSeriesParameter
{
    public MaximumProducedItemCount()
    {
    }

    public MaximumProducedItemCount(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 1)
    {
    }

    public MaximumProducedItemCount(
        int planningHorizon,
        int defaultMaximumProducedItemCount)
        : base(
            planningHorizon,
            defaultMaximumProducedItemCount)
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

        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A maximum produced-item count cannot be negative.");
        }
    }
}
