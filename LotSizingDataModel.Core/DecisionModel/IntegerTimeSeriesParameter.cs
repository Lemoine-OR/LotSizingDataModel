using System;
using System.ComponentModel;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel;

/// <summary>
/// Base class for decision-model parameters represented by one
/// integer value for each planning period.
///
/// This technical base class centralizes:
/// - XML serialization;
/// - planning-horizon resizing;
/// - change notifications;
/// - value validation.
/// </summary>
[Serializable]
[XmlType(TypeName = "integerTimeSeriesParameter")]
public abstract class IntegerTimeSeriesParameter :
    ModelObject,
    IPlanningHorizonAware
{
    private IntegerTimeSeries _values = new();

    /// <summary>
    /// Initializes an empty period-dependent integer parameter.
    ///
    /// Derived concrete classes must expose a public
    /// parameterless constructor for XmlSerializer.
    /// </summary>
    protected IntegerTimeSeriesParameter()
    {
        // Subscribe to changes in the values time series
        SubscribeToValues(_values);
    }

    /// <summary>
    /// Initializes a period-dependent integer parameter.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultValue">
    /// Initial value assigned to every period.
    /// </param>
    protected IntegerTimeSeriesParameter(
        int planningHorizon,
        int defaultValue = 0)
        : this()  // Call default constructor to subscribe to values
    {
        // Validate that the planning horizon is non-negative
        if (planningHorizon < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon cannot be negative.");
        }

        // Validate the default value before applying it to all periods
        ValidateValue(defaultValue, nameof(defaultValue));

        // Resize the time series to the specified planning horizon
        Values.Resize(
            planningHorizon,
            defaultValue);
    }

    /// <summary>
    /// Gets or sets the parameter values for all planning periods.
    /// </summary>
    [XmlElement("values")]
    public IntegerTimeSeries Values
    {
        get => _values;
        set
        {
            // Ensure the time series is never null
            IntegerTimeSeries newValue =
                value ?? new IntegerTimeSeries();

            // Validate all values in the new time series
            ValidateSeries(newValue);

            // Avoid unnecessary updates if the reference is the same
            if (ReferenceEquals(_values, newValue))
            {
                return;
            }

            // Unsubscribe from the old time series
            UnsubscribeFromValues(_values);

            _values = newValue;

            // Subscribe to the new time series
            SubscribeToValues(_values);

            // Notify dependent properties
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(PlanningHorizon));
        }
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by this parameter.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Values.PeriodCount;

    /// <summary>
    /// Gets or sets the parameter value for a planning period.
    ///
    /// Planning periods are numbered from 1.
    /// </summary>
    [XmlIgnore]
    public int this[int period]
    {
        get => Values[period];
        set
        {
            // Validate the value before setting
            ValidateValue(value, nameof(value));
            Values[period] = value;
        }
    }

    /// <summary>
    /// Gets the parameter value for a planning period.
    /// </summary>
    public int GetValue(int period)
    {
        return this[period];
    }

    /// <summary>
    /// Sets the parameter value for a planning period.
    /// </summary>
    public void SetValue(
        int period,
        int value)
    {
        this[period] = value;
    }

    /// <summary>
    /// Assigns the same value to every planning period.
    /// </summary>
    public void Fill(int value)
    {
        // Validate the value before filling all periods
        ValidateValue(value, nameof(value));
        Values.Fill(value);
    }

    /// <summary>
    /// Resizes the time series when the planning horizon changes.
    ///
    /// Existing values are preserved. Newly created periods receive
    /// <see cref="DefaultValueForNewPeriods"/>.
    /// </summary>
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

        // Resize values with the default value for new periods
        Values.Resize(
            periodCount,
            DefaultValueForNewPeriods);
    }

    /// <summary>
    /// Gets the value assigned to newly created periods
    /// when the planning horizon grows.
    /// </summary>
    [XmlIgnore]
    protected virtual int DefaultValueForNewPeriods => 0;

    /// <summary>
    /// Validates one integer value.
    ///
    /// The base implementation accepts every integer.
    /// Derived classes can impose stricter rules.
    /// </summary>
    protected virtual void ValidateValue(
        int value,
        string parameterName)
    {
        // Every value of type int is intrinsically valid here.
        // Business-specific restrictions are implemented
        // by derived classes.
    }

    /// <summary>
    /// Validates every value contained in a time series.
    /// </summary>
    protected void ValidateSeries(
        IntegerTimeSeries values)
    {
        // Ensure the time series is not null
        ArgumentNullException.ThrowIfNull(values);

        // Validate each value in the time series
        foreach (int value in values)
        {
            ValidateValue(value, nameof(values));
        }
    }

    /// <summary>
    /// Subscribes to property change notifications from the values time series.
    /// </summary>
    private void SubscribeToValues(
        IntegerTimeSeries values)
    {
        // Listen to property changes in the time series
        values.PropertyChanged +=
            OnValuesPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from property change notifications from the values time series.
    /// </summary>
    private void UnsubscribeFromValues(
        IntegerTimeSeries values)
    {
        // Stop listening to property changes in the time series
        values.PropertyChanged -=
            OnValuesPropertyChanged;
    }

    /// <summary>
    /// Handles property change notifications from the values time series
    /// and propagates relevant changes to dependent properties.
    /// </summary>
    private void OnValuesPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        // Always notify that Values changed
        OnPropertyChanged(nameof(Values));

        // Notify dependent properties when period count or values change
        if (e.PropertyName ==
                nameof(IntegerTimeSeries.PeriodCount) ||
            e.PropertyName ==
                nameof(IntegerTimeSeries.Values))
        {
            OnPropertyChanged(nameof(PlanningHorizon));
        }
    }
}