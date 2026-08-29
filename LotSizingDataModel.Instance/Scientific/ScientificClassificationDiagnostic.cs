namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// One structured diagnostic emitted by the scientific classification engine.
/// </summary>
public sealed class ScientificClassificationDiagnostic
{
    public ScientificClassificationDiagnostic(
        string code,
        ScientificClassificationDiagnosticSeverity severity,
        string path,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A scientific-classification diagnostic code is required.",
                nameof(code));
        }

        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(message);

        Code = code.Trim();
        Severity = severity;
        Path = path;
        Message = message;
    }

    public string Code { get; }
    public ScientificClassificationDiagnosticSeverity Severity { get; }
    public string Path { get; }
    public string Message { get; }

    public bool IsError =>
        Severity ==
        ScientificClassificationDiagnosticSeverity.Error;

    public override string ToString() =>
        $"[{Severity}] {Code} at {Path}: {Message}";
}
