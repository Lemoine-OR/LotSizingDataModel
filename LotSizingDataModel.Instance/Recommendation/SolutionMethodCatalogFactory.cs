using System;
using System.Collections.Generic;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Creates predefined catalogs of solution methods for
/// lot-sizing problem instances.
/// </summary>
/// <remarks>
/// The factory provides operational method definitions used
/// by <see cref="SolutionMethodAdvisor"/>.
///
/// The catalog describes technical applicability. It does not
/// claim that one method will systematically outperform
/// another method in practice.
///
/// Method kinds are resolved dynamically from the members
/// actually available in <see cref="SolutionMethodKind"/>.
/// No specific enumeration member is required at compile time.
/// </remarks>
public static class SolutionMethodCatalogFactory
{
    /// <summary>
    /// Gets the name of the standard solution-method catalog.
    /// </summary>
    public const string StandardCatalogName =
        "Standard lot-sizing solution-method catalog";

    /// <summary>
    /// Gets the current version of the standard
    /// solution-method catalog.
    /// </summary>
    public const string StandardCatalogVersion =
        "1.0";

    /// <summary>
    /// Gets the method code assigned to the Wagner-Whitin
    /// dynamic-programming method.
    /// </summary>
    public const string WagnerWhitinMethodCode =
        "WW-DP";

    /// <summary>
    /// Gets the method code assigned to the Silver-Meal
    /// heuristic.
    /// </summary>
    public const string SilverMealMethodCode =
        "SILVER-MEAL";

    /// <summary>
    /// Gets the method code assigned to the generic
    /// mixed-integer linear programming formulation.
    /// </summary>
    public const string GenericMilpMethodCode =
        "MILP-GENERIC";

    /// <summary>
    /// Gets the method code assigned to the production
    /// capacity Lagrangian-relaxation approach.
    /// </summary>
    public const string LagrangianRelaxationMethodCode =
        "LAGRANGIAN-CAPACITY";

    /// <summary>
    /// Gets the method code assigned to the generic
    /// fix-and-optimize matheuristic.
    /// </summary>
    public const string FixAndOptimizeMethodCode =
        "FIX-AND-OPTIMIZE";

    /// <summary>
    /// Creates the standard catalog of lot-sizing solution
    /// methods.
    /// </summary>
    /// <returns>
    /// A new, independent and structurally valid
    /// solution-method catalog.
    /// </returns>
    /// <remarks>
    /// The returned catalog contains:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Wagner-Whitin dynamic programming for the classical
    /// uncapacitated single-item problem;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the Silver-Meal single-item heuristic;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a generic mixed-integer linear formulation;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a production-capacity Lagrangian relaxation;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a generic fix-and-optimize matheuristic.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Each call creates new method-definition objects.
    /// Modifying one returned catalog therefore does not
    /// affect catalogs created by later calls.
    /// </remarks>
    public static SolutionMethodCatalog
        CreateStandardCatalog()
    {
        var catalog =
            new SolutionMethodCatalog(
                catalogName:
                    StandardCatalogName,

                catalogVersion:
                    StandardCatalogVersion)
            {
                Description =
                    "Operational catalog of mathematical " +
                    "programming methods, dynamic-programming " +
                    "procedures, relaxations and heuristics " +
                    "for lot-sizing problem instances.",

                AllowUnknownFeatureCodes =
                    false
            };

        catalog.AddMethod(
            CreateWagnerWhitinDefinition());

        catalog.AddMethod(
            CreateGenericMilpDefinition());

        catalog.AddMethod(
            CreateLagrangianRelaxationDefinition());

        catalog.AddMethod(
            CreateFixAndOptimizeDefinition());

        catalog.AddMethod(
            CreateSilverMealDefinition());

        catalog.EnsureValid();

        return catalog;
    }

