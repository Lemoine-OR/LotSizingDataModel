using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspAggregateProductionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "glspAggregateProduction";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        (int plantId, var workCenter, var profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);
        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(instance, plantId, workCenter.Id);

        for (int period = 1; period <= instance.PlanningHorizon; period++)
        {
            var microPeriods = profile.EnumerateMicroPeriods()
                .Where(micro => micro.MacroPeriod == period)
                .ToArray();

            foreach (ProductionRouting routing in routings)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var expression = new LinearExpressionBuilder()
                    .Add(GetVariable(context,
                        StandardFormulationVariableKeyFactory.CreateProductionKey(routing.Id, period)));

                foreach (var microPeriod in microPeriods)
                {
                    expression.Subtract(GetVariable(context,
                        GlspFormulationVariableKeyFactory.CreateMicroProductionKey(
                            plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod)));
                }

                AddConstraint(
                    context,
                    $"glspAggregateProduction_r{routing.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.Equal,
                    0.0,
                    description: "Macro-period production equals the sum of its GLSP micro-period quantities.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
