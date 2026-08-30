using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

public sealed class AuxiliaryMicroPeriodChangeoverDecisionMapper : MathematicalDecisionMapperBase
{
    public override string Category => MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover;

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
        // Mathematical-only: derivable from consecutive normalized setup states.
    }
}
