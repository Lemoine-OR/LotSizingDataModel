using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the global capacity decisions associated with one
/// transport resource over the complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// Item-specific transported quantities, setup decisions and
/// additional-capacity decisions are stored separately in
/// transport decisions.
/// </remarks>
[Serializable]
[XmlType(TypeName = "transportResourceCapacityDecision")]
public sealed class TransportResourceCapacityDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _transportResourceId;

    private IntegerTimeSeries _activations =
        new();

    private DoubleTimeSeries _additionalCapacityUsed =
        new();

    /// <summary>
    /// Initializes an empty transport-resource capacity decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public TransportResourceCapacityDecision()
    {
        SubscribeToObject(_activations);
        SubscribeToObject(_additionalCapacityUsed);
    }

    /// <summary>
    /// Initializes a capacity decision for a transport resource
    /// and a planning horizon.
    /// </summary>
    /// <param name="transportResourceId">
    /// Identifier of the transport resource.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public TransportResourceCapacityDecision(
        int transportResourceId,
        int planningHorizon)
        : this()
    {
        if (transportResourceId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(transportResourceId),
                transportResourceId,
                "The transport-resource identifier must be " +
                "strictly positive.");
        }

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        TransportResourceId =
            transportResourceId;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the transport resource
    /// associated with the capacity decision.
    /// </summary>
    [XmlAttribute("transportResourceId")]
    public int TransportResourceId
    {
        get => _transportResourceId;
        set
        {
            /*
             * Zero is tolerated for an empty object created
             * by XmlSerializer. The solution validator will
             * require a strictly positive identifier.
             */
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The transport-resource identifier " +
                    "cannot be negative.");
            }

            SetProperty(
                ref _transportResourceId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the binary global transport-resource
    /// activation decisions.
    /// </summary>
    /// <remarks>
    /// Each value must be zero or one.
    ///
    /// When the supply-chain model does not require a fixed
    /// transport-resource activation decision, this series
    /// may remain filled with zeros.
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
    /// by the transport resource during each planning period.
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
    /// Gets a value indicating whether the transport-resource
    /// capacity decision is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the transport resource
    /// exists in a particular supply-chain instance or that the
    /// additional capacity used remains below the available amount.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        TransportResourceId > 0 &&
        PlanningHorizon > 0 &&
        HasConsistentPlanningHorizon &&
        HasValidActivationValues &&
        HasValidAdditionalCapacityValues;

    /// <summary>
    /// Determines whether the transport resource is activated
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
    /// Sets the global transport-resource activation decision
    /// for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="isActivated">
    /// True to activate the transport resource;
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
    /// Gets the global additional transport capacity used
    /// during a planning period.
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
    /// Sets the global additional transport capacity used
    /// during a planning period.
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
    /// specified transport resource.
    /// </summary>
    /// <param name="transportResourceId">
    /// Transport-resource identifier to compare.
    /// </param>
    /// <returns>
    /// True when the identifiers match; otherwise, false.
    /// </returns>
    public bool Matches(
        int transportResourceId)
    {
        return TransportResourceId ==
            transportResourceId;
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
    /// Resets every transport-resource capacity decision value
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
            $"Transport resource {TransportResourceId}: " +
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
            nameof(HasValidActivationValues));

        OnPropertyChanged(
            nameof(HasValidAdditionalCapacityValues));

        OnPropertyChanged(
            nameof(IsInternallyValid));
    }
}