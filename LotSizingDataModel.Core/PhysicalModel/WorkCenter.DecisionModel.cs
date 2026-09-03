using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Adds the optional decision-model parameters associated
/// with a work center.
/// </summary>
public sealed partial class WorkCenter :
    IPlanningHorizonAware
{
    private CapacityConstraint? _capacityConstraint;
    private AdditionalCapacity? _additionalCapacity;
    private FixedUsageCost? _fixedUsageCost;
    private AdditionalCapacityCost? _additionalCapacityCost;

    /// <summary>
    /// Gets or sets the maximum regular capacity available
    /// during each planning period.
    ///
    /// A null value means that the work center is not subject
    /// to an explicit capacity constraint.
    /// </summary>
    [XmlElement("capacityConstraint")]
    public CapacityConstraint? CapacityConstraint
    {
        get => _capacityConstraint;
        set => SetDecisionParameter(
            ref _capacityConstraint,
            value,
            nameof(CapacityConstraint));
    }

    /// <summary>
    /// Gets or sets the maximum additional capacity that may
    /// be allocated during each planning period.
    ///
    /// This parameter should only be defined when a regular
    /// capacity constraint is also defined.
    /// </summary>
    [XmlElement("additionalCapacity")]
    public AdditionalCapacity? AdditionalCapacity
    {
        get => _additionalCapacity;
        set => SetDecisionParameter(
            ref _additionalCapacity,
            value,
            nameof(AdditionalCapacity));
    }

    /// <summary>
    /// Gets or sets the fixed cost incurred when the work center
    /// is used during a planning period.
    /// </summary>
    [XmlElement("fixedUsageCost")]
    public FixedUsageCost? FixedUsageCost
    {
        get => _fixedUsageCost;
        set => SetDecisionParameter(
            ref _fixedUsageCost,
            value,
            nameof(FixedUsageCost));
    }

    /// <summary>
    /// Gets or sets the cost of using one unit of additional
    /// capacity during each planning period.
    ///
    /// This parameter should only be defined when additional
    /// capacity is authorized.
    /// </summary>
    [XmlElement("additionalCapacityCost")]
    public AdditionalCapacityCost? AdditionalCapacityCost
    {
        get => _additionalCapacityCost;
        set => SetDecisionParameter(
            ref _additionalCapacityCost,
            value,
            nameof(AdditionalCapacityCost));
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
    /// Gets a value indicating whether at least one
    /// decision-model parameter is active.
    /// </summary>
    [XmlIgnore]
    public bool HasDecisionParameters =>
        CapacityConstraint is not null ||
        AdditionalCapacity is not null ||
        FixedUsageCost is not null ||
        AdditionalCapacityCost is not null ||
        SetupTransitionProfile is not null ||
        SchedulingProfile is not null;

    /// <summary>
    /// Gets a value indicating whether every active parameter
    /// uses the same planning horizon.
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
    /// Gets a value indicating whether the dependencies between
    /// capacity-related parameters are respected.
    /// </summary>
    [XmlIgnore]
    public bool HasValidCapacityConfiguration =>
        (AdditionalCapacity is null ||
         CapacityConstraint is not null) &&
        (AdditionalCapacityCost is null ||
         AdditionalCapacity is not null);

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
    /// Removes all decision-model parameters from
    /// this work center.
    /// </summary>
    public void ClearDecisionParameters()
    {
        CapacityConstraint = null;
        AdditionalCapacity = null;
        FixedUsageCost = null;
        AdditionalCapacityCost = null;
        SetupTransitionProfile = null;
        SchedulingProfile = null;
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
        OnPropertyChanged(nameof(HasDecisionParameters));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(
            nameof(HasValidCapacityConfiguration));
    }

    private IEnumerable<IPlanningHorizonAware>
        GetDecisionParameters()
    {
        if (CapacityConstraint is not null)
        {
            yield return CapacityConstraint;
        }

        if (AdditionalCapacity is not null)
        {
            yield return AdditionalCapacity;
        }

        if (FixedUsageCost is not null)
        {
            yield return FixedUsageCost;
        }

        if (AdditionalCapacityCost is not null)
        {
            yield return AdditionalCapacityCost;
        }

        if (SetupTransitionProfile is not null)
        {
            yield return SetupTransitionProfile;
        }

        if (SchedulingProfile is not null)
        {
            yield return SchedulingProfile;
        }
    }

    private void OnDecisionParameterPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Forward notification for the specific parameter that changed.
        if (ReferenceEquals(sender, CapacityConstraint))
        {
            OnPropertyChanged(nameof(CapacityConstraint));
        }
        else if (ReferenceEquals(sender, AdditionalCapacity))
        {
            OnPropertyChanged(nameof(AdditionalCapacity));
        }
        else if (ReferenceEquals(sender, FixedUsageCost))
        {
            OnPropertyChanged(nameof(FixedUsageCost));
        }
        else if (ReferenceEquals(
                     sender,
                     AdditionalCapacityCost))
        {
            OnPropertyChanged(nameof(AdditionalCapacityCost));
        }

        // Update all computed properties that may be affected.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(
            nameof(HasValidCapacityConfiguration));
    }
}