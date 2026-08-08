using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Identifies a supported mathematical optimization solver.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverKind")]
public enum SolverKind
{
    /// <summary>
    /// No solver has been specified.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The solver is selected automatically from the
    /// adapters available on the current computer.
    /// </summary>
    Automatic = 1,

    /// <summary>
    /// IBM ILOG CPLEX Optimization Studio.
    /// </summary>
    Cplex = 2,

    /// <summary>
    /// Gurobi Optimizer.
    /// </summary>
    Gurobi = 3,

    /// <summary>
    /// FICO Xpress Optimizer.
    /// </summary>
    Xpress = 4,

    /// <summary>
    /// COIN-OR CBC mixed-integer programming solver.
    /// </summary>
    CoinOrCbc = 5
}
