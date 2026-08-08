using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Identifies the current stage of a solver execution.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverProgressStage")]
public enum SolverProgressStage
{
    /// <summary>
    /// The execution stage is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The solver request is being initialized.
    /// </summary>
    Initializing = 1,

    /// <summary>
    /// The mathematical model is being generated.
    /// </summary>
    BuildingModel = 2,

    /// <summary>
    /// The generated model is being transferred to the
    /// selected solver.
    /// </summary>
    LoadingModel = 3,

    /// <summary>
    /// The solver presolve phase is running.
    /// </summary>
    Presolving = 4,

    /// <summary>
    /// The root linear relaxation is being solved.
    /// </summary>
    SolvingRootRelaxation = 5,

    /// <summary>
    /// The branch-and-bound or branch-and-cut search is
    /// running.
    /// </summary>
    Searching = 6,

    /// <summary>
    /// A new incumbent solution has been found.
    /// </summary>
    IncumbentFound = 7,

    /// <summary>
    /// The solver is processing a callback.
    /// </summary>
    ProcessingCallback = 8,

    /// <summary>
    /// The solver is finalizing the optimization result.
    /// </summary>
    Finalizing = 9,

    /// <summary>
    /// The solver execution completed successfully.
    /// </summary>
    Completed = 10,

    /// <summary>
    /// The solver execution was interrupted.
    /// </summary>
    Interrupted = 11,

    /// <summary>
    /// The solver execution failed.
    /// </summary>
    Failed = 12
}
