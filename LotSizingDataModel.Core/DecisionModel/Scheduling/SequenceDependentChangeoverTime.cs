using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Capacity time consumed by a changeover from one item setup state to another.
/// </summary>
[Serializable]
[XmlType(TypeName = "sequenceDependentChangeoverTime")]
public sealed class SequenceDependentChangeoverTime :
    DoubleTimeSeriesParameter
{
    public SequenceDependentChangeoverTime()
    {
    }

    public SequenceDependentChangeoverTime(
        int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public SequenceDependentChangeoverTime(
        int planningHorizon,
        double defaultValue)
        : base(planningHorizon, defaultValue)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries TimeByPeriod =>
        Values;

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
                "A sequence-dependent changeover time cannot be negative.");
        }
    }
}
