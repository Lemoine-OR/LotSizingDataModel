using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Instance.ClosedLoop;

/// <summary>
/// Non-negative period-dependent quantity or unit-cost series
/// used by the closed-loop instance extension.
/// </summary>
[Serializable]
[XmlType(TypeName = "closedLoopTimeSeriesParameter")]
public sealed class ClosedLoopTimeSeriesParameter :
    DoubleTimeSeriesParameter
{
    public ClosedLoopTimeSeriesParameter()
    {
    }

    public ClosedLoopTimeSeriesParameter(
        int planningHorizon,
        double defaultValue = 0.0)
        : base(
            planningHorizon,
            defaultValue)
    {
    }

    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        base.ValidateValue(
            value,
            parameterName);

        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A closed-loop period-dependent value cannot be negative.");
        }
    }
}
