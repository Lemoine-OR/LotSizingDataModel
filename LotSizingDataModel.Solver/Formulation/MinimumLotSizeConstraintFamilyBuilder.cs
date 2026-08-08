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
/// Builds minimum production-lot-size constraints.
/// </summary>
public sealed class MinimumLotSizeConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>Gets the family identifier.</summary>
    public override string ConstraintFamilyId => "minimumLotSize";

    /// <summary>Determines whether the family is enabled.</summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IncludeProductionSetups)
        {
            return false;
        }

        foreach (ProductionRouting routing in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.MinimumLotSize is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Builds minimum-lot-size constraints.</summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (ProductionRouting routing in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.MinimumLotSize is null)
            {
                continue;
            }

            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double minimum =
                    routing.MinimumLotSize.GetMinimumLotSize(period);

                if (minimum <= options.StructuralZeroTolerance)
                {
                    continue;
                }

                MathematicalVariable production =
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory.CreateProductionKey(
                            routing.Id,
                            period));

                MathematicalVariable setup =
                    context.GetVariable(
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.Setup)
                            .Add(MathematicalDomainKeySegment.Routing, routing.Id)
                            .Add(MathematicalDomainKeySegment.Period, period)
                            .Build());

                var expression =
                    new LinearExpressionBuilder()
                        .Add(production)
                        .Subtract(setup, minimum);

                AddConstraint(
                    context,
                    $"minimumLotSize_r{routing.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.GreaterThanOrEqual,
                    0.0,
                    description:
                        "Minimum production lot size when setup is active.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
