using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps mathematical cash-balance variables to the normalized financial trace.
/// </summary>
public sealed class CashBalanceDecisionMapper :
    MathematicalDecisionMapperBase
{
    public override string Category =>
        MathematicalDecisionCategory.CashBalance;

    protected override void MapValue(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        MathematicalDomainKey domainKey,
        MathematicalVariableValue variableValue)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(domainKey);
        ArgumentNullException.ThrowIfNull(variableValue);

        int period =
            domainKey.GetRequiredInt32(
                MathematicalDomainKeySegment.Period);

        solution.SetCashBalance(
            period,
            variableValue.Value);
    }
}
