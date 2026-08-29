namespace LotSizingDataModel.Instance.Notation.Matching;

/// <summary>
/// Represents one structured explanation emitted by the notation matcher.
/// </summary>
public sealed class UniversalNotationMatchIssue
{
    public UniversalNotationMatchIssue(
        string code,
        string path,
        string expected,
        string actual,
        string message,
        bool isContradiction)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A match issue code is required.",
                nameof(code));
        }

        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(message);

        Code = code.Trim();
        Path = path;
        Expected = expected;
        Actual = actual;
        Message = message;
        IsContradiction = isContradiction;
    }

    public string Code { get; }
    public string Path { get; }
    public string Expected { get; }
    public string Actual { get; }
    public string Message { get; }

    /// <summary>
    /// Gets whether this issue is a proven contradiction. False means the
    /// descriptor is currently incomplete for the requested requirement.
    /// </summary>
    public bool IsContradiction { get; }

    public override string ToString() =>
        $"[{Code}] {Path}: expected {Expected}, actual {Actual}. {Message}";
}
