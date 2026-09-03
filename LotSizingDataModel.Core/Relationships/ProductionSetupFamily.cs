using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents a shared production setup family on one work center.
/// </summary>
/// <remarks>
/// A production setup family is distinct from a commercial product family,
/// a BOM grouping, GroupingConstraint, and an item-level setup.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productionSetupFamily")]
public sealed class ProductionSetupFamily :
    IdentifiedEntity,
    IPlanningHorizonAware
{
    private WorkCenterReference _workCenter = new();
    private ProductionFamilySetupTime? _setupTime;

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

    [XmlArray("memberItemIds")]
    [XmlArrayItem("itemId")]
    public List<int> MemberItemIds { get; } = new();

    [XmlElement("setupTime")]
    public ProductionFamilySetupTime? SetupTime
    {
        get => _setupTime;
        set
        {
            if (ReferenceEquals(_setupTime, value))
            {
                return;
            }

            if (_setupTime is not null)
            {
                _setupTime.PropertyChanged -= OnSetupTimePropertyChanged;
            }

            _setupTime = value;

            if (_setupTime is not null)
            {
                _setupTime.PropertyChanged += OnSetupTimePropertyChanged;
            }

            OnPropertyChanged(nameof(SetupTime));
            OnPropertyChanged(nameof(PlanningHorizon));
        }
    }

    [XmlIgnore]
    public int PlanningHorizon =>
        SetupTime?.PlanningHorizon ?? 0;

    [XmlIgnore]
    public bool HasConsistentMemberIds =>
        MemberItemIds.All(itemId => itemId > 0) &&
        MemberItemIds.Distinct().Count() == MemberItemIds.Count;

    [XmlIgnore]
    public bool HasMembers => MemberItemIds.Count > 0;

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount));
        }

        SetupTime?.ResizeTimeSeries(periodCount);
        OnPropertyChanged(nameof(PlanningHorizon));
    }

    public bool ContainsItem(int itemId) =>
        MemberItemIds.Contains(itemId);

    private void OnSetupTimePropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SetupTime));
        OnPropertyChanged(nameof(PlanningHorizon));
    }
}
