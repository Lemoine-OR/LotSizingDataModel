using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Setup-state transition semantics attached to one work center.
/// </summary>
[Serializable]
[XmlType(TypeName = "productionSetupTransitionProfile")]
public sealed class ProductionSetupTransitionProfile :
    ModelObject,
    IPlanningHorizonAware
{
    private SetupCarryOverPolicy _carryOverPolicy =
        SetupCarryOverPolicy.Unspecified;

    [XmlAttribute("carryOverPolicy")]
    public SetupCarryOverPolicy CarryOverPolicy
    {
        get => _carryOverPolicy;
        set => SetProperty(ref _carryOverPolicy, value);
    }

    [XmlArray("changeovers")]
    [XmlArrayItem("changeover")]
    public List<ProductionChangeover> Changeovers
    {
        get;
    } = new();

    [XmlIgnore]
    public bool HasSequenceDependentTimes =>
        Changeovers.Any(changeover =>
            changeover.ChangeoverTime is not null);

    [XmlIgnore]
    public bool HasSequenceDependentCosts =>
        Changeovers.Any(changeover =>
            changeover.ChangeoverCost is not null);

    [XmlIgnore]
    public int PlanningHorizon =>
        Changeovers
            .Select(changeover => changeover.PlanningHorizon)
            .FirstOrDefault(value => value > 0);

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        Changeovers.All(changeover =>
            changeover.HasConsistentPlanningHorizon) &&
        Changeovers
            .Select(changeover => changeover.PlanningHorizon)
            .Where(value => value > 0)
            .Distinct()
            .Take(2)
            .Count() <= 1;

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(periodCount));
        }

        foreach (ProductionChangeover changeover in Changeovers)
        {
            changeover.ResizeTimeSeries(periodCount);
        }

        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }
}
