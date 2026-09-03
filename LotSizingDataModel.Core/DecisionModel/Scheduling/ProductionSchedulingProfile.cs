using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

[Serializable]
[XmlType(TypeName = "productionSchedulingProfile")]
public sealed class ProductionSchedulingProfile :
    ModelObject,
    IPlanningHorizonAware
{
    private SchedulingBucketMode _bucketMode =
        SchedulingBucketMode.Unspecified;

    private int _microPeriodsPerPeriod = 1;

    [XmlAttribute("bucketMode")]
    public SchedulingBucketMode BucketMode
    {
        get => _bucketMode;
        set => SetProperty(ref _bucketMode, value);
    }

    [XmlAttribute("microPeriodsPerPeriod")]
    public int MicroPeriodsPerPeriod
    {
        get => _microPeriodsPerPeriod;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "MicroPeriodsPerPeriod must be strictly positive.");
            }

            SetProperty(ref _microPeriodsPerPeriod, value);
        }
    }

    [XmlIgnore]
    public int PlanningHorizon { get; private set; }

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(periodCount));
        }

        PlanningHorizon = periodCount;
        OnPropertyChanged(nameof(PlanningHorizon));
    }
}
