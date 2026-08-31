using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Accepts the mathematical-only production start-up category.
/// </summary>
/// <remarks>
/// Start-up is reconstructed independently by the checker from normalized
/// setup/scheduling state; it is therefore not duplicated in LotSizingSolution.
/// </remarks>
public sealed class AuxiliaryProductionStartUpDecisionMapper :
    MathematicalDecisionMapperBase
{
    public override string Category =>
        MathematicalDecisionCategory.AuxiliaryProductionStartUp;

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
    }
}
