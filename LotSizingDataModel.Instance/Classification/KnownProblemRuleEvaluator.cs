using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Evaluates known-problem classification rules against
/// extracted lot-sizing problem features.
/// </summary>
/// <remarks>
/// The evaluator reads feature properties dynamically from
/// <see cref="LotSizingProblemFeatures"/>.
///
/// Feature lookup is case-insensitive. Consequently,
/// <c>itemCount</c>, <c>ItemCount</c> and <c>ITEMCOUNT</c>
/// identify the same feature.
/// </remarks>
public static class KnownProblemRuleEvaluator
{
    private static readonly IReadOnlyDictionary<
        string,
        PropertyInfo> FeatureProperties =
            BuildFeaturePropertyIndex();

    /// <summary>
    /// Evaluates one classification rule against a
    /// lot-sizing problem-feature profile.
    /// </summary>
    /// <param name="rule">
    /// Rule definition to evaluate.
    /// </param>
    /// <param name="features">
    /// Extracted lot-sizing problem features.
    /// </param>
    /// <param name="isRequired">
    /// Value indicating whether this rule is mandatory for
    /// the problem-family match currently being evaluated.
    /// </param>
    /// <returns>
    /// Evidence containing the expected value, observed value
    /// and evaluation result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rule"/> or
    /// <paramref name="features"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the rule is disabled, invalid, references
    /// an unknown feature or cannot be evaluated using the
    /// selected operator.
    /// </exception>
    public static ClassificationEvidence Evaluate(
        KnownProblemRuleDefinition rule,
        LotSizingProblemFeatures features,
        bool isRequired = true)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(features);

        EnsureRuleCanBeEvaluated(rule);

        PropertyInfo featureProperty =
            GetFeatureProperty(rule.FeatureCode);

        object? observedValue;

        try
        {
            observedValue =
                featureProperty.GetValue(features);
        }
        catch (TargetInvocationException exception)
        {
            throw new InvalidOperationException(
                $"Feature '{rule.FeatureCode}' could not " +
                "be read.",
                exception.InnerException ?? exception);
        }
        catch (Exception exception)
            when (exception is MethodAccessException or
                  TargetException)
        {
            throw new InvalidOperationException(
                $"Feature '{rule.FeatureCode}' could not " +
                "be read.",
                exception);
        }

        bool isSatisfied =
            EvaluateCondition(
                rule,
                observedValue,
                featureProperty.PropertyType);

        var evidence =
            new ClassificationEvidence(
                featureCode:
                    featureProperty.Name,

                expectedValue:
                    GetDisplayedExpectedValue(rule),

                observedValue:
                    FormatValue(observedValue),

                isSatisfied:
                    isSatisfied,

                isRequired:
                    isRequired,

                description:
                    rule.Description)
            {
                RuleCode =
                    rule.RuleCode,

                Weight =
                    rule.Weight
            };

