using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Common;

/// <summary>
/// Identifies the general type of method used to generate
/// a lot-sizing solution.
/// </summary>
/// <remarks>
/// This classification is independent of any specific solver,
/// software implementation or algorithm.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solutionMethodKind")]
public enum SolutionMethodKind
{
    /// <summary>
    /// The solution-generation method is unknown
    /// or has not been specified.
    /// </summary>
    [XmlEnum("unknown")]
    Unknown,

    /// <summary>
    /// An exact mathematical optimization method,
    /// such as branch-and-bound or branch-and-cut.
    /// </summary>
    [XmlEnum("exactOptimization")]
    ExactOptimization,

    /// <summary>
    /// A constructive heuristic that progressively builds
    /// a solution from an initially empty or partial state.
    /// </summary>
    [XmlEnum("constructiveHeuristic")]
    ConstructiveHeuristic,

    /// <summary>
    /// A local-search method that improves an existing
    /// solution by exploring neighboring solutions.
    /// </summary>
    [XmlEnum("localSearch")]
    LocalSearch,

    /// <summary>
    /// A metaheuristic such as tabu search, simulated annealing,
    /// variable-neighborhood search or a genetic algorithm.
    /// </summary>
    [XmlEnum("metaheuristic")]
    Metaheuristic,

    /// <summary>
    /// A method combining mathematical optimization
    /// and heuristic components.
    /// </summary>
    [XmlEnum("matheuristic")]
    Matheuristic,

    /// <summary>
    /// A method based on simulation or
    /// simulation-driven optimization.
    /// </summary>
    [XmlEnum("simulationOptimization")]
    SimulationOptimization,

    /// <summary>
    /// A method based primarily on machine learning,
    /// reinforcement learning or learned decision rules.
    /// </summary>
    [XmlEnum("machineLearning")]
    MachineLearning,

    /// <summary>
    /// A solution entered, constructed or modified manually.
    /// </summary>
    [XmlEnum("manual")]
    Manual,

    /// <summary>
    /// A solution imported from an external file,
    /// application or benchmark repository.
    /// </summary>
    [XmlEnum("imported")]
    Imported,

    /// <summary>
    /// A solution-generation method that does not fit
    /// any of the other categories.
    /// </summary>
    [XmlEnum("other")]
    Other
}