using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Adapters;

/// <summary>
/// Defines a dynamically discoverable solver adapter capable of
/// solving solver-independent mathematical models.
/// </summary>
/// <remarks>
/// This interface bridges the generic adapter-discovery contract
/// with the mathematical-model solving contract. Solver-specific
/// plugins such as CPLEX, Gurobi, FICO Xpress, and COIN-OR CBC
/// should implement this interface.
/// </remarks>
public interface IMathematicalModelSolverAdapter :
    ISolverAdapter,
    IMathematicalModelSolver
{
}
