using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Finance;

/// <summary>
/// Defines cash-flow timing and solvency semantics independently of OPEX budget.
/// </summary>
/// <remarks>
/// Economic objective coefficients remain the source of decision-dependent
/// receipts/disbursements. Positive economic coefficients are disbursements;
/// negative coefficients are receipts. Fixed net cash flows are represented
/// separately and may be positive or negative.
/// </remarks>
[Serializable]
[XmlType(TypeName = "cashFlowPolicy")]
public sealed class CashFlowPolicy :
    ModelObject,
    IPlanningHorizonAware
{
    private double _initialCashBalance;
    private int _receiptDelayPeriods;
    private int _disbursementDelayPeriods;
    private bool _enforceMinimumCashBalance = true;

    private DoubleTimeSeries _fixedNetCashFlow = new();
    private DoubleTimeSeries _minimumCashBalance = new();

    [XmlAttribute("initialCashBalance")]
    public double InitialCashBalance
    {
        get => _initialCashBalance;
        set
        {
            EnsureFinite(value, nameof(value));
            SetProperty(ref _initialCashBalance, value);
        }
    }

    [XmlAttribute("receiptDelayPeriods")]
    public int ReceiptDelayPeriods
    {
        get => _receiptDelayPeriods;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _receiptDelayPeriods, value);
        }
    }

    [XmlAttribute("disbursementDelayPeriods")]
    public int DisbursementDelayPeriods
    {
        get => _disbursementDelayPeriods;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _disbursementDelayPeriods, value);
        }
    }

    [XmlAttribute("enforceMinimumCashBalance")]
    public bool EnforceMinimumCashBalance
    {
        get => _enforceMinimumCashBalance;
        set => SetProperty(ref _enforceMinimumCashBalance, value);
    }

    [XmlElement("fixedNetCashFlow")]
    public DoubleTimeSeries FixedNetCashFlow
    {
        get => _fixedNetCashFlow;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetProperty(ref _fixedNetCashFlow, value);
        }
    }

    [XmlElement("minimumCashBalance")]
    public DoubleTimeSeries MinimumCashBalance
    {
        get => _minimumCashBalance;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetProperty(ref _minimumCashBalance, value);
        }
    }

    [XmlIgnore]
    public int PlanningHorizon =>
        Math.Max(
            FixedNetCashFlow.PeriodCount,
            MinimumCashBalance.PeriodCount);

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        FixedNetCashFlow.PeriodCount ==
        MinimumCashBalance.PeriodCount;

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(periodCount));
        }

        FixedNetCashFlow.Resize(periodCount, 0.0);
        MinimumCashBalance.Resize(periodCount, 0.0);

        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    private static void EnsureFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Cash values must be finite.");
        }
    }
}