    private static SolutionMethodDefinition
        CreateWagnerWhitinDefinition()
    {
        var definition =
            new SolutionMethodDefinition(
                methodCode:
                    WagnerWhitinMethodCode,

                name:
                    "Wagner-Whitin dynamic programming",

                methodKind:
                    ResolveMethodKind(
                        fallback:
                            default,

                        "DynamicProgramming",
                        "Dynamic_Programming",
                        "Dynamic",
                        "Exact",
                        "Optimization",
                        "MathematicalProgramming",
                        "Other",
                        "Unknown",
                        "Unspecified"))
            {
                MethodVersion =
                    StandardCatalogVersion,

                Description =
                    "Exact dynamic-programming method for " +
                    "the deterministic, uncapacitated, " +
                    "single-item and single-level " +
                    "lot-sizing problem.",

                Priority =
                    300,

                SupportsAnyProblemFamily =
                    false,

                SupportsAnyProductStructure =
                    false,

                SupportsUnclassifiedProblems =
                    false,

                SupportsAmbiguousClassifications =
                    false,

                SupportsCompleteProblems =
                    true,

                SupportsRelaxations =
                    false,

                SupportsSubproblems =
                    true,

                CanProduceFeasibleSolution =
                    true,

                CanProveOptimality =
                    true,

                CanProvideLowerBound =
                    true,

                CanProvideUpperBound =
                    true
            };

        definition.ReplaceSupportedProblemTypeCodes(
            new[]
            {
                "LS-U"
            });

        definition.ReplacePreferredProblemTypeCodes(
            new[]
            {
                "LS-U"
            });

        definition.ReplaceSupportedProductStructureTypes(
            new[]
            {
                ProductStructureType.IndependentItems
            });

        definition.ReplaceSupportedFeatureCodes(
            new[]
            {
                "HasSetupCosts",
                "HasTimeVaryingDemand",
                "HasInitialInventory"
            });

        definition.ReplacePreferredFeatureCodes(
            new[]
            {
                "HasSetupCosts",
                "HasTimeVaryingDemand"
            });

        definition.ReplaceUnsupportedFeatureCodes(
            GetClassicalSingleItemUnsupportedFeatureCodes());

        definition.Comment =
            "This definition represents the standard direct " +
            "application of the Wagner-Whitin method. " +
            "Specialized variants supporting additional " +
            "features should be represented by separate " +
            "method definitions.";

        return definition;
    }

    private static SolutionMethodDefinition
        CreateSilverMealDefinition()
    {
        var definition =
            new SolutionMethodDefinition(
                methodCode:
                    SilverMealMethodCode,

                name:
                    "Silver-Meal heuristic",

                methodKind:
                    ResolveMethodKind(
                        fallback:
                            default,

                        "Heuristic",
                        "ConstructiveHeuristic",
                        "Constructive",
                        "Approximation",
                        "Metaheuristic",
                        "Other",
                        "Unknown",
                        "Unspecified"))
            {
                MethodVersion =
                    StandardCatalogVersion,

                Description =
                    "Constructive heuristic for the " +
                    "deterministic, uncapacitated, " +
                    "single-item and single-level " +
                    "lot-sizing problem.",

                Priority =
                    130,

                SupportsAnyProblemFamily =
                    false,

                SupportsAnyProductStructure =
                    false,

                SupportsUnclassifiedProblems =
                    false,

                SupportsAmbiguousClassifications =
                    false,

                SupportsCompleteProblems =
                    true,

                SupportsRelaxations =
                    false,

                SupportsSubproblems =
                    true,

                CanProduceFeasibleSolution =
                    true,

                CanProveOptimality =
                    false,

                CanProvideLowerBound =
                    false,

                CanProvideUpperBound =
                    true
            };

        definition.ReplaceSupportedProblemTypeCodes(
            new[]
            {
                "LS-U"
            });

        definition.ReplacePreferredProblemTypeCodes(
            new[]
            {
                "LS-U"
            });

        definition.ReplaceSupportedProductStructureTypes(
            new[]
            {
                ProductStructureType.IndependentItems
            });

        definition.ReplaceSupportedFeatureCodes(
            new[]
            {
                "HasSetupCosts",
                "HasTimeVaryingDemand",
                "HasInitialInventory"
            });

        definition.ReplacePreferredFeatureCodes(
            new[]
            {
                "HasTimeVaryingDemand"
            });

        definition.ReplaceUnsupportedFeatureCodes(
            GetClassicalSingleItemUnsupportedFeatureCodes());

        definition.Comment =
            "The standard Silver-Meal heuristic does not " +
            "provide an optimality proof. Adapted variants " +
            "should be represented by separate method " +
            "definitions.";

        return definition;
    }

