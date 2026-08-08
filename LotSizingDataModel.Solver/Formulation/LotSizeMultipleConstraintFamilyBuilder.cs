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
/// Enforces routing-specific production lot-size multiples.
/// </summary>
/// <remarks>
/// For each routing and period for which a
/// <see cref="LotSizingDataModel.Core.DecisionModel.Constraints.LotSizeMultiple"/>
/// exists, the generated equality is:
/// <code>
/// production[r,t] - multiple[r,t] * z[r,t] = 0,
/// </code>
/// where <c>z[r,t]</c> is a non-negative integer auxiliary
/// variable.
/// </remarks>
public sealed class LotSizeMultipleConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "lotSizeMultiple";

    /// <summary>
    /// Determines whether lot-size-multiple constraints are
    /// required.
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
    /// Builds all lot-size-multiple equalities.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
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

                MathematicalVariable production =
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory
                            .CreateProductionKey(
                                routing.Id,
                                period));

                MathematicalVariable multiplier =
                    context.GetVariable(
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory
                                .AuxiliaryLotSizeMultiplier)
                            .Add(
                                MathematicalDomainKeySegment.Routing,
                                routing.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build());

                var expression =
                    new LinearExpressionBuilder()
                        .Add(
                            production)
                        .Subtract(
                            multiplier,
                            multiple);

                AddConstraint(
                    context,
                    $"lotSizeMultiple_r{routing.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.Equal,
                    0.0,
                    description:
                        "Production quantity must be an integer " +
                        "multiple of the configured lot size.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
