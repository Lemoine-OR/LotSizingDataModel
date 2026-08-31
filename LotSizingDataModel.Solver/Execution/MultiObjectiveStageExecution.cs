using LotSizingDataModel.Core.DecisionModel.Objectives;

namespace LotSizingDataModel.Solver.Execution;

public sealed class MultiObjectiveStageExecution
{
    public required OptimizationObjectiveKind Kind { get; init; }

    public int StageIndex { get; init; }

    public double ObjectiveValue { get; init; }

    public double AbsoluteTolerance { get; init; }

    public string SolverName { get; init; } = string.Empty;

    public string SolverVersion { get; init; } = string.Empty;
}
