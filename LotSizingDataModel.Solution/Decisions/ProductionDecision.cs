using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Stores the production decisions associated with one
/// production routing over the complete planning horizon.
/// </summary>
/// <remarks>
/// Period numbers are one-based.
///
/// The decision may originate from an exact optimization method,
/// a heuristic, a metaheuristic, a manual plan or an imported file.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productionDecision")]
public sealed class ProductionDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _routingId;

    private DoubleTimeSeries _quantities =
        new();

    private IntegerTimeSeries _setups =
        new();

    private IntegerTimeSeries _lotMultipleCounts =
        new();

    /// <summary>
    /// Initializes an empty production decision.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public ProductionDecision()
    {
        SubscribeToSeries(_quantities);
        SubscribeToSeries(_setups);
        SubscribeToSeries(_lotMultipleCounts);
    }

    /// <summary>
    /// Initializes a production decision for a routing
    /// and a planning horizon.
    /// </summary>
    /// <param name="routingId">
    /// Identifier of the production routing.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public ProductionDecision(
        int routingId,
        int planningHorizon)
        : this()
    {
        if (routingId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(routingId),
                routingId,
                "The routing identifier must be strictly positive.");
        }

        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        RoutingId = routingId;

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Gets or sets the identifier of the production routing
    /// associated with the decision.
    /// </summary>
    [XmlAttribute("routingId")]
    public int RoutingId
    {
        get => _routingId;
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
                    "The routing identifier cannot be negative.");
            }

            SetProperty(
                ref _routingId,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the quantities produced through the routing.
    /// </summary>
    /// <remarks>
    /// Values must be finite and non-negative.
    /// </remarks>
    [XmlElement("quantities")]
    public DoubleTimeSeries Quantities
    {
        get => _quantities;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _quantities,
                    value))
            {
                return;
            }

            UnsubscribeFromSeries(_quantities);

            SetProperty(
                ref _quantities,
                value);

            SubscribeToSeries(_quantities);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the binary routing-activation decisions.
    /// </summary>
    /// <remarks>
    /// Each value must be zero or one.
    /// </remarks>
    [XmlElement("setups")]
    public IntegerTimeSeries Setups
    {
        get => _setups;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _setups,
                    value))
            {
                return;
            }

            UnsubscribeFromSeries(_setups);

            SetProperty(
                ref _setups,
                value);

            SubscribeToSeries(_setups);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets or sets the number of lot-size multiples
    /// used during each planning period.
    /// </summary>
    /// <remarks>
    /// Values must be non-negative integers.
    ///
    /// When no multiple-of-lot constraint applies to the routing,
    /// the series may remain filled with zeros.
    /// </remarks>
    [XmlElement("lotMultipleCounts")]
    public IntegerTimeSeries LotMultipleCounts
    {
        get => _lotMultipleCounts;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _lotMultipleCounts,
                    value))
            {
                return;
            }

            UnsubscribeFromSeries(
                _lotMultipleCounts);

            SetProperty(
                ref _lotMultipleCounts,
                value);

            SubscribeToSeries(
                _lotMultipleCounts);

            NotifyDerivedProperties();
        }
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by the production-quantity series.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Quantities.PeriodCount;

    /// <summary>
    /// Gets a value indicating whether all decision series
    /// use the same planning horizon.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        Setups.PeriodCount ==
            PlanningHorizon &&
        LotMultipleCounts.PeriodCount ==
            PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether every production
    /// quantity is finite and non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidQuantities =>
        Quantities.All(
            quantity =>
                double.IsFinite(quantity) &&
                quantity >= 0.0);

    /// <summary>
    /// Gets a value indicating whether every setup value
    /// is equal to zero or one.
    /// </summary>
    [XmlIgnore]
    public bool HasValidSetupValues =>
        Setups.All(
            setup =>
                setup is 0 or 1);

    /// <summary>
    /// Gets a value indicating whether every lot-multiple
    /// count is non-negative.
    /// </summary>
    [XmlIgnore]
    public bool HasValidLotMultipleCounts =>
        LotMultipleCounts.All(
            count => count >= 0);

    /// <summary>
    /// Gets a value indicating whether the production decision
    /// is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify that the referenced routing
    /// exists in a particular supply-chain instance.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        RoutingId > 0 &&
        PlanningHorizon > 0 &&
        HasConsistentPlanningHorizon &&
        HasValidQuantities &&
        HasValidSetupValues &&
        HasValidLotMultipleCounts;

    /// <summary>
    /// Gets the production quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Production quantity recorded for the period.
    /// </returns>
    public double GetQuantity(int period)
    {
        return Quantities[period];
    }

    /// <summary>
    /// Sets the production quantity for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="quantity">
    /// Finite and non-negative production quantity.
    /// </param>
    public void SetQuantity(
        int period,
        double quantity)
    {
        if (!double.IsFinite(quantity) ||
            quantity < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "The production quantity must be finite " +
                "and non-negative.");
        }

        Quantities[period] = quantity;
    }

    /// <summary>
    /// Determines whether production is activated during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// True when the setup value is one; otherwise, false.
    /// </returns>
    public bool IsSetupActivated(int period)
    {
        return Setups[period] == 1;
    }

    /// <summary>
    /// Sets the production-activation decision for a period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="isActivated">
    /// True to activate production; otherwise, false.
    /// </param>
    public void SetSetupActivated(
        int period,
        bool isActivated)
    {
        Setups[period] =
            isActivated ? 1 : 0;
    }

    /// <summary>
    /// Gets the number of lot-size multiples used during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Non-negative number of lot-size multiples.
    /// </returns>
    public int GetLotMultipleCount(int period)
    {
        return LotMultipleCounts[period];
    }

    /// <summary>
    /// Sets the number of lot-size multiples used during
    /// a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <param name="count">
    /// Non-negative integer count.
    /// </param>
    public void SetLotMultipleCount(
        int period,
        int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                "The lot-multiple count cannot be negative.");
        }

        LotMultipleCounts[period] = count;
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

        Quantities.Resize(
            periodCount,
            defaultValue: 0.0);

        Setups.Resize(
            periodCount,
            defaultValue: 0);

        LotMultipleCounts.Resize(
            periodCount,
            defaultValue: 0);

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Resets every production decision value to zero.
    /// </summary>
    public void Clear()
    {
        Quantities.Fill(0.0);
        Setups.Fill(0);
        LotMultipleCounts.Fill(0);

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        double totalQuantity =
            Quantities.Sum();

        int activatedPeriodCount =
            Setups.Count(
                setup => setup == 1);

        return
            $"Routing {RoutingId}: " +
            $"total quantity {totalQuantity}; " +
            $"activated periods {activatedPeriodCount}";
    }

    private void SubscribeToSeries(
        ModelObject series)
    {
        series.PropertyChanged +=
            OnSeriesPropertyChanged;
    }

    private void UnsubscribeFromSeries(
        ModelObject series)
    {
        series.PropertyChanged -=
            OnSeriesPropertyChanged;
    }

    private void OnSeriesPropertyChanged(
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
            nameof(HasValidQuantities));

        OnPropertyChanged(
            nameof(HasValidSetupValues));

        OnPropertyChanged(
            nameof(HasValidLotMultipleCounts));

        OnPropertyChanged(
            nameof(IsInternallyValid));
    }
}