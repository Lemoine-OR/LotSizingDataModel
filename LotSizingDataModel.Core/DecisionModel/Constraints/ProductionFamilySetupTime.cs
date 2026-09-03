using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Constraints;

/// <summary>
/// Capacity consumed when a shared production-family setup is active.
/// </summary>
/// <remarks>
/// This parameter is deliberately distinct from item-level SetupTime.
/// It represents the common preparation effort shared by several items.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productionFamilySetupTime")]
public sealed class ProductionFamilySetupTime :
    DoubleTimeSeriesParameter
{
    public ProductionFamilySetupTime()
    {
    }

    public ProductionFamilySetupTime(int planningHorizon)
        : base(planningHorizon, defaultValue: 0.0)
    {
    }

    public ProductionFamilySetupTime(
        int planningHorizon,
        double defaultSetupTime)
        : base(planningHorizon, defaultSetupTime)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries SetupTimeByPeriod => Values;

    public double GetSetupTime(int period) => GetValue(period);

    public void SetSetupTime(int period, double setupTime) =>
        SetValue(period, setupTime);

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
                "A production-family setup time cannot be negative.");
        }
    }
}