    private static SolutionMethodDefinition
        CreateGenericMilpDefinition()
    {
        var definition =
            new SolutionMethodDefinition(
                methodCode:
                    GenericMilpMethodCode,

                name:
                    "Generic mixed-integer linear formulation",

                methodKind:
                    ResolveMethodKind(
                        fallback:
                            default,

                        "MixedIntegerProgramming",
                        "MixedIntegerLinearProgramming",
                        "MathematicalProgramming",
                        "Exact",
                        "Optimization",
                        "Solver",
                        "Other",
                        "Unknown",
                        "Unspecified"))
            {
                MethodVersion =
                    StandardCatalogVersion,

                Description =
                    "Generic mixed-integer linear formulation " +
                    "intended to represent single-level, " +
                    "multi-level, capacitated and " +
                    "supply-chain extensions when the " +
                    "corresponding variables and constraints " +
                    "are implemented.",

                Priority =
                    200,

                SupportsAnyProblemFamily =
                    true,

                SupportsAnyProductStructure =
                    true,

                SupportsUnclassifiedProblems =
                    true,

                SupportsAmbiguousClassifications =
                    true,

                SupportsCompleteProblems =
                    true,

                SupportsRelaxations =
                    true,

                SupportsSubproblems =
                    true,

                CanProduceFeasibleSolution =
                    true,

                CanProveOptimality =
                    true,

                CanProvideLowerBound =
                    true,

                CanProvideUpperBound =
                    true
            };

        definition.ReplaceSupportedFeatureCodes(
            GetBroadlySupportedFeatureCodes());

        definition.ReplacePreferredProblemTypeCodes(
            new[]
            {
                "LS-C",
                "CLSP",
                "MLLP",
                "MLCLSP"
            });

        definition.Comment =
            "Practical tractability depends on the selected " +
            "formulation, solver, parameterization, hardware " +
            "and instance dimensions.";

        return definition;
    }

    private static SolutionMethodDefinition
        CreateLagrangianRelaxationDefinition()
    {
        var definition =
            new SolutionMethodDefinition(
                methodCode:
                    LagrangianRelaxationMethodCode,

                name:
                    "Production-capacity Lagrangian " +
                    "relaxation",

                methodKind:
                    ResolveMethodKind(
                        fallback:
                            default,

                        "LagrangianRelaxation",
                        "Relaxation",
                        "Decomposition",
                        "Dual",
                        "Exact",
                        "Optimization",
                        "Other",
                        "Unknown",
                        "Unspecified"))
            {
                MethodVersion =
                    StandardCatalogVersion,

                Description =
                    "Lagrangian-relaxation template that " +
                    "dualizes production-capacity coupling " +
                    "constraints and solves the resulting " +
                    "structured subproblems.",

                Priority =
                    180,

                SupportsAnyProblemFamily =
                    false,

                SupportsAnyProductStructure =
                    true,

                SupportsUnclassifiedProblems =
                    false,

                SupportsAmbiguousClassifications =
                    true,

                SupportsCompleteProblems =
                    false,

                SupportsRelaxations =
                    true,

                SupportsSubproblems =
                    true,

                CanProduceFeasibleSolution =
                    false,

                CanProveOptimality =
                    false,

                CanProvideLowerBound =
                    true,

                CanProvideUpperBound =
                    false
            };

        definition.ReplaceSupportedProblemTypeCodes(
            new[]
            {
                "LS-C",
                "CLSP",
                "MLCLSP"
            });

        definition.ReplacePreferredProblemTypeCodes(
            new[]
            {
                "CLSP",
                "MLCLSP"
            });

        definition.ReplaceRequiredFeatureCodes(
            new[]
            {
                "HasProductionCapacityConstraints"
            });

        definition.ReplaceSupportedFeatureCodes(
            new[]
            {
                "HasSetupCosts",
                "HasSetupTimes",
                "HasProductionLeadTimes",
                "HasMinimumLotSizes",
                "HasLotSizeMultiples",
                "HasSafetyStockRequirements",
                "HasBacklogging",
                "HasTimeVaryingDemand",
                "HasTimeVaryingProductionCapacity",
                "HasSharedProductionCapacity"
            });

        definition.ReplacePreferredFeatureCodes(
            new[]
            {
                "HasSharedProductionCapacity",
                "HasTimeVaryingProductionCapacity"
            });

        definition.ReplacePartiallySupportedFeatureCodes(
            new[]
            {
                "IsMultiSite",
                "HasPurchasing",
                "HasSupplierCapacityConstraints",
                "HasSupplierLeadTimes",
                "HasTransportation",
                "HasTransportCapacityConstraints",
                "HasTransportLeadTimes",
                "HasWarehouseCapacityConstraints",
                "HasAdditionalCapacity"
            });

        definition.ReplaceUnsupportedFeatureCodes(
            new[]
            {
                "HasLostSales",
                "HasStartUpCosts",
                "HasFinancialConstraints",
                "HasMultipleObjectives"
            });

        definition.Comment =
            "A complete algorithm normally requires a dual " +
            "optimization procedure and, when a feasible " +
            "solution is required, a primal recovery method.";

        return definition;
    }

