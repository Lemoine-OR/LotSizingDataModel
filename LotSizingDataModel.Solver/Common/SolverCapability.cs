using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Identifies an optional capability implemented by a solver
/// adapter.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverCapability")]
public enum SolverCapability
{
    /// <summary>
    /// No capability has been specified.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Solves continuous linear programming models.
    /// </summary>
    LinearProgramming = 1,

    /// <summary>
    /// Solves mixed-integer linear programming models.
    /// </summary>
    MixedIntegerLinearProgramming = 2,

    /// <summary>
    /// Solves quadratic programming models.
    /// </summary>
    QuadraticProgramming = 3,

    /// <summary>
    /// Solves mixed-integer quadratic programming models.
    /// </summary>
    MixedIntegerQuadraticProgramming = 4,

    /// <summary>
    /// Reports solver progress during optimization.
    /// </summary>
    ProgressCallbacks = 5,

    /// <summary>
    /// Reports newly discovered incumbent solutions.
    /// </summary>
    IncumbentCallbacks = 6,

    /// <summary>
    /// Supports user-defined cut callbacks.
    /// </summary>
    UserCutCallbacks = 7,

    /// <summary>
    /// Supports lazy-constraint callbacks.
    /// </summary>
    LazyConstraintCallbacks = 8,

    /// <summary>
    /// Supports heuristic-solution callbacks.
    /// </summary>
    HeuristicCallbacks = 9,

    /// <summary>
    /// Supports callbacks related to branching decisions.
    /// </summary>
    BranchCallbacks = 10,

    /// <summary>
    /// Supports callbacks related to search-tree nodes.
    /// </summary>
    NodeCallbacks = 11,

    /// <summary>
    /// Supports cooperative interruption of a running solve.
    /// </summary>
    Interruption = 12,

    /// <summary>
    /// Supports warm starts or MIP starts.
    /// </summary>
    WarmStart = 13,

    /// <summary>
    /// Supports multiple solution retrieval.
    /// </summary>
    SolutionPool = 14,

    /// <summary>
    /// Supports exporting a model in LP format.
    /// </summary>
    LpExport = 15,

    /// <summary>
    /// Supports exporting a model in MPS format.
    /// </summary>
    MpsExport = 16,

    /// <summary>
    /// Supports importing a model from LP format.
    /// </summary>
    LpImport = 17,

    /// <summary>
    /// Supports importing a model from MPS format.
    /// </summary>
    MpsImport = 18,

    /// <summary>
    /// Supports infeasibility analysis such as IIS extraction.
    /// </summary>
    InfeasibilityAnalysis = 19,

    /// <summary>
    /// Supports conflict refinement.
    /// </summary>
    ConflictRefinement = 20,

    /// <summary>
    /// Supports deterministic parallel optimization.
    /// </summary>
    DeterministicParallelism = 21,

    /// <summary>
    /// Supports solver-native log capture.
    /// </summary>
    LogCapture = 22,

    /// <summary>
    /// Supports obtaining the final best bound and optimality
    /// gap.
    /// </summary>
    OptimalityGapReporting = 23,

    /// <summary>
    /// Supports obtaining explored-node and iteration counts.
    /// </summary>
    SearchStatistics = 24
}
