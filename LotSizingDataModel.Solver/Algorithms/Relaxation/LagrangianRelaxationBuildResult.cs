using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Result of a source-preserving Lagrangian model transformation.
/// </summary>
public sealed class LagrangianRelaxationBuildResult
{
    public LagrangianRelaxationBuildResult(
        MathematicalModel relaxedModel,
        IReadOnlyList<int> relaxedConstraintIds)
    {
        RelaxedModel =
            relaxedModel ??
            throw new ArgumentNullException(
                nameof(relaxedModel));

        ArgumentNullException.ThrowIfNull(
            relaxedConstraintIds);

        RelaxedConstraintIds =
            relaxedConstraintIds.ToArray();

        if (RelaxedConstraintIds.Count == 0)
        {
            throw new InvalidOperationException(
                "A Lagrangian build result must identify at least one relaxed constraint.");
        }
    }

    public MathematicalModel RelaxedModel
    {
        get;
    }

    public IReadOnlyList<int> RelaxedConstraintIds
    {
        get;
    }
}