        return evidence;
    }

    /// <summary>
    /// Attempts to evaluate one classification rule without
    /// propagating configuration or conversion exceptions.
    /// </summary>
    /// <param name="rule">
    /// Rule definition to evaluate.
    /// </param>
    /// <param name="features">
    /// Extracted lot-sizing problem features.
    /// </param>
    /// <param name="isRequired">
    /// Value indicating whether this rule is mandatory for
    /// the current problem-family match.
    /// </param>
    /// <param name="evidence">
    /// Evidence produced when evaluation succeeds.
    /// </param>
    /// <param name="errorMessage">
    /// Error message produced when evaluation fails.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the rule was evaluated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryEvaluate(
        KnownProblemRuleDefinition rule,
        LotSizingProblemFeatures features,
        bool isRequired,
        out ClassificationEvidence? evidence,
        out string errorMessage)
    {
        try
        {
            evidence =
                Evaluate(
                    rule,
                    features,
                    isRequired);

            errorMessage =
                string.Empty;

            return true;
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  InvalidOperationException or
                  NotSupportedException or
                  FormatException or
                  OverflowException)
        {
            evidence =
                null;

            errorMessage =
                exception.Message;

            return false;
        }
    }

    /// <summary>
    /// Determines whether a feature code identifies a
    /// readable property of
    /// <see cref="LotSizingProblemFeatures"/>.
    /// </summary>
    /// <param name="featureCode">
    /// Feature code to examine.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the feature exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsKnownFeatureCode(
        string featureCode)
    {
        if (string.IsNullOrWhiteSpace(featureCode))
        {
            return false;
        }

        return FeatureProperties.ContainsKey(
            featureCode.Trim());
    }

    /// <summary>
    /// Returns all feature codes supported by the standard
    /// rule evaluator.
    /// </summary>
    /// <returns>
    /// Ordered collection of readable feature-property names.
    /// </returns>
    public static IReadOnlyList<string>
        GetKnownFeatureCodes()
    {
        return FeatureProperties
            .Values
            .Select(
                property =>
                    property.Name)
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                propertyName =>
                    propertyName,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void EnsureRuleCanBeEvaluated(
        KnownProblemRuleDefinition rule)
    {
        if (!rule.IsEnabled)
        {
            throw new InvalidOperationException(
                $"Classification rule '{DisplayRuleCode(rule)}' " +
                "is disabled.");
        }

        if (!rule.IsValidDefinition)
        {
            throw new InvalidOperationException(
                $"Classification rule '{DisplayRuleCode(rule)}' " +
                "is incomplete or invalid.");
        }

        if (!IsKnownFeatureCode(rule.FeatureCode))
        {
            throw new InvalidOperationException(
                $"Classification rule '{DisplayRuleCode(rule)}' " +
                $"references unknown feature " +
                $"'{rule.FeatureCode}'.");
        }
    }

    private static PropertyInfo GetFeatureProperty(
        string featureCode)
    {
        string normalizedFeatureCode =
            featureCode.Trim();

        if (FeatureProperties.TryGetValue(
                normalizedFeatureCode,
                out PropertyInfo? property))
        {
            return property;
        }

        throw new InvalidOperationException(
            $"Unknown lot-sizing problem feature " +
            $"'{featureCode}'.");
    }

    private static bool EvaluateCondition(
        KnownProblemRuleDefinition rule,
        object? observedValue,
        Type featureType)
    {
        string operatorCode =
            rule.OperatorCode;

        return operatorCode switch
        {
            KnownProblemRuleDefinition.IsTrueOperator =>
                EvaluateBooleanCondition(
                    observedValue,
                    expectedValue: true),

            KnownProblemRuleDefinition.IsFalseOperator =>
                EvaluateBooleanCondition(
                    observedValue,
                    expectedValue: false),

            KnownProblemRuleDefinition.EqualsOperator =>
                AreEqual(
                    observedValue,
                    featureType,
                    rule.ExpectedValue),

            KnownProblemRuleDefinition.NotEqualsOperator =>
                !AreEqual(
                    observedValue,
                    featureType,
                    rule.ExpectedValue),

            KnownProblemRuleDefinition.GreaterThanOperator =>
                Compare(
                    observedValue,
                    featureType,
                    rule.ExpectedValue) > 0,

            KnownProblemRuleDefinition
                .GreaterThanOrEqualOperator =>
                    Compare(
                        observedValue,
                        featureType,
                        rule.ExpectedValue) >= 0,

            KnownProblemRuleDefinition.LessThanOperator =>
                Compare(
                    observedValue,
                    featureType,
                    rule.ExpectedValue) < 0,

            KnownProblemRuleDefinition
                .LessThanOrEqualOperator =>
                    Compare(
                        observedValue,
                        featureType,
                        rule.ExpectedValue) <= 0,

            KnownProblemRuleDefinition.ContainsOperator =>
                Contains(
                    observedValue,
                    featureType,
                    rule.ExpectedValue),

            KnownProblemRuleDefinition.NotContainsOperator =>
                !Contains(
                    observedValue,
                    featureType,
                    rule.ExpectedValue),

            KnownProblemRuleDefinition.InOperator =>
                IsInExpectedValues(
                    observedValue,
                    featureType,
                    rule.ExpectedValue),

            KnownProblemRuleDefinition.NotInOperator =>
                !IsInExpectedValues(
                    observedValue,
                    featureType,
                    rule.ExpectedValue),

            _ =>
                throw new NotSupportedException(
                    $"Operator '{operatorCode}' is not " +
                    "supported by the standard evaluator.")
        };
    }

    private static bool EvaluateBooleanCondition(
        object? observedValue,
        bool expectedValue)
    {
        if (observedValue is not bool observedBoolean)
        {
            throw new InvalidOperationException(
                "The IS_TRUE and IS_FALSE operators can only " +
                "be applied to Boolean features.");
        }

        return observedBoolean ==
               expectedValue;
    }

    private static bool AreEqual(
        object? observedValue,
        Type featureType,
        string expectedText)
    {
        if (observedValue is null)
        {
            return IsNullLiteral(expectedText);
        }

        if (IsNullLiteral(expectedText))
        {
            return false;
        }

        Type effectiveType =
            GetEffectiveType(featureType);

        object expectedValue =
            ConvertTextToType(
                expectedText,
                effectiveType);

        if (effectiveType == typeof(string))
        {
            return string.Equals(
                Convert.ToString(
                    observedValue,
                    CultureInfo.InvariantCulture),
                Convert.ToString(
                    expectedValue,
                    CultureInfo.InvariantCulture),
                StringComparison.OrdinalIgnoreCase);
        }

        if (IsNumericType(effectiveType))
        {
            double observedNumber =
                Convert.ToDouble(
                    observedValue,
                    CultureInfo.InvariantCulture);

            double expectedNumber =
                Convert.ToDouble(
                    expectedValue,
                    CultureInfo.InvariantCulture);

            return observedNumber.Equals(
                expectedNumber);
        }

        return Equals(
            observedValue,
            expectedValue);
    }

    private static int Compare(
        object? observedValue,
        Type featureType,
        string expectedText)
    {
        if (observedValue is null)
        {
            throw new InvalidOperationException(
                "A null feature value cannot be used with an " +
                "ordered comparison operator.");
        }

        Type effectiveType =
            GetEffectiveType(featureType);

        object expectedValue =
            ConvertTextToType(
                expectedText,
                effectiveType);

        if (IsNumericType(effectiveType))
        {
            double observedNumber =
                Convert.ToDouble(
                    observedValue,
                    CultureInfo.InvariantCulture);

            double expectedNumber =
                Convert.ToDouble(
                    expectedValue,
                    CultureInfo.InvariantCulture);

            return observedNumber.CompareTo(
                expectedNumber);
        }

        if (effectiveType == typeof(string))
        {
            return string.Compare(
                Convert.ToString(
                    observedValue,
                    CultureInfo.InvariantCulture),
                Convert.ToString(
                    expectedValue,
                    CultureInfo.InvariantCulture),
                StringComparison.OrdinalIgnoreCase);
        }

        if (observedValue is IComparable comparable)
        {
            return comparable.CompareTo(
                expectedValue);
        }

        throw new InvalidOperationException(
            $"Feature type '{effectiveType.FullName}' does " +
            "not support ordered comparisons.");
    }

    private static bool Contains(
        object? observedValue,
        Type featureType,
        string expectedText)
    {
        if (observedValue is null)
        {
            return false;
        }

        if (observedValue is string observedText)
        {
            return observedText.IndexOf(
                expectedText,
                StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (observedValue is not IEnumerable enumerable)
        {
            throw new InvalidOperationException(
                "The CONTAINS operator can only be applied " +
                "to text or collection features.");
        }

        Type? elementType =
            TryGetEnumerableElementType(
                featureType);

        foreach (object? element
                 in enumerable)
        {
            if (element is null)
            {
                if (IsNullLiteral(expectedText))
                {
                    return true;
                }

                continue;
            }

            Type comparisonType =
                elementType ??
                element.GetType();

            if (AreEqual(
                    element,
                    comparisonType,
                    expectedText))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInExpectedValues(
        object? observedValue,
        Type featureType,
        string expectedText)
    {
        string[] expectedValues =
            SplitExpectedValues(
                expectedText);

        if (expectedValues.Length == 0)
        {
            throw new InvalidOperationException(
                "The IN and NOT_IN operators require at " +
                "least one expected value.");
        }

        return expectedValues.Any(
            value =>
                AreEqual(
                    observedValue,
                    featureType,
                    value));
    }

    private static object ConvertTextToType(
        string text,
        Type targetType)
    {
        Type effectiveType =
            GetEffectiveType(targetType);

        string normalizedText =
            text.Trim();

        if (effectiveType == typeof(string))
        {
            return normalizedText;
        }

        if (effectiveType == typeof(bool))
        {
            return ParseBoolean(
                normalizedText);
        }

        if (effectiveType.IsEnum)
        {
            try
            {
                return Enum.Parse(
                    effectiveType,
                    normalizedText,
                    ignoreCase: true);
            }
            catch (ArgumentException exception)
            {
                string allowedValues =
                    string.Join(
                        ", ",
                        Enum.GetNames(effectiveType));

                throw new FormatException(
                    $"Value '{text}' is not valid for " +
                    $"enumeration '{effectiveType.Name}'. " +
                    $"Allowed values are: {allowedValues}.",
                    exception);
            }
        }

        if (effectiveType == typeof(Guid))
        {
            return Guid.Parse(
                normalizedText);
        }

        if (effectiveType == typeof(DateTime))
        {
            return DateTime.Parse(
                normalizedText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        if (effectiveType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(
                normalizedText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        if (effectiveType == typeof(TimeSpan))
        {
            return TimeSpan.Parse(
                normalizedText,
                CultureInfo.InvariantCulture);
        }

        TypeConverter converter =
            TypeDescriptor.GetConverter(
                effectiveType);

        if (converter.CanConvertFrom(
                typeof(string)))
        {
            object? convertedValue =
                converter.ConvertFrom(
                    context: null,
                    culture:
                        CultureInfo.InvariantCulture,
                    value:
                        normalizedText);

            if (convertedValue is not null)
            {
                return convertedValue;
            }
        }

        try
        {
            return Convert.ChangeType(
                normalizedText,
                effectiveType,
                CultureInfo.InvariantCulture);
        }
        catch (Exception exception)
            when (exception is InvalidCastException or
                  FormatException or
                  OverflowException)
        {
            throw new FormatException(
                $"Value '{text}' cannot be converted to " +
                $"feature type '{effectiveType.Name}'.",
                exception);
        }
    }

    private static bool ParseBoolean(
        string text)
    {
        if (bool.TryParse(
                text,
                out bool booleanValue))
        {
            return booleanValue;
        }

        return text.Trim().ToUpperInvariant() switch
        {
            "1" or "YES" or "Y" =>
                true,

            "0" or "NO" or "N" =>
                false,

            _ =>
                throw new FormatException(
                    $"Value '{text}' is not a valid Boolean " +
                    "value.")
        };
    }

    private static string[] SplitExpectedValues(
        string expectedText)
    {
        return expectedText
            .Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string GetDisplayedExpectedValue(
        KnownProblemRuleDefinition rule)
    {
        return rule.OperatorCode switch
        {
            KnownProblemRuleDefinition.IsTrueOperator =>
                "true",

            KnownProblemRuleDefinition.IsFalseOperator =>
                "false",

            _ =>
                rule.ExpectedValue
        };
    }

    private static string FormatValue(
        object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        if (value is bool booleanValue)
        {
            return booleanValue
                ? "true"
                : "false";
        }

        if (value is DateTime dateTime)
        {
            return dateTime.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        if (value is IEnumerable enumerable &&
            value is not string)
        {
            var formattedElements =
                new List<string>();

            foreach (object? element
                     in enumerable)
            {
                formattedElements.Add(
                    FormatValue(element));
            }

            return string.Join(
                ";",
                formattedElements);
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(
                       format: null,
                       formatProvider:
                           CultureInfo.InvariantCulture) ??
                   string.Empty;
        }

        return value.ToString() ??
               string.Empty;
    }

    private static Type GetEffectiveType(
        Type type)
    {
        return Nullable.GetUnderlyingType(type) ??
               type;
    }

    private static bool IsNumericType(
        Type type)
    {
        Type effectiveType =
            GetEffectiveType(type);

        return Type.GetTypeCode(effectiveType) switch
        {
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.UInt16 or
            TypeCode.UInt32 or
            TypeCode.UInt64 or
            TypeCode.Int16 or
            TypeCode.Int32 or
            TypeCode.Int64 or
            TypeCode.Decimal or
            TypeCode.Double or
            TypeCode.Single =>
                true,

            _ =>
                false
        };
    }

    private static bool IsNullLiteral(
        string text)
    {
        return string.Equals(
                   text.Trim(),
                   "NULL",
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   text.Trim(),
                   "<NULL>",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static Type? TryGetEnumerableElementType(
        Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        if (collectionType.IsGenericType &&
            collectionType.GetGenericTypeDefinition() ==
            typeof(IEnumerable<>))
        {
            return collectionType
                .GetGenericArguments()[0];
        }

        Type? enumerableInterface =
            collectionType
                .GetInterfaces()
                .FirstOrDefault(
                    implementedInterface =>
                        implementedInterface.IsGenericType &&
                        implementedInterface
                            .GetGenericTypeDefinition() ==
                        typeof(IEnumerable<>));

        return enumerableInterface?
            .GetGenericArguments()[0];
    }

    private static IReadOnlyDictionary<
        string,
        PropertyInfo> BuildFeaturePropertyIndex()
    {
        return typeof(LotSizingProblemFeatures)
            .GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public)
            .Where(
                property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0)
            .ToDictionary(
                property =>
                    property.Name,
                property =>
                    property,
                StringComparer.OrdinalIgnoreCase);
    }

    private static string DisplayRuleCode(
        KnownProblemRuleDefinition rule)
    {
        return string.IsNullOrWhiteSpace(
                rule.RuleCode)
            ? "<missing code>"
            : rule.RuleCode;
    }
}