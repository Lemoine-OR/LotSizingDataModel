using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the global capacity decisions associated with one
/// work center over the complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// This class stores decisions that apply to the complete
/// work center, independently of individual items or
/// production routings.
///
/// Item-specific production quantities and setup decisions
/// are stored separately in production decisions.
/// </remarks>
[Serializable]
[XmlType(TypeName = "workCenterCapacityDecision")]
public sealed class WorkCenterCapacityDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private WorkCenterReference _workCenter =
        new();

    private IntegerTimeSeries _activations =
        new();

    private DoubleTimeSeries _additionalCapacityUsed =
        new();

    /// <summary>
    /// Initializes an empty work-center capacity decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public WorkCenterCapacityDecision()
    {
        SubscribeToObject(_workCenter);
        SubscribeToObject(_activations);
        SubscribeToObject(_additionalCapacityUsed);
    }

    /// <summary>
    /// Initializes a capacity decision for a work center
    /// and a planning horizon.
    /// </summary>
    /// <param name="workCenter">
    /// Reference to the work center.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public WorkCenterCapacityDecision(
        WorkCenterReference workCenter,
        int planningHorizon)
        : this()
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        if (workCenter.PlantId <= 0)
        {
            throw new ArgumentException(
                "The plant identifier must be strictly positive.",
                nameof(workCenter));
        }

        if (workCenter.WorkCenterId <= 0)
        {
            throw new ArgumentException(
                "The work-center identifier must be " +
                "strictly positive.",
                nameof(workCenter));
        }

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        WorkCenter = workCenter;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the reference to the work center
    /// associated with the capacity decision.
    /// </summary>
    [XmlElement("workCenter")]
    public WorkCenterReference WorkCenter
    {
        get => _workCenter;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _workCenter,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_workCenter);

            SetProperty(
                ref _workCenter,
                value);

            SubscribeToObject(_workCenter);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the binary global work-center
    /// activation decisions.
    /// </summary>
    /// <remarks>
    /// Each value must be zero or one.
    ///
    /// An activation value of one indicates that the work
    /// center is globally available or used during the period.
    ///
    /// When no fixed work-center usage decision is required,
    /// the series may remain filled with zeros.
    /// </remarks>
    [XmlElement("activations")]
    public IntegerTimeSeries Activations
    {
        get => _activations;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _activations,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_activations);

            SetProperty(
                ref _activations,
                value);

            SubscribeToObject(_activations);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the global additional capacity used
    /// by the work center during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    ///
    /// The maximum available additional capacity belongs
    /// to the supply-chain instance and is not duplicated
    /// in this solution object.
    /// </remarks>
    [XmlElement("additionalCapacityUsed")]
    public DoubleTimeSeries AdditionalCapacityUsed
    {
        get => _additionalCapacityUsed;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _additionalCapacityUsed,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _additionalCapacityUsed);

            SetProperty(
                ref _additionalCapacityUsed,
                value);

            SubscribeToObject(
                _additionalCapacityUsed);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by the activation series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Activations.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether every decision series
    /// uses the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        AdditionalCapacityUsed.PeriodCount ==
            PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether the work-center
    /// reference is initialized.
    /// </summary>
    [XmlIgnore]
    public bool HasValidWorkCenterReference =>
        WorkCenter.PlantId > 0 &&
        WorkCenter.WorkCenterId > 0;

    /// <summary>
    /// Gets a value indicating whether every activation value
    /// is equal to zero or one.
    /// </summary>
    [XmlIgnore]
    public bool HasValidActivationValues =>
        Activations.All(
            activation =>
                activation is 0 or 1);

    /// <summary>
    /// Gets a value indicating whether every additional-capacity
    /// value is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidAdditionalCapacityValues =>
        AdditionalCapacityUsed.All(
            capacity =>
                double.IsFinite(capacity) &&
                capacity >= 0.0);

    /// <summary>
    /// Gets a value indicating whether the work-center
    /// capacity decision is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the referenced
    /// work center exists in a particular supply-chain
    /// instance or that the used additional capacity does
    /// not exceed the available amount.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        PlanningHorizon > 0 &&
        HasValidWorkCenterReference &&
        HasConsistentPlanningHorizon &&
        HasValidActivationValues &&
        HasValidAdditionalCapacityValues;

    /// <summary>
    /// Determines whether the work center is activated
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// True when the activation value is one;
    /// otherwise, false.
    /// </returns>
    public bool IsActivated(int period)
    {
        return Activations[period] == 1;
    }

    /// <summary>
    /// Sets the global work-center activation decision
    /// for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="isActivated">
    /// True to activate the work center;
    /// otherwise, false.
    /// </param>
    public void SetActivated(
        int period,
        bool isActivated)
    {
        Activations[period] =
            isActivated ? 1 : 0;
    }

    /// <summary>
    /// Gets the global additional capacity used during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Non-negative additional-capacity quantity.
    /// </returns>
    public double GetAdditionalCapacityUsed(
        int period)
    {
        return AdditionalCapacityUsed[period];
    }

    /// <summary>
    /// Sets the global additional capacity used during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="capacity">
    /// Finite and non-negative additional-capacity quantity.
    /// </param>
    public void SetAdditionalCapacityUsed(
        int period,
        double capacity)
    {
        if (!double.IsFinite(capacity) ||
            capacity < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                "The additional capacity must be finite " +
                "and non-negative.");
        }

        AdditionalCapacityUsed[period] =
            capacity;
    }

    /// <summary>
    /// Determines whether this decision refers to the
    /// specified work center.
    /// </summary>
    /// <param name="workCenter">
    /// Work-center reference to compare.
    /// </param>
    /// <returns>
    /// True when the plant and work-center identifiers
    /// match; otherwise, false.
    /// </returns>
    public bool Matches(
        WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        return WorkCenter.PlantId ==
                   workCenter.PlantId &&
               WorkCenter.WorkCenterId ==
                   workCenter.WorkCenterId;
    }

    /// <summary>
    /// Resizes every decision series to the specified
    /// planning horizon.
    /// </summary>
    /// <param name="periodCount">
    /// Non-negative number of planning periods.
    /// </param>
    /// <remarks>
    /// Existing values are preserved whenever possible.
    /// New periods are initialized with zero.
    /// </remarks>
    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The period count cannot be negative.");
        }

        Activations.Resize(
            periodCount,
            defaultValue: 0);

        AdditionalCapacityUsed.Resize(
            periodCount,
            defaultValue: 0.0);

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Resets every work-center capacity decision value
    /// to zero.
    /// </summary>
    public void Clear()
    {
        Activations.Fill(0);
        AdditionalCapacityUsed.Fill(0.0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        int activatedPeriodCount =
            Activations.Count(
                activation =>
                    activation == 1);

        double totalAdditionalCapacity =
            AdditionalCapacityUsed.Sum();

        return
            $"Work center {WorkCenter}: " +
            $"activated periods {activatedPeriodCount}; " +
            $"additional capacity " +
            $"{totalAdditionalCapacity}";
    }

    private void SubscribeToObject(
        ModelObject modelObject)
    {
        modelObject.PropertyChanged +=
            OnNestedPropertyChanged;
    }

    private void UnsubscribeFromObject(
        ModelObject modelObject)
    {
        modelObject.PropertyChanged -=
            OnNestedPropertyChanged;
    }

    private void OnNestedPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        NotifyDerivedProperties();
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(PlanningHorizon));

        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));

        OnPropertyChanged(
            nameof(HasValidWorkCenterReference));

        OnPropertyChanged(
            nameof(HasValidActivationValues));

        OnPropertyChanged(
            nameof(HasValidAdditionalCapacityValues));

        OnPropertyChanged(
            nameof(IsInternallyValid));
    }
}