using System.Globalization;
using System.Reflection;

namespace LotSizingDataModel.Checker.Tests.Infrastructure;

internal static class ReflectionMutation
{
    public static void SetScalarProperty(
        object target,
        string propertyName,
        object? value)
    {
        ArgumentNullException.ThrowIfNull(target);

        PropertyInfo property =
            target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found on " +
                $"{target.GetType().FullName}.");

        if (!property.CanWrite)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' is read-only.");
        }

        property.SetValue(
            target,
            ConvertValue(
                value,
                property.PropertyType));
    }

    public static double GetFirstNumericSeriesValue(
        object owner,
        string propertyName)
    {
        Array values =
            GetSeriesValues(owner, propertyName, out _);

        if (values.Length == 0)
        {
            throw new InvalidOperationException(
                $"Series '{propertyName}' is empty.");
        }

        return Convert.ToDouble(
            values.GetValue(0),
            CultureInfo.InvariantCulture);
    }

    public static void SetFirstNumericSeriesValue(
        object owner,
        string propertyName,
        double numericValue)
    {
        Array values =
            GetSeriesValues(
                owner,
                propertyName,
                out PropertyInfo valuesProperty);

        if (values.Length == 0)
        {
            throw new InvalidOperationException(
                $"Series '{propertyName}' is empty.");
        }

        Array updated =
            (Array)values.Clone();

        Type elementType =
            updated.GetType().GetElementType() ??
            throw new InvalidOperationException(
                $"Series '{propertyName}' has no element type.");

        updated.SetValue(
            Convert.ChangeType(
                numericValue,
                elementType,
                CultureInfo.InvariantCulture),
            0);

        valuesProperty.SetValue(
            GetSeriesObject(owner, propertyName),
            updated);
    }

    private static Array GetSeriesValues(
        object owner,
        string propertyName,
        out PropertyInfo valuesProperty)
    {
        object series =
            GetSeriesObject(owner, propertyName);

        valuesProperty =
            series.GetType().GetProperty(
                "Values",
                BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"Series type '{series.GetType().FullName}' has no public Values property.");

        return valuesProperty.GetValue(series) as Array ??
            throw new InvalidOperationException(
                $"Series '{propertyName}' did not expose an array through Values.");
    }

    private static object GetSeriesObject(
        object owner,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(owner);

        PropertyInfo property =
            owner.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found on " +
                $"{owner.GetType().FullName}.");

        return property.GetValue(owner) ??
            throw new InvalidOperationException(
                $"Property '{propertyName}' is null.");
    }

    private static object? ConvertValue(
        object? value,
        Type targetType)
    {
        Type nonNullableType =
            Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is null)
        {
            return null;
        }

        if (nonNullableType.IsInstanceOfType(value))
        {
            return value;
        }

        return Convert.ChangeType(
            value,
            nonNullableType,
            CultureInfo.InvariantCulture);
    }
}
