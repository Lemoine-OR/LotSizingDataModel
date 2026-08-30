using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// Scientific solution-method families relevant to the current lot-sizing
/// catalog.
/// </summary>
/// <remarks>
/// CatalogOnly does not mean every named algorithm in a family supports every
/// extension. A future concrete algorithm adapter must declare its own exact
/// applicability before becoming executable.
/// </remarks>
public static class ScientificSolutionMethodCatalog
{
    private static readonly CanonicalLotSizingProblemClassId[]
        AllExecutableCoreClasses =
            new[]
            {
                CanonicalLotSizingProblemClassId
                    .SingleItemUncapacitatedLotSizing,
                CanonicalLotSizingProblemClassId
                    .SingleItemCapacitatedLotSizing,
                CanonicalLotSizingProblemClassId
                    .MultiItemUncapacitatedLotSizing,
                CanonicalLotSizingProblemClassId
                    .MultiItemCapacitatedLotSizing,
                CanonicalLotSizingProblemClassId
                    .UncapacitatedMultiLevelLotSizing,
                CanonicalLotSizingProblemClassId
                    .MultiLevelCapacitatedLotSizing
            };

    private static readonly CanonicalLotSizingProblemClassId[]
        GeneralMilpProblemClasses =
            AllExecutableCoreClasses
                .Concat(
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .DiscreteLotSizingAndScheduling,
                        CanonicalLotSizingProblemClassId
                            .ContinuousSetupLotSizing,
                        CanonicalLotSizingProblemClassId
                            .ProportionalLotSizingAndScheduling
                    })
                .ToArray();

    public static ScientificSolutionMethodDefinition GeneralMilp { get; } =
        new(
            methodId: "MILP-GENERAL",
            name: "General mixed-integer linear programming",
            category:
                ScientificSolutionMethodCategory
                    .MixedIntegerLinearProgramming,
            supportLevel:
                ScientificSolutionMethodSupportLevel.Executable,
            applicableProblemClasses:
                GeneralMilpProblemClasses,
            requiresMathematicalFormulation:
                true,
            requiresMilpBackend:
                true,
            evidence:
                new[]
                {
                    "Current solver-independent mathematical formulation stack",
                    "CPLEX/Gurobi/Xpress/COIN-OR CBC adapter architecture"
                },
            note:
                "The only solution-method family currently connected " +
                "end-to-end in LotSizingDataModel.");

    public static ScientificSolutionMethodDefinition
        SpecializedDynamicProgramming { get; } =
            new(
                methodId: "DP-SI-ULS",
                name: "Specialized dynamic programming",
                category:
                    ScientificSolutionMethodCategory.DynamicProgramming,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .SingleItemUncapacitatedLotSizing
                    },
                requiresMathematicalFormulation:
                    false,
                requiresMilpBackend:
                    false,
                evidence:
                    new[]
                    {
                        "Classical and survey literature on single-item " +
                        "uncapacitated lot sizing"
                    },
                note:
                    "Future concrete ULSAlgorithm adapters must declare " +
                    "their exact cost/extension assumptions.");

    public static ScientificSolutionMethodDefinition
        ShortestPathNetwork { get; } =
            new(
                methodId: "SP-SI-ULS",
                name: "Shortest-path / network exact method",
                category:
                    ScientificSolutionMethodCategory.ShortestPathNetwork,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .SingleItemUncapacitatedLotSizing
                    },
                requiresMathematicalFormulation:
                    false,
                requiresMilpBackend:
                    false,
                evidence:
                    new[]
                    {
                        "Classical network formulations of single-item " +
                        "uncapacitated lot sizing"
                    });

    public static ScientificSolutionMethodDefinition
        LagrangianRelaxation { get; } =
            new(
                methodId: "LR-CLSP",
                name: "Lagrangian relaxation",
                category:
                    ScientificSolutionMethodCategory.LagrangianRelaxation,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .MultiItemCapacitatedLotSizing,
                        CanonicalLotSizingProblemClassId
                            .MultiLevelCapacitatedLotSizing
                    },
                requiresMathematicalFormulation:
                    false,
                requiresMilpBackend:
                    false,
                evidence:
                    new[]
                    {
                        "Capacitated dynamic lot-sizing solution literature"
                    });

    public static ScientificSolutionMethodDefinition
        DantzigWolfeBranchAndPrice { get; } =
            new(
                methodId: "DW-BP-CLSP",
                name: "Dantzig-Wolfe decomposition / branch-and-price",
                category:
                    ScientificSolutionMethodCategory
                        .DantzigWolfeBranchAndPrice,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .MultiItemCapacitatedLotSizing
                    },
                requiresMathematicalFormulation:
                    true,
                requiresMilpBackend:
                    true,
                evidence:
                    new[]
                    {
                        "Branch-and-price literature for capacitated lot " +
                        "sizing with setup times"
                    });

    public static ScientificSolutionMethodDefinition
        ConstructiveHeuristic { get; } =
            new(
                methodId: "HEURISTIC-GENERAL",
                name: "Dedicated constructive/improvement heuristic",
                category:
                    ScientificSolutionMethodCategory.ConstructiveHeuristic,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    AllExecutableCoreClasses,
                requiresMathematicalFormulation:
                    false,
                requiresMilpBackend:
                    false,
                note:
                    "Family-level catalog entry only; concrete heuristic " +
                    "adapters will own exact applicability.");

    public static ScientificSolutionMethodDefinition
        Metaheuristic { get; } =
            new(
                methodId: "METAHEURISTIC-GENERAL",
                name: "Metaheuristic",
                category:
                    ScientificSolutionMethodCategory.Metaheuristic,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    AllExecutableCoreClasses,
                requiresMathematicalFormulation:
                    false,
                requiresMilpBackend:
                    false,
                evidence:
                    new[]
                    {
                        "Dynamic lot-sizing metaheuristics review literature"
                    },
                note:
                    "Future MetaheuristicsPlatform adapters must declare " +
                    "representation, evaluation and exact applicability.");

    public static ScientificSolutionMethodDefinition
        Matheuristic { get; } =
            new(
                methodId: "MATHEURISTIC-GENERAL",
                name: "Matheuristic / MILP-based hybrid",
                category:
                    ScientificSolutionMethodCategory.Matheuristic,
                supportLevel:
                    ScientificSolutionMethodSupportLevel.CatalogOnly,
                applicableProblemClasses:
                    AllExecutableCoreClasses,
                requiresMathematicalFormulation:
                    true,
                requiresMilpBackend:
                    true,
                note:
                    "Catalogued for future solver/metaheuristic integration.");

    public static IReadOnlyList<ScientificSolutionMethodDefinition>
        All { get; } =
            new[]
            {
                GeneralMilp,
                SpecializedDynamicProgramming,
                ShortestPathNetwork,
                LagrangianRelaxation,
                DantzigWolfeBranchAndPrice,
                ConstructiveHeuristic,
                Metaheuristic,
                Matheuristic
            };

    public static IReadOnlyList<ScientificSolutionMethodDefinition>
        ExecutableMethods =>
            All
                .Where(
                    method =>
                        method.SupportLevel ==
                        ScientificSolutionMethodSupportLevel.Executable)
                .ToArray();

    public static ScientificSolutionMethodDefinition? Find(
        string methodId)
    {
        if (string.IsNullOrWhiteSpace(methodId))
        {
            return null;
        }

        return All.FirstOrDefault(
            method =>
                method.MethodId.Equals(
                    methodId.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }
}
