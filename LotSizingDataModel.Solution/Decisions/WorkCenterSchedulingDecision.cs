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

    public WorkCenterSchedulingDecision()
    {
    }

    public WorkCenterSchedulingDecision(
        WorkCenterReference workCenter,
        int planningHorizon)
    {
        WorkCenter = workCenter ??
            throw new ArgumentNullException(nameof(workCenter));
        PlanningHorizon = planningHorizon;
    }

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

    [XmlAttribute("planningHorizon")]
    public int PlanningHorizon
    {
        get => _planningHorizon;
        set => ResizeTimeSeries(value);
    }

    [XmlArray("microPeriods")]
    [XmlArrayItem("microPeriodDecision")]
    public List<ProductionMicroPeriodDecision> MicroPeriods { get; } = new();

    [XmlIgnore]
    public int MicroPeriodDecisionCount => MicroPeriods.Count;

    [XmlIgnore]
    public bool HasUniqueMicroPeriodKeys =>
        MicroPeriods
            .Select(decision =>
                (
                    decision.MicroPeriod.MacroPeriod,
                    decision.MicroPeriod.MicroPeriodIndex
                ))
            .Distinct()
            .Count() ==
        MicroPeriods.Count;

    [XmlIgnore]
    public bool IsInternallyValid =>
        WorkCenter.PlantId > 0 &&
        WorkCenter.WorkCenterId > 0 &&
        PlanningHorizon > 0 &&
        MicroPeriods.All(decision =>
            decision.IsInternallyValid &&
            decision.MicroPeriod.MacroPeriod <= PlanningHorizon) &&
        HasUniqueMicroPeriodKeys;

    public IReadOnlyList<ProductionMicroPeriodDecision>
        GetMicroPeriods(int macroPeriod)
    {
        if (macroPeriod <= 0 || macroPeriod > PlanningHorizon)
            throw new ArgumentOutOfRangeException(
                nameof(macroPeriod),
                macroPeriod,
                "The macro period is outside the scheduling horizon.");

        return MicroPeriods
            .Where(decision =>
                decision.MicroPeriod.MacroPeriod == macroPeriod)
            .OrderBy(decision =>
                decision.MicroPeriod.MicroPeriodIndex)
            .ToArray();
    }

    public void AddMicroPeriodDecision(
        ProductionMicroPeriodDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (!decision.IsInternallyValid)
            throw new ArgumentException(
                "The micro-period decision is not internally valid.",
                nameof(decision));

        if (decision.MicroPeriod.MacroPeriod > PlanningHorizon)
            throw new ArgumentException(
                "The micro-period macro index exceeds the scheduling horizon.",
                nameof(decision));

        if (MicroPeriods.Any(existing =>
                existing.RefersToSameMicroPeriod(decision)))
            throw new InvalidOperationException(
                $"A micro-period decision already exists for {decision.MicroPeriod}.");

        MicroPeriods.Add(decision);
        OnPropertyChanged(nameof(MicroPeriods));
        OnPropertyChanged(nameof(MicroPeriodDecisionCount));
        OnPropertyChanged(nameof(HasUniqueMicroPeriodKeys));
        OnPropertyChanged(nameof(IsInternallyValid));
    }

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");

        SetProperty(
            ref _planningHorizon,
            periodCount,
            nameof(PlanningHorizon));

        MicroPeriods.RemoveAll(decision =>
            decision.MicroPeriod.MacroPeriod > periodCount);

        OnPropertyChanged(nameof(MicroPeriods));
        OnPropertyChanged(nameof(MicroPeriodDecisionCount));
        OnPropertyChanged(nameof(IsInternallyValid));
    }
}
