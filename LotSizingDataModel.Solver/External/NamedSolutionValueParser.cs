using System.Globalization;
using System.Text.RegularExpressions;

namespace LotSizingDataModel.Solver.External;

/// <summary>
/// Parses variable values from solver text files when variables use the
/// portable <c>v_&lt;id&gt;</c> naming convention.
/// </summary>
public static partial class NamedSolutionValueParser
{
    /// <summary>
    /// Parses all recognized variable/value pairs from a solution file.
    /// </summary>
    /// <param name="path">Solution-file path.</param>
    /// <returns>Variable values keyed by mathematical variable identifier.</returns>
    public static IReadOnlyDictionary<int, double> ParseFile(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !File.Exists(path))
        {
            return new Dictionary<int, double>();
        }

        return ParseLines(
            File.ReadLines(path));
    }

    /// <summary>
    /// Parses recognized variable/value pairs from text lines.
    /// </summary>
    /// <param name="lines">Solution text lines.</param>
    /// <returns>Variable values keyed by mathematical variable identifier.</returns>
    public static IReadOnlyDictionary<int, double> ParseLines(
        IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var values =
            new Dictionary<int, double>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            Match match =
                VariableNameRegex().Match(line);

            if (!match.Success ||
                !int.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int variableId))
            {
                continue;
            }

            string suffix =
                line[(match.Index + match.Length)..];

            Match numberMatch =
                NumberRegex().Match(suffix);

            if (!numberMatch.Success ||
                !double.TryParse(
                    numberMatch.Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value) ||
                !double.IsFinite(value))
            {
                continue;
            }

            values[variableId] =
                value;
        }

        return values;
    }

    /// <summary>
    /// Tries to parse the first floating-point number captured after a supplied
    /// regular-expression prefix.
    /// </summary>
    /// <param name="text">Text to inspect.</param>
    /// <param name="prefixPattern">Case-insensitive regular-expression prefix.</param>
    /// <param name="value">Parsed finite value.</param>
    /// <returns><see langword="true"/> when a value was found.</returns>
    public static bool TryParseNumberAfter(
        string text,
        string prefixPattern,
        out double value)
    {
        value =
            default;

        if (string.IsNullOrWhiteSpace(text) ||
            string.IsNullOrWhiteSpace(prefixPattern))
        {
            return false;
        }

        Match match =
            Regex.Match(
                text,
                prefixPattern +
                @"\s*(?<value>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)",
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant);

        return match.Success &&
            double.TryParse(
                match.Groups["value"].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value) &&
            double.IsFinite(value);
    }

    [GeneratedRegex(@"\bv_(\d+)\b", RegexOptions.CultureInvariant)]
    private static partial Regex VariableNameRegex();

    [GeneratedRegex(@"[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
