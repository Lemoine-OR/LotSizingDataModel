using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Finance;

/// <summary>
/// Represents the maximum modeled operating expenditure allowed in each
/// planning period.
/// </summary>
/// <remarks>
/// The executable alpha.23 formulation interprets this parameter against
/// positive coefficients of period-indexed variables in the economic
/// objective. Negative revenue coefficients do not replenish the budget and
/// positive objective terms without a period segment are outside this periodic
/// operating envelope.
/// </remarks>
[Serializable]
[XmlType(TypeName = "periodicOperatingExpenditureBudget")]
public sealed class PeriodicOperatingExpenditureBudget :
    DoubleTimeSeriesParameter
{
    public PeriodicOperatingExpenditureBudget()
    {
    }

    public PeriodicOperatingExpenditureBudget(
        int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)
    {
    }

    public PeriodicOperatingExpenditureBudget(
        int planningHorizon,
        double defaultBudget)
        : base(
            planningHorizon,
            defaultBudget)
    {
    }

    [XmlIgnore]
    public DoubleTimeSeries BudgetByPeriod =>
        Values;

    public double GetBudget(int period) =>
        GetValue(period);

    public void SetBudget(
        int period,
        double budget) =>
            SetValue(period, budget);

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
                "An operating expenditure budget cannot be negative.");
        }
    }
}
