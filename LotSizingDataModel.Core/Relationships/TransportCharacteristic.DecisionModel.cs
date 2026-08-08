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
/// Adds the optional decision-model parameters associated with
/// transporting an item using a transport resource.
/// </summary>
public sealed partial class TransportCharacteristic :
    IPlanningHorizonAware
{
    private CapacityConstraint? _capacityConstraint;
    private AdditionalCapacity? _additionalCapacity;
    private AdditionalCapacityCost? _additionalCapacityCost;

    private UnitCapacityConsumption? _unitCapacityConsumption;
    private SetupTime? _setupTime;
    private FixedSetupCost? _fixedSetupCost;
    private UnitUsageCost? _unitUsageCost;

    /// <summary>
    /// Gets or sets the maximum transport capacity available
    /// specifically for this item during each planning period.
    ///
    /// A null value means that no item-specific capacity limit
    /// is defined.
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
    /// Gets or sets the maximum additional transport capacity
    /// available specifically for this item.
    ///
    /// This parameter should only be defined when an item-specific
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
    /// Gets or sets the unit cost of additional transport
    /// capacity allocated to this item.
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
    /// Gets or sets the transport capacity consumed by one unit
    /// of the item during each planning period.
    /// </summary>
    [XmlElement("unitCapacityConsumption")]
    public UnitCapacityConsumption? UnitCapacityConsumption
    {
        get => _unitCapacityConsumption;
        set => SetDecisionParameter(
            ref _unitCapacityConsumption,
            value,
            nameof(UnitCapacityConsumption));
    }

    /// <summary>
    /// Gets or sets the capacity consumed to prepare the transport
    /// resource for this item during each planning period.
    /// </summary>
    [XmlElement("setupTime")]
    public SetupTime? SetupTime
    {
        get => _setupTime;
        set => SetDecisionParameter(
            ref _setupTime,
            value,
            nameof(SetupTime));
    }

    /// <summary>
    /// Gets or sets the fixed cost incurred when the transport
    /// resource is prepared for this item.
    /// </summary>
    [XmlElement("fixedSetupCost")]
    public FixedSetupCost? FixedSetupCost
    {
        get => _fixedSetupCost;
        set => SetDecisionParameter(
            ref _fixedSetupCost,
            value,
            nameof(FixedSetupCost));
    }

    /// <summary>
    /// Gets or sets the cost of transporting one unit of the item
    /// during each planning period.
    /// </summary>
    [XmlElement("unitUsageCost")]
    public UnitUsageCost? UnitUsageCost
    {
        get => _unitUsageCost;
        set => SetDecisionParameter(
            ref _unitUsageCost,
            value,
            nameof(UnitUsageCost));
    }

    /// <summary>
    /// Gets the planning horizon represented by the first active
    /// period-dependent parameter.
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
        AdditionalCapacityCost is not null ||
        UnitCapacityConsumption is not null ||
        SetupTime is not null ||
        FixedSetupCost is not null ||
        UnitUsageCost is not null;

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
    /// Gets a value indicating whether the dependencies between
    /// the transport parameters are respected.
    /// </summary>
    [XmlIgnore]
    public bool HasValidDecisionConfiguration =>
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
    /// Removes all optional decision-model parameters from this
    /// transport characteristic.
    /// </summary>
    public void ClearDecisionParameters()
    {
        CapacityConstraint = null;
        AdditionalCapacity = null;
        AdditionalCapacityCost = null;

        UnitCapacityConsumption = null;
        SetupTime = null;
        FixedSetupCost = null;
        UnitUsageCost = null;
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
            nameof(HasValidDecisionConfiguration));
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

        if (AdditionalCapacityCost is not null)
        {
            yield return AdditionalCapacityCost;
        }

        if (UnitCapacityConsumption is not null)
        {
            yield return UnitCapacityConsumption;
        }

        if (SetupTime is not null)
        {
            yield return SetupTime;
        }

        if (FixedSetupCost is not null)
        {
            yield return FixedSetupCost;
        }

        if (UnitUsageCost is not null)
        {
            yield return UnitUsageCost;
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
        else if (ReferenceEquals(
                     sender,
                     AdditionalCapacityCost))
        {
            OnPropertyChanged(
                nameof(AdditionalCapacityCost));
        }
        else if (ReferenceEquals(
                     sender,
                     UnitCapacityConsumption))
        {
            OnPropertyChanged(
                nameof(UnitCapacityConsumption));
        }
        else if (ReferenceEquals(sender, SetupTime))
        {
            OnPropertyChanged(nameof(SetupTime));
        }
        else if (ReferenceEquals(sender, FixedSetupCost))
        {
            OnPropertyChanged(nameof(FixedSetupCost));
        }
        else if (ReferenceEquals(sender, UnitUsageCost))
        {
            OnPropertyChanged(nameof(UnitUsageCost));
        }

        // Update all computed properties that may be affected.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
        OnPropertyChanged(
            nameof(HasValidDecisionConfiguration));
    }
}