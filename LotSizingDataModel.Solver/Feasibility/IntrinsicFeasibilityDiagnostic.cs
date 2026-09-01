namespace LotSizingDataModel.Solver.Feasibility;

public sealed class IntrinsicFeasibilityDiagnostic
{
    public IntrinsicFeasibilityDiagnostic(
        string code,
        string message,
        int? constraintId = null,
        double? minimumLeftHandSide = null,
        double? maximumLeftHandSide = null,
        double? rightHandSide = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A feasibility diagnostic code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "A feasibility diagnostic message is required.",
                nameof(message));
        }

        Code = code.Trim();
        Message = message.Trim();
        ConstraintId = constraintId;
        MinimumLeftHandSide = minimumLeftHandSide;
        MaximumLeftHandSide = maximumLeftHandSide;
        RightHandSide = rightHandSide;
    }

    public string Code { get; }
    public string Message { get; }
    public int? ConstraintId { get; }
    public double? MinimumLeftHandSide { get; }
    public double? MaximumLeftHandSide { get; }
    public double? RightHandSide { get; }
}
