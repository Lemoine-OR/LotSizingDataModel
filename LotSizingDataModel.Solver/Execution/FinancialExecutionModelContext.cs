using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Execution;

public sealed class FinancialExecutionModelContext
{
    public required MathematicalModel Model { get; init; }

    public required LinearExpression EconomicCriterion { get; init; }

    public LinearExpression? FinancialCriterion { get; init; }

    public int FinancialHorizon { get; init; }
}
