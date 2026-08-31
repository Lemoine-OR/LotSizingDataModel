using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Execution;

public sealed class ExecutableObjectiveCriterion
{
    public required OptimizationObjectiveKind Kind { get; init; }

    public required LinearExpression Expression { get; init; }

    public double Weight { get; init; } = 1.0;

    public int Priority { get; init; }

    public double AbsoluteTolerance { get; init; }
}
