using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Represents a period-dependent sequence-dependent setup/changeover time.
/// </summary>
[Serializable]
[XmlType(TypeName = "productionChangeoverTime")]
public sealed class ProductionChangeoverTime : DoubleTimeSeriesParameter
{
    public ProductionChangeoverTime() { }

    public ProductionChangeoverTime(int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public ProductionChangeoverTime(int planningHorizon, double defaultValue)
        : base(planningHorizon, defaultValue)
    {
    }

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
                "A changeover parameter cannot be negative.");
        }
    }
}
