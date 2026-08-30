using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspChangeoverCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId => "glspChangeoverCost";

    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        (int plantId, var workCenter, ProductionSchedulingProfile profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);
        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(instance, plantId, workCenter.Id);
        var microPeriods = profile.EnumerateMicroPeriods().ToArray();

        for (int index = 1; index < microPeriods.Length; index++)
        {
            var current = microPeriods[index];
            foreach (ProductionRouting from in routings)
            {
                foreach (ProductionRouting to in routings)
                {
                    if (from.ItemId == to.ItemId) continue;
                    ProductionChangeover? changeover =
                        GlspSchedulingData.FindChangeover(profile, from.ItemId, to.ItemId);
                    double cost = changeover?.ChangeoverCost?[current.MacroPeriod] ?? 0.0;
                    if (cost == 0.0) continue;

                    AddCostTerm(
                        context,
                        expressionBuilder,
                        GlspFormulationVariableKeyFactory.CreateChangeoverKey(
                            plantId, workCenter.Id, from.ItemId, to.ItemId, current),
                        cost);
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
