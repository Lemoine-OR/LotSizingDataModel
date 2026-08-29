using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// Known native optimization backends in the current Solver abstraction.
/// </summary>
public static class ScientificSolverBackendCatalog
{
    private static readonly ScientificSolutionMethodCategory[]
        MilpOnly =
            new[]
            {
                ScientificSolutionMethodCategory
                    .MixedIntegerLinearProgramming,
                ScientificSolutionMethodCategory
                    .DantzigWolfeBranchAndPrice,
                ScientificSolutionMethodCategory
                    .Matheuristic
            };

    public static ScientificSolverBackendDefinition Cplex { get; } =
        new(
            SolverKind.Cplex,
            "IBM ILOG CPLEX",
            MilpOnly);

    public static ScientificSolverBackendDefinition Gurobi { get; } =
        new(
            SolverKind.Gurobi,
            "Gurobi Optimizer",
            MilpOnly);

    public static ScientificSolverBackendDefinition Xpress { get; } =
        new(
            SolverKind.Xpress,
            "FICO Xpress Optimizer",
            MilpOnly);

    public static ScientificSolverBackendDefinition CoinOrCbc { get; } =
        new(
            SolverKind.CoinOrCbc,
            "COIN-OR CBC",
            MilpOnly);

    public static IReadOnlyList<ScientificSolverBackendDefinition>
        All { get; } =
            new[]
            {
                Cplex,
                Gurobi,
                Xpress,
                CoinOrCbc
            };

    public static ScientificSolverBackendDefinition? Find(
        SolverKind solverKind) =>
            All.FirstOrDefault(
                backend =>
                    backend.SolverKind == solverKind);
}
