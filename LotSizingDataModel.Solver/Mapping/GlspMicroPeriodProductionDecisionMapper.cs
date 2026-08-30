using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

public sealed class GlspMicroPeriodProductionDecisionMapper : MathematicalDecisionMapperBase
{
    public override string Category => MathematicalDecisionCategory.MicroPeriodProduction;

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
        // Hydrated by GlspMicroPeriodSetupStateDecisionMapper using the complete result context.
    }
}
