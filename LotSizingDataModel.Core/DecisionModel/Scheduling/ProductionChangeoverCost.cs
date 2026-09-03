using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Represents a period-dependent sequence-dependent setup/changeover cost.
/// </summary>
[Serializable]
[XmlType(TypeName = "productionChangeoverCost")]
public sealed class ProductionChangeoverCost : DoubleTimeSeriesParameter
{
    public ProductionChangeoverCost() { }

    public ProductionChangeoverCost(int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public ProductionChangeoverCost(int planningHorizon, double defaultValue)
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
