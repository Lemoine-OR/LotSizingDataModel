using System;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents optional additional sales beyond mandatory demand.
/// </summary>
/// <remarks>
/// A sales option is intentionally distinct from demand, shortage, backlog and
/// lost-sales semantics. For each period, up to <see cref="MaximumAdditionalSales"/>
/// units may be sold at <see cref="UnitPrice"/> per unit.
/// </remarks>
[Serializable]
[XmlType(TypeName = "salesOption")]
public sealed class SalesOption :
    ModelObject,
    IPlanningHorizonAware
{
    private int _itemId;
    private int _distributionCenterId;
    private DoubleTimeSeries _maximumAdditionalSales = new();
    private DoubleTimeSeries _unitPrice = new();

    public SalesOption()
    {
    }

    public SalesOption(
        int itemId,
        int distributionCenterId,
        int planningHorizon)
    {
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemId));
        }

        if (distributionCenterId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distributionCenterId));
        }

        ItemId = itemId;
        DistributionCenterId = distributionCenterId;
        ResizeTimeSeries(planningHorizon);
    }

    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _itemId, value);
        }
    }

    [XmlAttribute("distributionCenterId")]
    public int DistributionCenterId
    {
        get => _distributionCenterId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _distributionCenterId, value);
        }
    }

    [XmlElement("maximumAdditionalSales")]
    public DoubleTimeSeries MaximumAdditionalSales
    {
        get => _maximumAdditionalSales;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetProperty(ref _maximumAdditionalSales, value);
        }
    }

    [XmlElement("unitPrice")]
    public DoubleTimeSeries UnitPrice
    {
        get => _unitPrice;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetProperty(ref _unitPrice, value);
        }
    }

    [XmlIgnore]
    public int PlanningHorizon =>
        Math.Max(
            MaximumAdditionalSales.PeriodCount,
            UnitPrice.PeriodCount);

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        MaximumAdditionalSales.PeriodCount == UnitPrice.PeriodCount;

    [XmlIgnore]
    public bool IsInternallyValid =>
        ItemId > 0 &&
        DistributionCenterId > 0 &&
        PlanningHorizon > 0 &&
        HasConsistentPlanningHorizon &&
        MaximumAdditionalSales.All(value => double.IsFinite(value) && value >= 0.0) &&
        UnitPrice.All(value => double.IsFinite(value) && value >= 0.0);

    public double GetMaximumAdditionalSales(int period) =>
        MaximumAdditionalSales[period];

    public void SetMaximumAdditionalSales(int period, double value)
    {
        ValidateNonNegative(value, nameof(value));
        MaximumAdditionalSales[period] = value;
    }

    public double GetUnitPrice(int period) =>
        UnitPrice[period];

    public void SetUnitPrice(int period, double value)
    {
        ValidateNonNegative(value, nameof(value));
        UnitPrice[period] = value;
    }

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(periodCount));
        }

        MaximumAdditionalSales.Resize(periodCount, 0.0);
        UnitPrice.Resize(periodCount, 0.0);

        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(nameof(IsInternallyValid));
    }

    private static void ValidateNonNegative(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) || value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value must be finite and non-negative.");
        }
    }
}
