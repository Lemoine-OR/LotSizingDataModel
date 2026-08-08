using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Represents a parsed mathematical-model business-domain key.
/// </summary>
/// <remarks>
/// Domain keys use the canonical format:
/// <code>
/// category|name=value|name=value
/// </code>
/// For example:
/// <code>
/// inventory|item=1|warehouse=2|period=3
/// </code>
/// </remarks>
public sealed class MathematicalDomainKey
{
    private readonly Dictionary<string, string> _segments =
        new(
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a mathematical domain key.
    /// </summary>
    /// <param name="category">
    /// Domain-key category.
    /// </param>
    public MathematicalDomainKey(
        string category)
    {
        if (string.IsNullOrWhiteSpace(
                category))
        {
            throw new ArgumentException(
                "A mathematical domain-key category is required.",
                nameof(category));
        }

        Category =
            category.Trim();
    }

    /// <summary>
    /// Gets the domain-key category.
    /// </summary>
    public string Category
    {
        get;
    }

    /// <summary>
    /// Gets the parsed name-value segments.
    /// </summary>
    public IReadOnlyDictionary<string, string> Segments =>
        _segments;

    /// <summary>
    /// Gets the number of parsed segments.
    /// </summary>
    public int SegmentCount =>
        _segments.Count;

    /// <summary>
    /// Determines whether a segment exists.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the segment exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        string name)
    {
        return _segments.ContainsKey(
            NormalizeName(
                name));
    }

    /// <summary>
    /// Gets a required text segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <returns>
    /// Segment value.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the segment does not exist.
    /// </exception>
    public string GetRequiredString(
        string name)
    {
        string normalizedName =
            NormalizeName(
                name);

        if (_segments.TryGetValue(
                normalizedName,
                out string? value))
        {
            return value;
        }

        throw new KeyNotFoundException(
            $"Domain-key segment '{normalizedName}' is required.");
    }

    /// <summary>
    /// Gets a required integer segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <returns>
    /// Parsed integer value.
    /// </returns>
    public int GetRequiredInt32(
        string name)
    {
        string value =
            GetRequiredString(
                name);

        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsedValue))
        {
            throw new FormatException(
                $"Domain-key segment '{name}' value '{value}' " +
                "is not a valid 32-bit integer.");
        }

        return parsedValue;
    }

    /// <summary>
    /// Attempts to get a text segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Segment value when found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the segment exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetString(
        string name,
        out string? value)
    {
        return _segments.TryGetValue(
            NormalizeName(
                name),
            out value);
    }

    /// <summary>
    /// Attempts to get an integer segment.
    /// </summary>
    /// <param name="name">
    /// Segment name.
    /// </param>
    /// <param name="value">
    /// Parsed integer value when successful.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the segment exists and is a
    /// valid integer; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetInt32(
        string name,
        out int value)
    {
        value =
            default;

        return TryGetString(
                   name,
                   out string? text) &&
               int.TryParse(
                   text,
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out value);
    }

    /// <summary>
    /// Parses a canonical mathematical domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Domain-key text.
    /// </param>
    /// <returns>
    /// Parsed mathematical domain key.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the domain key is empty.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when the domain key contains malformed or duplicate
    /// segments.
    /// </exception>
    public static MathematicalDomainKey Parse(
        string domainKey)
    {
        if (string.IsNullOrWhiteSpace(
                domainKey))
        {
            throw new ArgumentException(
                "A mathematical domain key is required.",
                nameof(domainKey));
        }

        string[] parts =
            domainKey
                .Split(
                    '|',
                    StringSplitOptions.TrimEntries);

        if (parts.Length == 0 ||
            string.IsNullOrWhiteSpace(
                parts[0]))
        {
            throw new FormatException(
                "A mathematical domain key must start with a " +
                "category.");
        }

        var result =
            new MathematicalDomainKey(
                parts[0]);

        foreach (
            string part
            in parts.Skip(
                1))
        {
            int separatorIndex =
                part.IndexOf(
                    '=',
                    StringComparison.Ordinal);

            if (separatorIndex <= 0 ||
                separatorIndex ==
                part.Length - 1)
            {
                throw new FormatException(
                    $"Malformed mathematical domain-key segment " +
                    $"'{part}'. Expected 'name=value'.");
            }

            string name =
                part[..separatorIndex].Trim();

            string value =
                part[(separatorIndex + 1)..].Trim();

            if (name.Length == 0 ||
                value.Length == 0)
            {
                throw new FormatException(
                    $"Malformed mathematical domain-key segment " +
                    $"'{part}'.");
            }

            if (!result._segments.TryAdd(
                    name,
                    value))
            {
                throw new FormatException(
                    $"Mathematical domain-key segment '{name}' " +
                    "appears more than once.");
            }
        }

        return result;
    }

    /// <summary>
    /// Attempts to parse a canonical mathematical domain key.
    /// </summary>
    /// <param name="domainKey">
    /// Domain-key text.
    /// </param>
    /// <param name="result">
    /// Parsed key when successful.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when parsing succeeds; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string domainKey,
        out MathematicalDomainKey? result)
    {
        try
        {
            result =
                Parse(
                    domainKey);

            return true;
        }
        catch (
            ArgumentException)
        {
            result =
                null;

            return false;
        }
        catch (
            FormatException)
        {
            result =
                null;

            return false;
        }
    }

    /// <summary>
    /// Returns the canonical textual representation.
    /// </summary>
    /// <returns>
    /// Canonical domain key.
    /// </returns>
    public override string ToString()
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

    private static string NormalizeName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(
                name))
        {
            throw new ArgumentException(
                "A domain-key segment name is required.",
                nameof(name));
        }

        return name.Trim();
    }
}
