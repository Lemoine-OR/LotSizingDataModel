using System;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Consumes mathematical family-setup values without persisting
/// them in LotSizingSolution.
/// </summary>
/// <remarks>
/// In the current scope there is no family setup cost and no
/// independent family-state constraint. The normalized solution
/// therefore derives the canonical family activation as the OR of
/// member item setup decisions. Persisting w[f,t] would duplicate
/// derivable information.
/// </remarks>
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

        // Intentionally no-op: normalized family setup activation
        // is derived from item setup decisions.
    }
}
