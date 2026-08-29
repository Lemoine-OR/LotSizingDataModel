using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>Builds maximum production-lot-size constraints.</summary>
public sealed class MaximumLotSizeConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "maximumLotSize";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.ProductionRoutings.Any(
            routing => routing.MaximumLotSize is not null);
    }

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.MaximumLotSize is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double maximum =
                    routing.MaximumLotSize
                        .GetMaximumLotSize(period);

                MathematicalVariable production =
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory
                            .CreateProductionKey(
                                routing.Id,
                                period));

                AddConstraint(
                    context,
                    $"maximumLotSize_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(production)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    maximum,
                    description:
                        "Maximum production lot size for the routing.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
