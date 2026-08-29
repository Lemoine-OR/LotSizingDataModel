namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// Stable structured diagnostic emitted by the end-to-end scientific solve
/// pipeline.
/// </summary>
public sealed class ScientificSolvePipelineDiagnostic
{
    public ScientificSolvePipelineDiagnostic(
        string code,
        ScientificSolvePipelineDiagnosticSeverity severity,
        string path,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A scientific solve-pipeline diagnostic code is required.",
                nameof(code));
        }

        Code = code.Trim();
        Severity = severity;
        Path = path ?? string.Empty;
        Message = message ?? string.Empty;
    }

    public string Code { get; }
    public ScientificSolvePipelineDiagnosticSeverity Severity { get; }
    public string Path { get; }
    public string Message { get; }

    public bool IsError =>
        Severity ==
        ScientificSolvePipelineDiagnosticSeverity.Error;

    public override string ToString() =>
        $"[{Severity}] {Code} at {Path}: {Message}";
}
