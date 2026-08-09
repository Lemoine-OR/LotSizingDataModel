using System.Globalization;
using System.Text.RegularExpressions;

namespace LotSizingDataModel.Solver.Xpress;

/// <summary>
/// Parses Xpress ASCII solution rows written with the standard Optimizer
/// solution layout.
/// </summary>
/// <remarks>
/// In the documented ASCII layout, field 2 is the column name and field 5 is
/// the activity value. The parser also contains a conservative fallback for
/// minor formatting differences between Xpress releases.
/// </remarks>
public static partial class XpressAsciiSolutionValueParser
{
    /// <summary>
    /// Parses variable activities keyed by the portable mathematical variable
    /// identifier embedded in names of the form <c>v_&lt;id&gt;</c>.
    /// </summary>
    /// <param name="path">Xpress ASCII solution file.</param>
    /// <returns>Parsed finite activities keyed by mathematical variable ID.</returns>
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
    /// Parses Xpress ASCII solution lines.
    /// </summary>
    /// <param name="lines">Solution lines.</param>
    /// <returns>Parsed activities.</returns>
    public static IReadOnlyDictionary<int, double> ParseLines(
        IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var result =
            new Dictionary<int, double>();

        foreach (string? line in lines)
        {
            string currentLine =
                line ?? string.Empty;

            Match nameMatch =
                VariableNameRegex().Match(
                    currentLine);

            if (!nameMatch.Success ||
                !int.TryParse(
                    nameMatch.Groups[1].Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int variableId))
            {
                continue;
            }

            string[] fields =
                currentLine
                    .Split(
                        (char[]?)null,
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries);

            int nameIndex =
                Array.FindIndex(
                    fields,
                    field =>
                        string.Equals(
                            field,
                            nameMatch.Value,
                            StringComparison.Ordinal));

            if (nameIndex >= 0 &&
                nameIndex + 3 < fields.Length &&
                TryParseFinite(
                    fields[nameIndex + 3],
                    out double documentedActivity))
            {
                result[variableId] =
                    documentedActivity;
                continue;
            }

            string suffix =
                currentLine[(nameMatch.Index + nameMatch.Length)..];

            MatchCollection numbers =
                NumberRegex().Matches(suffix);

            if (numbers.Count == 0)
            {
                continue;
            }

            // The activity is normally the third field after the name. If
            // formatting has collapsed fields, the final finite number on the
            // row is the safest fallback for a standard column solution row.
            for (int index = numbers.Count - 1; index >= 0; index--)
            {
                if (TryParseFinite(
                        numbers[index].Value,
                        out double fallbackActivity))
                {
                    result[variableId] =
                        fallbackActivity;
                    break;
                }
            }
        }

        return result;
    }

    private static bool TryParseFinite(
        string text,
        out double value)
    {
        return double.TryParse(
                   text,
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out value) &&
               double.IsFinite(value);
    }

    [GeneratedRegex(
        @"\bv_(\d+)\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex VariableNameRegex();

    [GeneratedRegex(
        @"[+\-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+\-]?\d+)?",
        RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
