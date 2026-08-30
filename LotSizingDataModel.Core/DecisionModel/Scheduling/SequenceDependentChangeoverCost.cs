using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Cost incurred by a changeover from one item setup state to another.
/// </summary>
[Serializable]
[XmlType(TypeName = "sequenceDependentChangeoverCost")]
public sealed class SequenceDependentChangeoverCost :
    DoubleTimeSeriesParameter
{
    public SequenceDependentChangeoverCost()
    {
    }

    public SequenceDependentChangeoverCost(
        int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public SequenceDependentChangeoverCost(
        int planningHorizon,
        double defaultValue)
        : base(planningHorizon, defaultValue)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries CostByPeriod =>
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
                "A sequence-dependent changeover cost cannot be negative.");
        }
    }
}
