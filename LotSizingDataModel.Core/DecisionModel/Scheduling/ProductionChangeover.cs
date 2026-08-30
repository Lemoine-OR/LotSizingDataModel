using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// One ordered setup-state transition on a work center.
/// </summary>
/// <remarks>
/// The transition is directional: A -> B and B -> A are distinct definitions.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productionChangeover")]
public sealed class ProductionChangeover :
    ModelObject,
    IPlanningHorizonAware
{
    private int _fromItemId;
    private int _toItemId;

    [XmlAttribute("fromItemId")]
    public int FromItemId
    {
        get => _fromItemId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The origin item identifier cannot be negative.");
            }

            SetProperty(ref _fromItemId, value);
        }
    }

    [XmlAttribute("toItemId")]
    public int ToItemId
    {
        get => _toItemId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The destination item identifier cannot be negative.");
            }

            SetProperty(ref _toItemId, value);
        }
    }

    [XmlElement("changeoverTime")]
    public SequenceDependentChangeoverTime? ChangeoverTime
    {
        get;
        set;
    }

    [XmlElement("changeoverCost")]
    public SequenceDependentChangeoverCost? ChangeoverCost
    {
        get;
        set;
    }

    [XmlIgnore]
    public bool IsNonTrivialTransition =>
        FromItemId != ToItemId;

    [XmlIgnore]
    public int PlanningHorizon =>
        ChangeoverTime?.PlanningHorizon ??
        ChangeoverCost?.PlanningHorizon ??
        0;

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        ChangeoverTime is null ||
        ChangeoverCost is null ||
        ChangeoverTime.PlanningHorizon ==
            ChangeoverCost.PlanningHorizon;

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
        }

        ChangeoverTime?.ResizeTimeSeries(periodCount);
        ChangeoverCost?.ResizeTimeSeries(periodCount);

        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }
}
