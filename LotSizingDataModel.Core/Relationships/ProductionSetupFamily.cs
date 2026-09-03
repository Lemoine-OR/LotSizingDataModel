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
/// A production setup family is not a commercial product family,
/// a BOM grouping, a GroupingConstraint, or an item-level setup.
/// It states that setup activation of any member item requires one
/// common family-level setup activation on the referenced work center.
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

    /// <summary>
    /// Item identifiers sharing this setup family on the work center.
    /// Membership is intentionally binary in this first generic model.
    /// </summary>
    [XmlArray("memberItemIds")]
    [XmlArrayItem("itemId")]
    public List<int> MemberItemIds { get; } = new();

    /// <summary>
    /// Optional family-level capacity consumption by period.
    /// Null means that no family setup time is represented.
    /// </summary>
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
            OnPropertyChanged(nameof(HasConsistentMemberIds));
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
    public bool HasMembers =>
        MemberItemIds.Count > 0;

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
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
