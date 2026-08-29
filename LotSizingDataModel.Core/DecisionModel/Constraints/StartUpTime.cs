using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Represents the production capacity consumed when a new sequence of setups
/// starts during a planning period.
/// </summary>
/// <remarks>
/// This is intentionally distinct from <see cref="SetupTime"/>.
/// Wolsey's start-up-time extension reduces the capacity of the period in
/// which a sequence of setups starts. The standard formulation does not yet
/// contain the transition variable required to enforce this consumption.
/// </remarks>
[Serializable]
[XmlType(TypeName = "startUpTime")]
public sealed class StartUpTime : DoubleTimeSeriesParameter
{
    public StartUpTime()
    {
    }

    public StartUpTime(int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public StartUpTime(
        int planningHorizon,
        double defaultStartUpTime)
        : base(planningHorizon, defaultStartUpTime)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries StartUpTimeByPeriod =>
        Values;

    public double GetStartUpTime(int period) =>
        GetValue(period);

    public void SetStartUpTime(
        int period,
        double startUpTime) =>
            SetValue(period, startUpTime);

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
                "A start-up time cannot be negative.");
        }
    }
}
