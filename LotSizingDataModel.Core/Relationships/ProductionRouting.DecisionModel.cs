using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;
using LotSizingDataModel.Core.DecisionModel.Constraints;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Adds the optional lot-sizing constraints associated
/// with a production routing.
/// </summary>
public sealed partial class ProductionRouting :
    IPlanningHorizonAware
{
    private MinimumLotSize? _minimumLotSize;
    private LotSizeMultiple? _lotSizeMultiple;
    private GroupingConstraint? _groupingConstraint;

    /// <summary>
    /// Gets or sets the minimum production-lot size
    /// for each planning period.
    ///
    /// A null value means that no minimum-lot-size
    /// constraint is active for this routing.
    /// </summary>
    [XmlElement("minimumLotSize")]
    public MinimumLotSize? MinimumLotSize
    {
        get => _minimumLotSize;
        set => SetDecisionParameter(
            ref _minimumLotSize,
            value,
            nameof(MinimumLotSize));
    }

    /// <summary>
    /// Gets or sets the standard production-lot size
    /// for each planning period.
    ///
    /// When this parameter is active, produced quantities
    /// must be integer multiples of its values.
    /// </summary>
    [XmlElement("lotSizeMultiple")]
    public LotSizeMultiple? LotSizeMultiple
    {
        get => _lotSizeMultiple;
        set => SetDecisionParameter(
            ref _lotSizeMultiple,
            value,
            nameof(LotSizeMultiple));
    }

    /// <summary>
    /// Gets or sets the production-flow grouping rule
    /// for each planning period.
    /// </summary>
    [XmlElement("groupingConstraint")]
    public GroupingConstraint? GroupingConstraint
    {
        get => _groupingConstraint;
        set => SetDecisionParameter(
            ref _groupingConstraint,
            value,
            nameof(GroupingConstraint));
    }

    /// <summary>
    /// Gets the planning horizon represented by the first
    /// active period-dependent routing parameter.
    ///
    /// Returns zero when no lot-sizing constraint is active.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon
    {
        get
        {
            // Return the planning horizon of the first active parameter
            foreach (IPlanningHorizonAware parameter
                     in GetDecisionParameters())
            {
                return parameter.PlanningHorizon;
            }

            // Return zero when no constraint is active
            return 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether at least one
    /// lot-sizing constraint is active.
    /// </summary>
    [XmlIgnore]
    public bool HasLotSizingConstraints =>
        MinimumLotSize is not null ||
        LotSizeMultiple is not null ||
        GroupingConstraint is not null;

    /// <summary>
    /// Gets a value indicating whether all active routing
    /// parameters use the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon
    {
        get
        {
            int? expectedHorizon = null;

            // Check if all active parameters use the same planning horizon
            foreach (IPlanningHorizonAware parameter
                     in GetDecisionParameters())
            {
                // Store the first parameter's horizon as reference
                if (expectedHorizon is null)
                {
                    expectedHorizon =
                        parameter.PlanningHorizon;

                    continue;
                }

                // Return false if any parameter has a different horizon
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
    /// Resizes every active period-dependent parameter
    /// associated with this production routing.
    ///
    /// Existing values are preserved.
    /// </summary>
    /// <param name="periodCount">
    /// New number of planning periods.
    /// </param>
    public void ResizeTimeSeries(int periodCount)
    {
        // Validate that the period count is non-negative
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
        }

        // Resize all active parameters to the new planning horizon
        foreach (IPlanningHorizonAware parameter
                 in GetDecisionParameters())
        {
            parameter.ResizeTimeSeries(periodCount);
        }

        // Notify dependent properties
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Removes all lot-sizing constraints from this routing.
    /// </summary>
    public void ClearLotSizingConstraints()
    {
        MinimumLotSize = null;
        LotSizeMultiple = null;
        GroupingConstraint = null;
    }

    private void SetDecisionParameter<T>(
        ref T? field,
        T? value,
        string propertyName)
        where T : ModelObject, IPlanningHorizonAware
    {
        // Avoid unnecessary updates if the reference is the same
        if (ReferenceEquals(field, value))
        {
            return;
        }

        // Unsubscribe from the old parameter's property changes
        if (field is not null)
        {
            field.PropertyChanged -=
                OnDecisionParameterPropertyChanged;
        }

        field = value;

        // Subscribe to the new parameter's property changes
        if (field is not null)
        {
            field.PropertyChanged +=
                OnDecisionParameterPropertyChanged;
        }

        // Notify dependent properties
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasLotSizingConstraints));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    private IEnumerable<IPlanningHorizonAware>
        GetDecisionParameters()
    {
        if (MinimumLotSize is not null)
        {
            yield return MinimumLotSize;
        }

        if (LotSizeMultiple is not null)
        {
            yield return LotSizeMultiple;
        }

        if (GroupingConstraint is not null)
        {
            yield return GroupingConstraint;
        }
    }

    private void OnDecisionParameterPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Propagate property change notifications for the specific parameter
        if (ReferenceEquals(sender, MinimumLotSize))
        {
            OnPropertyChanged(nameof(MinimumLotSize));
        }
        else if (ReferenceEquals(sender, LotSizeMultiple))
        {
            OnPropertyChanged(nameof(LotSizeMultiple));
        }
        else if (ReferenceEquals(
                     sender,
                     GroupingConstraint))
        {
            OnPropertyChanged(nameof(GroupingConstraint));
        }

        // Notify dependent computed properties
        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }
}