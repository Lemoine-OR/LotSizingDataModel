using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel;

/// <summary>
/// Base class for decision-model parameters represented by one
/// double value for each planning period.
///
/// This technical base class centralizes:
/// - XML serialization;
/// - planning-horizon resizing;
/// - change notifications;
/// - value validation.
/// </summary>
[Serializable]
[XmlType(TypeName = "doubleTimeSeriesParameter")]
public abstract class DoubleTimeSeriesParameter :
    ModelObject,
    IPlanningHorizonAware
{
    private DoubleTimeSeries _values = new();

    /// <summary>
    /// Initializes an empty period-dependent parameter.
    ///
    /// Derived concrete classes must expose a public
    /// parameterless constructor for XmlSerializer.
    /// </summary>
    protected DoubleTimeSeriesParameter()
    {
        // Subscribe to changes in the values time series
        SubscribeToValues(_values);
    }

    /// <summary>
    /// Initializes a period-dependent parameter with the specified
    /// planning horizon.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultValue">
    /// Initial value assigned to every period.
    /// </param>
    protected DoubleTimeSeriesParameter(
        int planningHorizon,
        double defaultValue = 0.0)
        : this()  // Call default constructor to subscribe to values
    {
        // Validate the default value before applying it to all periods
        ValidateValue(defaultValue, nameof(defaultValue));

        // Resize the time series to the specified planning horizon
        Values.Resize(
            planningHorizon,
            defaultValue);
    }

    /// <summary>
    /// Gets or sets the values of the parameter for all
    /// planning periods.
    ///
    /// The value at position t corresponds to the parameter
    /// value for planning period t.
    /// </summary>
    [XmlElement("values")]
    public DoubleTimeSeries Values
    {
        get => _values;
        set
        {
            // Ensure the time series is never null
            DoubleTimeSeries newValue =
                value ?? new DoubleTimeSeries();

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
    /// Gets the number of planning periods currently represented
    /// by this parameter.
    ///
    /// This value is calculated from the time series and is not
    /// serialized separately.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        Values.PeriodCount;

    /// <summary>
    /// Gets or sets the parameter value for a planning period.
    ///
    /// Period numbering starts at 1.
    /// </summary>
    /// <param name="period">
    /// Planning period between 1 and <see cref="PlanningHorizon"/>.
    /// </param>
    [XmlIgnore]
    public double this[int period]
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
    public double GetValue(int period)
    {
        return this[period];
    }

    /// <summary>
    /// Sets the parameter value for a planning period.
    /// </summary>
    public void SetValue(
        int period,
        double value)
    {
        this[period] = value;
    }

    /// <summary>
    /// Assigns the same value to every planning period.
    /// </summary>
    public void Fill(double value)
    {
        // Validate the value before filling all periods
        ValidateValue(value, nameof(value));
        Values.Fill(value);
    }

    /// <summary>
    /// Resizes the time series when the planning horizon changes.
    ///
    /// Existing values are preserved and newly created periods
    /// receive <see cref="DefaultValueForNewPeriods"/>.
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
    /// Gets the value assigned to periods created when
    /// the planning horizon grows.
    ///
    /// Derived classes can override this property when another
    /// default value is more appropriate.
    /// </summary>
    [XmlIgnore]
    protected virtual double DefaultValueForNewPeriods => 0.0;

    /// <summary>
    /// Validates one value before it is assigned to the parameter.
    ///
    /// The default implementation accepts every finite value.
    /// Derived classes can impose additional constraints,
    /// such as non-negativity.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="parameterName">
    /// Name used when an exception is raised.
    /// </param>
    protected virtual void ValidateValue(
        double value,
        string parameterName)
    {
        // Check if the value is finite (not NaN or Infinity)
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A period-dependent value must be finite.");
        }
    }

    /// <summary>
    /// Validates all values contained in a time series.
    /// </summary>
    protected void ValidateSeries(
        DoubleTimeSeries values)
    {
        // Ensure the time series is not null
        ArgumentNullException.ThrowIfNull(values);

        // Validate each value in the time series
        foreach (double value in values)
        {
            ValidateValue(value, nameof(values));
        }
    }

    /// <summary>
    /// Subscribes to property change notifications from the values time series.
    /// </summary>
    private void SubscribeToValues(
        DoubleTimeSeries values)
    {
        // Listen to property changes in the time series
        values.PropertyChanged +=
            OnValuesPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from property change notifications from the values time series.
    /// </summary>
    private void UnsubscribeFromValues(
        DoubleTimeSeries values)
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
                nameof(DoubleTimeSeries.PeriodCount) ||
            e.PropertyName ==
                nameof(DoubleTimeSeries.Values))
        {
            OnPropertyChanged(nameof(PlanningHorizon));
        }
    }
}