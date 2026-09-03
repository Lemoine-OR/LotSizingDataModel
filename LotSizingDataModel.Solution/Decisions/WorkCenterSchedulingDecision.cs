using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

[Serializable]
[XmlType(TypeName = "workCenterSchedulingDecision")]
public sealed class WorkCenterSchedulingDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private WorkCenterReference _workCenter = new();
    private int _planningHorizon;

    [XmlElement("workCenter")]
    public WorkCenterReference WorkCenter
    {
        get => _workCenter;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetProperty(ref _workCenter, value);
        }
    }

    [XmlArray("microPeriods")]
    [XmlArrayItem("microPeriod")]
    public List<WorkCenterMicroPeriodDecision> MicroPeriods
    {
        get;
    } = new();

    [XmlAttribute("planningHorizon")]
    public int PlanningHorizon
    {
        get => _planningHorizon;
        set => ResizeTimeSeries(value);
    }

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(periodCount));
        }

        _planningHorizon = periodCount;
        OnPropertyChanged(nameof(PlanningHorizon));
    }

    [XmlIgnore]
    public bool IsInternallyValid =>
        PlanningHorizon > 0 &&
        WorkCenter.PlantId > 0 &&
        WorkCenter.WorkCenterId > 0 &&
        MicroPeriods.All(decision =>
            decision.Period >= 1 &&
            decision.Period <= PlanningHorizon &&
            decision.MicroPeriod >= 1 &&
            decision.ItemId >= 0 &&
            double.IsFinite(decision.Quantity) &&
            decision.Quantity >= 0.0);
}
