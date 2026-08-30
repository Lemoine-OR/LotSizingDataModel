using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Consumes mathematical-only setup-start values.
/// </summary>
public sealed class AuxiliarySchedulingSetupStartDecisionMapper :
    MathematicalDecisionMapperBase
{
    public override string Category =>
        MathematicalDecisionCategory
            .AuxiliarySchedulingSetupStart;

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

        // Setup starts are exactly derivable from persisted successive setup
        // states for the currently executable no-initial-state formulation.
    }
}
