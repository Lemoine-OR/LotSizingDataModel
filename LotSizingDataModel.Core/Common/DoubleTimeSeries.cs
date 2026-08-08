using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.Common;

/// <summary>
/// Represents a sequence of finite double values indexed by planning period.
///
/// Period numbers are one-based:
/// period 1 corresponds to the first value,
/// period 2 to the second value, and so on.
/// </summary>
[Serializable]
[XmlType(TypeName = "doubleTimeSeries")]
public sealed class DoubleTimeSeries :
    ModelObject,
    IEnumerable<double>
{
    private double[] _values = Array.Empty<double>();

    /// <summary>
    /// Initializes an empty time series.
    ///
    /// This constructor is required by XmlSerializer.
    /// </summary>
    public DoubleTimeSeries()
    {
    }

    /// <summary>
    /// Initializes a time series with the specified number of periods.
    /// </summary>
    /// <param name="periodCount">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultValue">
    /// Initial value assigned to every period.
    /// </param>
    public DoubleTimeSeries(
        int periodCount,
        double defaultValue = 0.0)
    {
        Resize(periodCount, defaultValue);
    }

    /// <summary>
    /// Gets or replaces all values of the time series.
    ///
    /// Each XML element corresponds to one planning period.
    /// The first value corresponds to period 1.
    /// </summary>
    [XmlElement("value")]
    public double[] Values
    {
        get => (double[])_values.Clone();
        set => ReplaceValues(value ?? Array.Empty<double>());
    }

    /// <summary>
    /// Gets the number of periods currently represented.
    /// </summary>
    [XmlIgnore]
    public int PeriodCount => _values.Length;

    /// <summary>
    /// Gets or sets the value for a planning period.
    ///
    /// The period number starts at 1, not 0.
    /// </summary>
    /// <param name="period">
    /// Planning period between 1 and <see cref="PeriodCount"/>.
    /// </param>
    [XmlIgnore]
    public double this[int period]
    {
        get
        {
            int index = ConvertPeriodToIndex(period);
            return _values[index];
        }
        set
        {
            ValidateFiniteValue(value, nameof(value));

            int index = ConvertPeriodToIndex(period);

            // Avoid unnecessary notifications if the value does not change
            if (_values[index].Equals(value))
            {
                return;
            }

            _values[index] = value;

            OnPropertyChanged(nameof(Values));
            OnPropertyChanged("Item[]");
        }
    }

    /// <summary>
    /// Resizes the time series.
    ///
    /// Existing values are preserved. When the horizon grows,
    /// new periods receive <paramref name="defaultValue"/>.
    /// When the horizon shrinks, values beyond the new horizon
    /// are removed.
    /// </summary>
    /// <param name="periodCount">
    /// New number of periods.
    /// </param>
    /// <param name="defaultValue">
    /// Value assigned to newly created periods.
    /// </param>
    public void Resize(
        int periodCount,
        double defaultValue = 0.0)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The number of periods cannot be negative.");
        }

        ValidateFiniteValue(defaultValue, nameof(defaultValue));

        // No change necessary if the size is the same
        if (periodCount == _values.Length)
        {
            return;
        }

        int previousPeriodCount = _values.Length;
        var resizedValues = new double[periodCount];

        // Copy existing values (the minimum between old and new size)
        int copiedValueCount = Math.Min(
            previousPeriodCount,
            periodCount);

        if (copiedValueCount > 0)
        {
            Array.Copy(
                _values,
                resizedValues,
                copiedValueCount);
        }

        // Initialize new periods with the default value when expanding
        if (periodCount > previousPeriodCount)
        {
            Array.Fill(
                resizedValues,
                defaultValue,
                previousPeriodCount,
                periodCount - previousPeriodCount);
        }

        _values = resizedValues;

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(PeriodCount));
        OnPropertyChanged("Item[]");
    }

    /// <summary>
    /// Assigns the same value to every planning period.
    /// </summary>
    public void Fill(double value)
    {
        ValidateFiniteValue(value, nameof(value));

        // Avoid unnecessary notifications if all values are already equal
        if (_values.All(currentValue => currentValue.Equals(value)))
        {
            return;
        }

        Array.Fill(_values, value);

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged("Item[]");
    }

    /// <summary>
    /// Gets the value associated with a planning period.
    /// </summary>
    public double GetValue(int period)
    {
        return this[period];
    }

    /// <summary>
    /// Sets the value associated with a planning period.
    /// </summary>
    public void SetValue(int period, double value)
    {
        this[period] = value;
    }

    /// <summary>
    /// Appends a value to the end of the time series.
    /// </summary>
    /// <param name="value">
    /// Finite value to append.
    /// </param>
    /// <remarks>
    /// This public method is required by <see cref="XmlSerializer"/>
    /// because this type implements <see cref="IEnumerable{T}"/>.
    /// During XML deserialization, the serializer calls this method
    /// once for each serialized period value.
    /// </remarks>
    public void Add(double value)
    {
        ValidateFiniteValue(value, nameof(value));

        int previousPeriodCount = _values.Length;

        Array.Resize(
            ref _values,
            previousPeriodCount + 1);

        _values[previousPeriodCount] = value;

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(PeriodCount));
        OnPropertyChanged("Item[]");
    }

    /// <summary>
    /// Removes all values from the time series.
    /// </summary>
    public void Clear()
    {
        if (_values.Length == 0)
        {
            return;
        }

        _values = Array.Empty<double>();

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(PeriodCount));
        OnPropertyChanged("Item[]");
    }

    /// <summary>
    /// Creates an independent copy of this time series.
    /// </summary>
    public DoubleTimeSeries Clone()
    {
        return new DoubleTimeSeries
        {
            Values = Values
        };
    }

    /// <summary>
    /// Returns an enumerator over the period values.
    /// </summary>
    public IEnumerator<double> GetEnumerator()
    {
        return ((IEnumerable<double>)_values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Replaces the internal values of the time series with a new array.
    /// Validates all values before assignment.
    /// </summary>
    /// <param name="values">The new array of values to assign.</param>
    private void ReplaceValues(double[] values)
    {
        // Validate that all values are finite (neither NaN nor Infinity)
        foreach (double value in values)
        {
            ValidateFiniteValue(value, nameof(values));
        }

        // Avoid unnecessary notifications if the values are identical
        if (_values.SequenceEqual(values))
        {
            return;
        }

        // Detect if the period count changes to notify PropertyChanged
        bool periodCountChanged =
            _values.Length != values.Length;

        // Clone the array to ensure encapsulation
        _values = (double[])values.Clone();

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged("Item[]");

        if (periodCountChanged)
        {
            OnPropertyChanged(nameof(PeriodCount));
        }
    }

    /// <summary>
    /// Converts a period number (1-based) to an array index (0-based).
    /// </summary>
    /// <param name="period">The period number (starts at 1).</param>
    /// <returns>The corresponding index in the array (starts at 0).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the period is outside the valid range [1, PeriodCount].
    /// </exception>
    private int ConvertPeriodToIndex(int period)
    {
        if (period < 1 || period > _values.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(period),
                period,
                $"The period must be between 1 and {_values.Length}.");
        }

        // Conversion: period 1 -> index 0, period 2 -> index 1, etc.
        return period - 1;
    }

    /// <summary>
    /// Validates that a double value is finite (neither NaN nor infinite).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name for the error message.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the value is NaN or infinite.
    /// </exception>
    private static void ValidateFiniteValue(
        double value,
        string parameterName)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A time-series value must be a finite number.");
        }
    }
}