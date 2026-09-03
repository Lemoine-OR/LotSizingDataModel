using System;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

public sealed class AuxiliaryProductionFamilySetupDecisionMapper :
    MathematicalDecisionMapperBase
{
    public override string Category =>
        MathematicalDecisionCategory.AuxiliaryProductionFamilySetup;

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

        // Intentionally not persisted: family setup activation is derivable
        // from item-level setup decisions in the current semantics.
    }
}
