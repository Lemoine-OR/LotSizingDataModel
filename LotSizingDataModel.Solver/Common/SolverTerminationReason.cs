using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Describes why a solver execution terminated.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverTerminationReason")]
public enum SolverTerminationReason
{
    /// <summary>
    /// The termination reason is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// An optimal solution was proven.
    /// </summary>
    Optimal = 1,

    /// <summary>
    /// A feasible solution was found, but optimality was not
    /// proven.
    /// </summary>
    Feasible = 2,

    /// <summary>
    /// The mathematical model was proven infeasible.
    /// </summary>
    Infeasible = 3,

    /// <summary>
    /// The mathematical model was proven unbounded.
    /// </summary>
    Unbounded = 4,

    /// <summary>
    /// The solver could not distinguish between infeasibility
    /// and unboundedness.
    /// </summary>
    InfeasibleOrUnbounded = 5,

    /// <summary>
    /// The configured time limit was reached.
    /// </summary>
    TimeLimit = 6,

    /// <summary>
    /// The configured node limit was reached.
    /// </summary>
    NodeLimit = 7,

    /// <summary>
    /// The configured iteration limit was reached.
    /// </summary>
    IterationLimit = 8,

    /// <summary>
    /// The configured solution limit was reached.
    /// </summary>
    SolutionLimit = 9,

    /// <summary>
    /// The configured memory limit was reached.
    /// </summary>
    MemoryLimit = 10,

    /// <summary>
    /// The configured objective limit was reached.
    /// </summary>
    ObjectiveLimit = 11,

    /// <summary>
    /// The configured relative optimality-gap limit was
    /// reached.
    /// </summary>
    RelativeGapLimit = 12,

    /// <summary>
    /// The configured absolute optimality-gap limit was
    /// reached.
    /// </summary>
    AbsoluteGapLimit = 13,

    /// <summary>
    /// The execution was interrupted by the user or by a
    /// cancellation request.
    /// </summary>
    UserInterrupted = 14,

    /// <summary>
    /// The solver stopped because of numerical difficulties.
    /// </summary>
    NumericalDifficulty = 15,

    /// <summary>
    /// The solver could not run because a valid license was
    /// unavailable.
    /// </summary>
    LicenseError = 16,

    /// <summary>
    /// The requested solver was unavailable on the current
    /// computer.
    /// </summary>
    SolverUnavailable = 17,

    /// <summary>
    /// The mathematical model was invalid or unsupported.
    /// </summary>
    ModelError = 18,

    /// <summary>
    /// The solver stopped because of an internal error.
    /// </summary>
    InternalError = 19
}
