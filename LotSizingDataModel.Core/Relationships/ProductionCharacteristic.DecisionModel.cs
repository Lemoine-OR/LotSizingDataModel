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
/// Adds the optional decision-model parameters associated
/// with the production of an item on a work center.
/// </summary>
public sealed partial class ProductionCharacteristic :
    IPlanningHorizonAware
{
    private UnitCapacityConsumption? _unitCapacityConsumption;
    private SetupTime? _setupTime;
    private StartUpTime? _startUpTime;
    private FixedSetupCost? _fixedSetupCost;
    private StartUpCost? _startUpCost;
    private UnitUsageCost? _unitUsageCost;

    /// <summary>
    /// Gets or sets the capacity consumed to produce one unit
    /// of the item during each planning period.
    ///
    /// A null value means that no unit-capacity consumption
    /// is defined for this item-work-center association.
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
    /// Gets or sets the capacity consumed to prepare the
    /// work center for the item during each planning period.
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
    /// Gets or sets the capacity consumed when a new sequence of
    /// production setups starts during each planning period.
    /// </summary>
    [XmlElement("startUpTime")]
    public StartUpTime? StartUpTime
    {
        get => _startUpTime;
        set => SetDecisionParameter(
            ref _startUpTime,
            value,
            nameof(StartUpTime));
    }

    /// <summary>
    /// Gets or sets the fixed cost incurred when the work center
    /// is prepared to produce the item during a planning period.
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
    /// Gets or sets the cost incurred when a new sequence of
    /// production setups starts during each planning period.
    /// </summary>
    [XmlElement("startUpCost")]
    public StartUpCost? StartUpCost
    {
        get => _startUpCost;
        set => SetDecisionParameter(
            ref _startUpCost,
            value,
            nameof(StartUpCost));
    }

    /// <summary>
    /// Gets or sets the cost of producing one unit of the item
    /// on the work center during each planning period.
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
    /// production decision parameter is active.
    /// </summary>
    [XmlIgnore]
    public bool HasDecisionParameters =>
        UnitCapacityConsumption is not null ||
        SetupTime is not null ||
        StartUpTime is not null ||
        FixedSetupCost is not null ||
        StartUpCost is not null ||
        UnitUsageCost is not null;

    /// <summary>
    /// Gets a value indicating whether this production
    /// characteristic consumes work-center capacity.
    ///
    /// The global model validator will verify that the referenced
    /// work center is subject to a capacity constraint.
    /// </summary>
    [XmlIgnore]
    public bool RequiresCapacityConstrainedWorkCenter =>
        UnitCapacityConsumption is not null ||
        SetupTime is not null ||
        StartUpTime is not null;

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
    /// Removes all decision-model parameters from this
    /// production characteristic.
    /// </summary>
    public void ClearDecisionParameters()
    {
        UnitCapacityConsumption = null;
        SetupTime = null;
        StartUpTime = null;
        FixedSetupCost = null;
        StartUpCost = null;
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
            nameof(RequiresCapacityConstrainedWorkCenter));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    private IEnumerable<IPlanningHorizonAware>
        GetDecisionParameters()
    {
        if (UnitCapacityConsumption is not null)
        {
            yield return UnitCapacityConsumption;
        }

        if (SetupTime is not null)
        {
            yield return SetupTime;
        }

        if (StartUpTime is not null)
        {
            yield return StartUpTime;
        }

        if (FixedSetupCost is not null)
        {
            yield return FixedSetupCost;
        }

        if (StartUpCost is not null)
        {
            yield return StartUpCost;
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
        if (ReferenceEquals(
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
        else if (ReferenceEquals(sender, StartUpTime))
        {
            OnPropertyChanged(nameof(StartUpTime));
        }
        else if (ReferenceEquals(sender, FixedSetupCost))
        {
            OnPropertyChanged(nameof(FixedSetupCost));
        }
        else if (ReferenceEquals(sender, StartUpCost))
        {
            OnPropertyChanged(nameof(StartUpCost));
        }
        else if (ReferenceEquals(sender, UnitUsageCost))
        {
            OnPropertyChanged(nameof(UnitUsageCost));
        }

        // Update all computed properties that may be affected.
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }
}