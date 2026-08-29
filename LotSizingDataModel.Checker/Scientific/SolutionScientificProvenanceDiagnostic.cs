namespace LotSizingDataModel.Checker.Scientific;

/// <summary>
/// Structured scientific solution-provenance diagnostic.
/// </summary>
public sealed class SolutionScientificProvenanceDiagnostic
{
    public SolutionScientificProvenanceDiagnostic(
        string code,
        SolutionScientificProvenanceDiagnosticSeverity severity,
        string path,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A provenance diagnostic code is required.",
                nameof(code));
        }

        Code = code.Trim();
        Severity = severity;
        Path = path ?? string.Empty;
        Message = message ?? string.Empty;
    }

    public string Code { get; }
    public SolutionScientificProvenanceDiagnosticSeverity Severity { get; }
    public string Path { get; }
    public string Message { get; }

    public bool IsError =>
        Severity ==
        SolutionScientificProvenanceDiagnosticSeverity.Error;
}
