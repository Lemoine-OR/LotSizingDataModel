using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds the auxiliary integer multipliers required by
/// production lot-size-multiple constraints.
/// </summary>
/// <remarks>
/// For a routing-period pair with multiple <c>m</c>, an integer
/// variable <c>z[r,t] &gt;= 0</c> is created. The associated
/// constraint family later imposes:
/// <code>
/// production[r,t] = m[r,t] * z[r,t].
/// </code>
/// </remarks>
public sealed class LotSizeMultipleVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the auxiliary variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.AuxiliaryLotSizeMultiplier;

    /// <summary>
    /// Determines whether at least one routing defines a
    /// lot-size multiple.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        foreach (ProductionRouting routing
                 in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.LotSizeMultiple is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds non-negative integer lot-size multipliers.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (ProductionRouting routing
                 in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.LotSizeMultiple is null)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double multiple =
                    routing.LotSizeMultiple[period];

                if (!double.IsFinite(multiple) ||
                    multiple <= 0.0)
                {
                    throw new InvalidOperationException(
                        $"Lot-size multiple for routing " +
                        $"{routing.Id}, period {period}, must be " +
                        "finite and strictly positive.");
                }

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .AuxiliaryLotSizeMultiplier)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                context.AddVariable(
                    $"zLot_r{routing.Id}_t{period}",
                    domainKey,
                    MathematicalVariableType.Integer,
                    0.0,
                    double.PositiveInfinity,
                    $"Auxiliary integer multiplier for routing " +
                    $"{routing.Id} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
