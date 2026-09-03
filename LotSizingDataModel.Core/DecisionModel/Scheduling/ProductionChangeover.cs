using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Sequence-dependent transition from one item setup state to another.
/// </summary>
[Serializable]
[XmlType(TypeName = "productionChangeover")]
public sealed class ProductionChangeover :
    ModelObject,
    IPlanningHorizonAware
{
    private int _fromItemId;
    private int _toItemId;
    private ProductionChangeoverTime? _changeoverTime;
    private ProductionChangeoverCost? _changeoverCost;

    [XmlAttribute("fromItemId")]
    public int FromItemId
    {
        get => _fromItemId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
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
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _toItemId, value);
        }
    }

    [XmlElement("changeoverTime")]
    public ProductionChangeoverTime? ChangeoverTime
    {
        get => _changeoverTime;
        set => SetParameter(ref _changeoverTime, value, nameof(ChangeoverTime));
    }

    [XmlElement("changeoverCost")]
    public ProductionChangeoverCost? ChangeoverCost
    {
        get => _changeoverCost;
        set => SetParameter(ref _changeoverCost, value, nameof(ChangeoverCost));
    }

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
            throw new ArgumentOutOfRangeException(nameof(periodCount));
        }

        ChangeoverTime?.ResizeTimeSeries(periodCount);
        ChangeoverCost?.ResizeTimeSeries(periodCount);

        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    private void SetParameter<T>(
        ref T? field,
        T? value,
        string propertyName)
        where T : ModelObject, IPlanningHorizonAware
    {
        if (ReferenceEquals(field, value))
        {
            return;
        }

        if (field is not null)
        {
            field.PropertyChanged -= OnParameterChanged;
        }

        field = value;

        if (field is not null)
        {
            field.PropertyChanged += OnParameterChanged;
        }

        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    private void OnParameterChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }
}
