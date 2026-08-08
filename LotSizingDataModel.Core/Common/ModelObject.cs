using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LotSizingDataModel.Core.Common;

/// <summary>
/// Base class for model objects that notify listeners when one of their
/// properties changes.
///
/// This class is compatible with XML serialization. Events are not serialized.
/// </summary>
[Serializable]
public abstract class ModelObject : INotifyPropertyChanged
{
    /// <summary>
    /// Raised when the value of a property changes.
    /// </summary>
    [field: NonSerialized]
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Assigns a new value to a backing field and raises
    /// <see cref="PropertyChanged"/> when the value has actually changed.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="field">Backing field of the property.</param>
    /// <param name="value">New value.</param>
    /// <param name="propertyName">
    /// Name of the modified property. It is normally supplied automatically
    /// by the compiler.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the value has changed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        // Avoid unnecessary notifications if the value hasn't changed
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        // Update the backing field and notify listeners
        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the modified property.</param>
    protected virtual void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        // Invoke the event if there are any subscribers
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}