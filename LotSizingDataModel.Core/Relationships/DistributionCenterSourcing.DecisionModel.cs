using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Adds the decision-model parameters associated with the
/// sourcing of an item by a distribution center from a warehouse.
/// </summary>
public sealed partial class DistributionCenterSourcing :
    IPlanningHorizonAware
{
    private BacklogConstraint? _backlogConstraint;
    private BacklogCost? _backlogCost;

    private ShortageConstraint? _shortageConstraint;
    private ShortageCost? _shortageCost;

    private SellingPrice? _sellingPrice;

    /// <summary>
    /// Gets or sets the maximum backlog quantity allowed
    /// during each planning period.
    ///
    /// Backlog demand remains to be fulfilled during
    /// a later planning period.
    ///
    /// A null value means that backlog is not authorized
    /// for this sourcing relationship.
    /// </summary>
    [XmlElement("backlogConstraint")]
    public BacklogConstraint? BacklogConstraint
    {
        get => _backlogConstraint;
        set => SetDecisionParameter(
            ref _backlogConstraint,
            value,
            nameof(BacklogConstraint));
    }

    /// <summary>
    /// Gets or sets the unit cost applied to each item unit
    /// remaining in backlog during a planning period.
    ///
    /// This parameter should only be defined when backlog
    /// is authorized.
    /// </summary>
    [XmlElement("backlogCost")]
    public BacklogCost? BacklogCost
    {
        get => _backlogCost;
        set => SetDecisionParameter(
            ref _backlogCost,
            value,
            nameof(BacklogCost));
    }

    /// <summary>
    /// Gets or sets the maximum quantity of demand that may
    /// be permanently lost during each planning period.
    ///
    /// A null value means that shortage is not authorized
    /// for this sourcing relationship.
    /// </summary>
    [XmlElement("shortageConstraint")]
    public ShortageConstraint? ShortageConstraint
    {
        get => _shortageConstraint;
        set => SetDecisionParameter(
            ref _shortageConstraint,
            value,
            nameof(ShortageConstraint));
    }

    /// <summary>
    /// Gets or sets the unit cost applied to each item unit
    /// permanently lost as shortage.
    ///
    /// This parameter should only be defined when shortage
    /// is authorized.
    /// </summary>
    [XmlElement("shortageCost")]
    public ShortageCost? ShortageCost
    {
        get => _shortageCost;
        set => SetDecisionParameter(
            ref _shortageCost,
            value,
            nameof(ShortageCost));
    }

    /// <summary>
    /// Gets or sets the unit selling price of the item supplied
    /// by the warehouse to the distribution center during each
    /// planning period.
    /// </summary>
    [XmlElement("sellingPrice")]
    public SellingPrice? SellingPrice
    {
        get => _sellingPrice;
        set => SetDecisionParameter(
            ref _sellingPrice,
            value,
            nameof(SellingPrice));
    }

    /// <summary>
    /// Gets the planning horizon represented by the first
    /// active period-dependent parameter.
    ///
    /// Returns zero when no decision parameter is active.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon
    {
        get
        {
            // Return the first active parameter's horizon.
            foreach (IPlanningHorizonAware parameter
                     in GetDecisionParameters())
            {
                return parameter.PlanningHorizon;
            }

            // No active parameter → horizon is zero.
            return 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether backlog is authorized.
    /// </summary>
    [XmlIgnore]
    public bool AllowsBacklog =>
        BacklogConstraint is not null;

    /// <summary>
    /// Gets a value indicating whether shortage is authorized.
    /// </summary>
    [XmlIgnore]
    public bool AllowsShortage =>
        ShortageConstraint is not null;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// decision-model parameter is active.
    /// </summary>
    [XmlIgnore]
    public bool HasDecisionParameters =>
        BacklogConstraint is not null ||
        BacklogCost is not null ||
        ShortageConstraint is not null ||
        ShortageCost is not null ||
        SellingPrice is not null;

    /// <summary>
    /// Gets a value indicating whether all active parameters
    /// use the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon
    {
        get
        {
            int? expectedHorizon = null;

            // Compare each active parameter's horizon against the first.
            foreach (IPlanningHorizonAware parameter
                     in GetDecisionParameters())
            {
                if (expectedHorizon is null)
                {
                    expectedHorizon =
                        parameter.PlanningHorizon;

                    continue;
                }

                if (parameter.PlanningHorizon !=
                    expectedHorizon.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the dependencies
    /// between service constraints and costs are respected.
    ///
    /// A cost cannot be defined if the corresponding
    /// service option is not authorized.
    /// </summary>
    [XmlIgnore]
    public bool HasValidDecisionConfiguration =>
        (BacklogCost is null ||
         BacklogConstraint is not null) &&

        (ShortageCost is null ||
         ShortageConstraint is not null);

    /// <summary>
    /// Resizes every active period-dependent parameter.
    ///
    /// Existing values are preserved.
    /// </summary>
    /// <param name="periodCount">
    /// New number of planning periods.
    /// </param>
    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
        }

        // Resize every active decision parameter.
        foreach (IPlanningHorizonAware parameter
                 in GetDecisionParameters())
        {
            parameter.ResizeTimeSeries(periodCount);
        }

        // Notify dependent computed properties.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Removes all optional decision-model parameters from
    /// this sourcing relationship.
    /// </summary>
    public void ClearDecisionParameters()
    {
        BacklogConstraint = null;
        BacklogCost = null;

        ShortageConstraint = null;
        ShortageCost = null;

        SellingPrice = null;
    }

    private void SetDecisionParameter<T>(
        ref T? field,
        T? value,
        string propertyName)
        where T : ModelObject, IPlanningHorizonAware
    {
        if (ReferenceEquals(field, value))
        {
            return;
        }

        // Unsubscribe from old parameter.
        if (field is not null)
        {
            field.PropertyChanged -=
                OnDecisionParameterPropertyChanged;
        }

        field = value;

        // Subscribe to new parameter.
        if (field is not null)
        {
            field.PropertyChanged +=
                OnDecisionParameterPropertyChanged;
        }

        // Notify the specific property plus all dependent ones.
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(AllowsBacklog));
        OnPropertyChanged(nameof(AllowsShortage));
        OnPropertyChanged(nameof(HasDecisionParameters));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(
            nameof(HasValidDecisionConfiguration));
    }

    private IEnumerable<IPlanningHorizonAware>
        GetDecisionParameters()
    {
        if (BacklogConstraint is not null)
        {
            yield return BacklogConstraint;
        }

        if (BacklogCost is not null)
        {
            yield return BacklogCost;
        }

        if (ShortageConstraint is not null)
        {
            yield return ShortageConstraint;
        }

        if (ShortageCost is not null)
        {
            yield return ShortageCost;
        }

        if (SellingPrice is not null)
        {
            yield return SellingPrice;
        }
    }

    private void OnDecisionParameterPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Forward notification for the specific parameter that changed.
        if (ReferenceEquals(sender, BacklogConstraint))
        {
            OnPropertyChanged(nameof(BacklogConstraint));
        }
        else if (ReferenceEquals(sender, BacklogCost))
        {
            OnPropertyChanged(nameof(BacklogCost));
        }
        else if (ReferenceEquals(sender, ShortageConstraint))
        {
            OnPropertyChanged(nameof(ShortageConstraint));
        }
        else if (ReferenceEquals(sender, ShortageCost))
        {
            OnPropertyChanged(nameof(ShortageCost));
        }
        else if (ReferenceEquals(sender, SellingPrice))
        {
            OnPropertyChanged(nameof(SellingPrice));
        }

        // Update all computed properties that may be affected.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(
            nameof(HasValidDecisionConfiguration));
    }
}