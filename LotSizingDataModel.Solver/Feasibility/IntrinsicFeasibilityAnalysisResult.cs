namespace LotSizingDataModel.Solver.Feasibility;

public sealed class IntrinsicFeasibilityAnalysisResult
{
    public IntrinsicFeasibilityAnalysisResult(
        IntrinsicFeasibilityStatus status,
        IReadOnlyList<IntrinsicFeasibilityDiagnostic> diagnostics,
        int evaluatedConstraintCount)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (evaluatedConstraintCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(evaluatedConstraintCount));
        }

        Status = status;
        Diagnostics = diagnostics.ToArray();
        EvaluatedConstraintCount = evaluatedConstraintCount;
    }

    public IntrinsicFeasibilityStatus Status { get; }
    public IntrinsicFeasibilityDiagnostic[] Diagnostics { get; }
    public int EvaluatedConstraintCount { get; }

    public bool HasProofOfInfeasibility =>
        Status == IntrinsicFeasibilityStatus.Infeasible;
}
