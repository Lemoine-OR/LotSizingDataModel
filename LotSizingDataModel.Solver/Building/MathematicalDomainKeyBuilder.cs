using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LotSizingDataModel.Solver.Building;

/// <summary>
/// Builds stable business-domain keys for mathematical
/// variables and constraints.
/// </summary>
/// <remarks>
/// A domain key is composed of a category followed by ordered
/// name-value segments:
/// <code>
/// inventory|item=1|warehouse=2|period=3
/// </code>
/// Segment names are compared using ordinal comparison and are
/// emitted in insertion order.
/// </remarks>
public sealed class MathematicalDomainKeyBuilder
{
    private readonly List<KeyValuePair<string, string>> _segments =
        new();

    /// <summary>
    /// Initializes a domain-key builder.
    /// </summary>
    /// <param name="category">
    /// Domain-key category.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="category"/> is empty or
    /// contains a reserved character.
    /// </exception>
    public MathematicalDomainKeyBuilder(
        string category)
    {
        Category =
            NormalizeToken(
                category,
                nameof(category));
    }

    /// <summary>
    /// Gets the domain-key category.
    /// </summary>
    public string Category
    {
        get;
    }

    /// <summary>
    /// Gets the number of added segments.
    /// </summary>
    public int SegmentCount =>
        _segments.Count;

    /// <summary>
    /// Adds a text segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Segment value.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalDomainKeyBuilder Add(
        string name,
        string value)
    {
        string normalizedName =
            NormalizeToken(
                name,
                nameof(name));

        string normalizedValue =
            NormalizeToken(
                value,
                nameof(value));

        EnsureSegmentNameIsUnique(
            normalizedName);

        _segments.Add(
            new KeyValuePair<string, string>(
                normalizedName,
                normalizedValue));

        return this;
    }

    /// <summary>
    /// Adds an integer segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Segment value.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalDomainKeyBuilder Add(
        string name,
        int value)
    {
        return Add(
            name,
            value.ToString(
                CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Adds a long integer segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Segment value.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalDomainKeyBuilder Add(
        string name,
        long value)
    {
        return Add(
            name,
            value.ToString(
                CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Adds a globally unique identifier segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Segment value.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalDomainKeyBuilder Add(
        string name,
        Guid value)
    {
        return Add(
            name,
            value.ToString(
                "D",
                CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Adds an enumeration segment.
    /// </summary>
    /// <typeparam name="TEnum">
    /// Enumeration type.
    /// </typeparam>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Segment value.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalDomainKeyBuilder Add<TEnum>(
        string name,
        TEnum value)
        where TEnum : struct, Enum
    {
        return Add(
            name,
            value.ToString());
    }

    /// <summary>
    /// Adds a segment only when the supplied text value is not
    /// empty.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Optional segment value.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public MathematicalDomainKeyBuilder AddOptional(
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(
                value))
        {
            Add(
                name,
                value);
        }

        return this;
    }

    /// <summary>
    /// Builds the immutable domain-key text.
    /// </summary>
    /// <returns>
    /// Stable domain key.
    /// </returns>
    public string Build()
    {
        if (_segments.Count == 0)
        {
            return Category;
        }

        return string.Join(
            "|",
            new[]
            {
                Category
            }.Concat(
                _segments.Select(
                    segment =>
                        $"{segment.Key}={segment.Value}")));
    }

    /// <summary>
    /// Returns the built domain key.
    /// </summary>
    /// <returns>
    /// Stable domain key.
    /// </returns>
    public override string ToString()
    {
        return Build();
    }

    private void EnsureSegmentNameIsUnique(
        string segmentName)
    {
        if (_segments.Any(
                segment =>
                    string.Equals(
                        segment.Key,
                        segmentName,
                        StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Domain-key segment '{segmentName}' is already " +
                "defined.");
        }
    }

    private static string NormalizeToken(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            throw new ArgumentException(
                "A domain-key token cannot be empty.",
                parameterName);
        }

        string normalizedValue =
            value.Trim();

        if (normalizedValue.Contains(
                '|',
                StringComparison.Ordinal) ||
            normalizedValue.Contains(
                '=',
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A domain-key token cannot contain the reserved " +
                "characters '|' or '='.",
                parameterName);
        }

        return normalizedValue;
    }
}
