namespace LotSizingDataModel.Checker.Feasibility;

public sealed class MathematicalFeasibilityDiagnostic
{
    public MathematicalFeasibilityDiagnostic(
        string code,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Feasibility diagnostics require code and message.");
        }

        Code = code.Trim();
        Message = message.Trim();
    }

    public string Code { get; }
    public string Message { get; }
}
