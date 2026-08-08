using System;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Consumes internal lot-size multiplier values without writing
/// them to the normalized lot-sizing solution.
/// </summary>
/// <remarks>
/// Lot-size multipliers are auxiliary integer variables required
/// by the MILP formulation. They are not business decisions and
/// therefore have no counterpart in
/// <see cref="LotSizingSolution"/>.
/// Registering this mapper allows strict known-category
/// validation to remain enabled.
/// </remarks>
public sealed class AuxiliaryLotSizeMultiplierDecisionMapper :
    MathematicalDecisionMapperBase
{
    /// <summary>
    /// Gets the mathematical category handled by this mapper.
    /// </summary>
    public override string Category =>
        MathematicalDecisionCategory.AuxiliaryLotSizeMultiplier;

    /// <summary>
    /// Intentionally ignores one auxiliary mathematical value.
    /// </summary>
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

        // Intentionally no-op:
        // the multiplier is a formulation variable only.
    }
}
