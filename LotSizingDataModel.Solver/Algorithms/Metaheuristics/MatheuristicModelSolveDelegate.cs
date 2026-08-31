using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public delegate MathematicalModelSolveResult
    MatheuristicModelSolveDelegate(
        MathematicalModel model,
        int nodeLimit,
        CancellationToken cancellationToken);
