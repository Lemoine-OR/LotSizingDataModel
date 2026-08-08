namespace LotSizingDataModel.Core.Common;

/// <summary>
/// Defines a model object containing data indexed by planning period.
///
/// Implementations must resize all their period-dependent data when
/// the planning horizon changes.
///
/// This interface enables bulk resizing of time-dependent properties
/// across multiple related objects.
/// </summary>
public interface IPlanningHorizonAware
{
    /// <summary>
    /// Gets the number of planning periods currently represented
    /// by the object's time-dependent data.
    /// </summary>
    int PlanningHorizon { get; }

    /// <summary>
    /// Resizes every time-dependent series contained in the object.
    ///
    /// Existing values must be preserved whenever possible.
    /// Newly created periods receive an appropriate default value.
    /// Values beyond a shortened horizon are discarded.
    /// </summary>
    /// <param name="periodCount">
    /// New number of planning periods.
    /// </param>
    void ResizeTimeSeries(int periodCount);
}