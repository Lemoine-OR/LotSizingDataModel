using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the cost incurred when a new sequence of production setups
/// starts during a planning period.
/// </summary>
/// <remarks>
/// This is intentionally distinct from <see cref="FixedSetupCost"/>.
/// A setup cost is associated with activating production for an item in a
/// period. A start-up cost is associated with the beginning of a sequence of
/// setups, i.e. with a transition into an active setup sequence.
///
/// The parameter provides factual model semantics only. The standard
/// formulation does not yet contain the transition variable required to
/// enforce this cost.
/// </remarks>
[Serializable]
[XmlType(TypeName = "startUpCost")]
public sealed class StartUpCost : DoubleTimeSeriesParameter
{
    public StartUpCost()
    {
    }

    public StartUpCost(int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public StartUpCost(
        int planningHorizon,
        double defaultStartUpCost)
        : base(planningHorizon, defaultStartUpCost)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries StartUpCostByPeriod =>
        Values;

    public double GetStartUpCost(int period) =>
        GetValue(period);

    public void SetStartUpCost(
        int period,
        double cost) =>
            SetValue(period, cost);

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
                "A start-up cost cannot be negative.");
        }
    }
}
