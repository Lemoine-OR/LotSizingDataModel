using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.Common;

/// <summary>
/// Represents a sequence of integer values indexed by planning period.
///
/// Period numbers are one-based:
/// period 1 corresponds to the first value,
/// period 2 to the second value, and so on.
/// </summary>
[Serializable]
[XmlType(TypeName = "integerTimeSeries")]
public sealed class IntegerTimeSeries :
    ModelObject,
    IEnumerable<int>
{
    private int[] _values = Array.Empty<int>();

    /// <summary>
    /// Initializes an empty time series.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public IntegerTimeSeries()
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
    public IntegerTimeSeries(
        int periodCount,
        int defaultValue = 0)
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
    public int[] Values
    {
        get => (int[])_values.Clone();
        set => ReplaceValues(value ?? Array.Empty<int>());
    }

    /// <summary>
    /// Gets the number of planning periods represented
    /// by this time series.
    /// </summary>
    [XmlIgnore]
    public int PeriodCount => _values.Length;

    /// <summary>
    /// Gets or sets the value associated with a planning period.
    ///
    /// Period numbering starts at 1.
    /// </summary>
    /// <param name="period">
    /// Planning period between 1 and <see cref="PeriodCount"/>.
    /// </param>
    [XmlIgnore]
    public int this[int period]
    {
        get
        {
            int index = ConvertPeriodToIndex(period);
            return _values[index];
        }
        set
        {
            int index = ConvertPeriodToIndex(period);

            // Avoid unnecessary notifications if the value does not change
            if (_values[index] == value)
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
    /// Existing values are preserved. If the horizon grows,
    /// newly created periods receive <paramref name="defaultValue"/>.
    /// If the horizon shrinks, values outside the new horizon
    /// are discarded.
    /// </summary>
    /// <param name="periodCount">
    /// New number of planning periods.
    /// </param>
    /// <param name="defaultValue">
    /// Value assigned to newly created periods.
    /// </param>
    public void Resize(
        int periodCount,
        int defaultValue = 0)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The number of periods cannot be negative.");
        }

        // No change necessary if the size is the same
        if (periodCount == _values.Length)
        {
            return;
        }

        int previousPeriodCount = _values.Length;
        var resizedValues = new int[periodCount];

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
    /// <param name="value">
    /// Value assigned to all periods.
    /// </param>
    public void Fill(int value)
    {
        // Avoid unnecessary notifications if all values are already equal
        if (_values.All(currentValue => currentValue == value))
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
    public int GetValue(int period)
    {
        return this[period];
    }

    /// <summary>
    /// Sets the value associated with a planning period.
    /// </summary>
    public void SetValue(int period, int value)
    {
        this[period] = value;
    }

    /// <summary>
    /// Creates an independent copy of this time series.
    /// </summary>
    public IntegerTimeSeries Clone()
    {
        return new IntegerTimeSeries
        {
            Values = Values
        };
    }

    /// <summary>
    /// Adds a value to the end of the time series.
    /// </summary>
    /// <param name="value">
    /// Value to append.
    /// </param>
    /// <remarks>
    /// This public method is required by
    /// <see cref="XmlSerializer"/> because this type implements
    /// <see cref="IEnumerable{T}"/>.
    /// </remarks>
    public void Add(
        int value)
    {
        int previousLength =
            _values.Length;

        Array.Resize(
            ref _values,
            previousLength + 1);

        _values[previousLength] =
            value;

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

        _values =
            Array.Empty<int>();

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(PeriodCount));
        OnPropertyChanged("Item[]");
    }

    /// <summary>
    /// Returns an enumerator over the period values.
    /// </summary>
    public IEnumerator<int> GetEnumerator()
    {
        return ((IEnumerable<int>)_values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Replaces the internal values of the time series with a new array.
    /// </summary>
    /// <param name="values">The new array of values to assign.</param>
    private void ReplaceValues(int[] values)
    {
        // Avoid unnecessary notifications if the values are identical
        if (_values.SequenceEqual(values))
        {
            return;
        }

        // Detect if the period count changes to notify PropertyChanged
        bool periodCountChanged =
            _values.Length != values.Length;

        // Clone the array to ensure encapsulation
        _values = (int[])values.Clone();

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
}