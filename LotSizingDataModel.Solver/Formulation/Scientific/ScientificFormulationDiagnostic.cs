namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Stable structured formulation-capability diagnostic.
/// </summary>
public sealed class ScientificFormulationDiagnostic
{
    public ScientificFormulationDiagnostic(
        string code,
        ScientificFormulationDiagnosticSeverity severity,
        string path,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A formulation diagnostic code is required.",
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
    public ScientificFormulationDiagnosticSeverity Severity { get; }
    public string Path { get; }
    public string Message { get; }

    public bool IsError =>
        Severity == ScientificFormulationDiagnosticSeverity.Error;

    public override string ToString() =>
        $"[{Severity}] {Code} at {Path}: {Message}";
}