    private static SolutionMethodDefinition
        CreateFixAndOptimizeDefinition()
    {
        var definition =
            new SolutionMethodDefinition(
                methodCode:
                    FixAndOptimizeMethodCode,

                name:
                    "Generic fix-and-optimize matheuristic",

                methodKind:
                    ResolveMethodKind(
                        fallback:
                            default,

                        "Matheuristic",
                        "Hybrid",
                        "Heuristic",
                        "Metaheuristic",
                        "NeighborhoodSearch",
                        "Optimization",
                        "Other",
                        "Unknown",
                        "Unspecified"))
            {
                MethodVersion =
                    StandardCatalogVersion,

                Description =
                    "Matheuristic that repeatedly fixes part " +
                    "of a mixed-integer solution and " +
                    "reoptimizes selected neighborhoods.",

                Priority =
                    150,

                SupportsAnyProblemFamily =
                    true,

                SupportsAnyProductStructure =
                    true,

                SupportsUnclassifiedProblems =
                    true,

                SupportsAmbiguousClassifications =
                    true,

                SupportsCompleteProblems =
                    true,

                SupportsRelaxations =
                    false,

                SupportsSubproblems =
                    true,

                CanProduceFeasibleSolution =
                    true,

                CanProveOptimality =
                    false,

                CanProvideLowerBound =
                    false,

                CanProvideUpperBound =
                    true
            };

        definition.ReplaceSupportedFeatureCodes(
            GetBroadlySupportedFeatureCodes());

        definition.ReplacePreferredProblemTypeCodes(
            new[]
            {
                "CLSP",
                "MLLP",
                "MLCLSP"
            });

        definition.ReplacePreferredFeatureCodes(
            new[]
            {
                "IsMultiItem",
                "IsMultiLevel",
                "HasProductionCapacityConstraints",
                "HasSharedProductionCapacity"
            });

        definition.Comment =
            "Compatibility assumes that a valid MILP " +
            "formulation and an initial feasible solution or " +
            "solution-construction procedure are available.";

        return definition;
    }

    private static IReadOnlyList<string>
        GetClassicalSingleItemUnsupportedFeatureCodes()
    {
        return new[]
        {
            "HasProductionCapacityConstraints",
            "HasSharedProductionCapacity",
            "HasTimeVaryingProductionCapacity",
            "HasSafetyStockRequirements",
            "HasBacklogging",
            "HasLostSales",
            "HasProductionLeadTimes",
            "HasMinimumLotSizes",
            "HasMaximumLotSizes",
            "HasLotSizeMultiples",
            "HasSetupTimes",
            "HasStartUpCosts",
            "HasAdditionalCapacity",
            "HasPurchasing",
            "HasSupplierCapacityConstraints",
            "HasSupplierLeadTimes",
            "HasTransportation",
            "HasTransportCapacityConstraints",
            "HasTransportLeadTimes",
            "HasWarehouseCapacityConstraints",
            "IsMultiSite",
            "HasFinancialConstraints",
            "HasMultipleObjectives"
        };
    }

    private static IReadOnlyList<string>
        GetBroadlySupportedFeatureCodes()
    {
        return new[]
        {
            "HasDemand",
            "HasDeterministicDemand",
            "HasTimeVaryingDemand",
            "HasInitialInventory",
            "HasSafetyStockRequirements",
            "HasBacklogging",
            "HasLostSales",
            "HasProduction",
            "HasProductionCapacityConstraints",
            "HasSharedProductionCapacity",
            "HasTimeVaryingProductionCapacity",
            "HasSetupCosts",
            "HasSetupTimes",
            "HasStartUpCosts",
            "HasProductionLeadTimes",
            "HasMinimumLotSizes",
            "HasMaximumLotSizes",
            "HasLotSizeMultiples",
            "HasAdditionalCapacity",
            "HasPurchasing",
            "HasSupplierCapacityConstraints",
            "HasSupplierLeadTimes",
            "HasTransportation",
            "HasTransportCapacityConstraints",
            "HasTransportLeadTimes",
            "HasWarehouseCapacityConstraints",
            "IsMultiSite",
            "HasFinancialConstraints",
            "HasMultipleObjectives",
            "IsSingleItem",
            "IsMultiItem",
            "IsSingleLevel",
            "IsMultiLevel",
            "IsCapacitated"
        };
    }

    private static SolutionMethodKind ResolveMethodKind(
        SolutionMethodKind fallback,
        params string[] candidateNames)
    {
        ArgumentNullException.ThrowIfNull(
            candidateNames);

        foreach (string candidateName in candidateNames)
        {
            if (string.IsNullOrWhiteSpace(
                    candidateName))
            {
                continue;
            }

            if (Enum.TryParse(
                    candidateName.Trim(),
                    ignoreCase:
                        true,
                    out SolutionMethodKind parsedValue) &&
                Enum.IsDefined(
                    typeof(SolutionMethodKind),
                    parsedValue))
            {
                return parsedValue;
            }
        }

        return fallback;
    }
}