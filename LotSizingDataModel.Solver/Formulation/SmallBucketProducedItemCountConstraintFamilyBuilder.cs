using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Enforces the real period-dependent MaximumProducedItemCount parameter from
/// positive-production activation variables.
/// </summary>
public sealed class SmallBucketProducedItemCountConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "smallBucketProducedItemCount";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        ProductionSchedulingProfile profile =
            instance.SupplyChain.WorkCenters
                .Single(
                    workCenter =>
                        workCenter.SchedulingProfile is not null)
                .SchedulingProfile!;

        MaximumProducedItemCount limit =
            profile.MaximumProducedItemCount ??
            throw new InvalidOperationException(
                "Executable small-bucket formulations require MaximumProducedItemCount.");

        for (
            int period = 1;
            period <= instance.PlanningHorizon;
            period++)
        {
            var expression =
                new LinearExpressionBuilder();

            foreach (
                var routing
                in instance.SupplyChain.ProductionRoutings)
            {
                string key =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .AuxiliarySmallBucketProductionActivation)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                expression.Add(
                    GetVariable(
                        context,
                        key));
            }

            AddConstraint(
                context,
                $"smallBucketProducedItemCount_t{period}",
                expression.Build(),
                MathematicalConstraintSense.LessThanOrEqual,
                limit.GetCount(period),
                description:
                    "Maximum number of distinct positively produced items in the bucket.");
        }

        return ValueTask.CompletedTask;
    }
}
