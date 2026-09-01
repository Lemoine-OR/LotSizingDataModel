using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Checker.Feasibility;

public sealed class MathematicalFeasibilityCheckResult
{
    public MathematicalFeasibilityCheckResult(
        FeasibilityStatus status,
        IReadOnlyList<MathematicalFeasibilityDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        Status = status;
        Diagnostics = diagnostics.ToArray();
    }

    public FeasibilityStatus Status { get; }
    public MathematicalFeasibilityDiagnostic[] Diagnostics { get; }

    public bool IsFeasible =>
        Status == FeasibilityStatus.Feasible;
}
