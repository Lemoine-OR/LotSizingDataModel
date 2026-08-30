using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Enforces at most one active routing setup state per small bucket.
/// </summary>
public sealed class SmallBucketSingleSetupStateConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "smallBucketSingleSetupState";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
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
                        MathematicalDecisionCategory.Setup)
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
                $"smallBucketSingleSetupState_t{period}",
                expression.Build(),
                MathematicalConstraintSense.LessThanOrEqual,
                1.0,
                description:
                    "At most one routing setup state is active in a small bucket.");
        }

        return ValueTask.CompletedTask;
    }
}
