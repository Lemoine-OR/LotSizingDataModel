using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspMacroCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "glspMacroCapacity";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        (int plantId, var workCenter, ProductionSchedulingProfile profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);
        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(instance, plantId, workCenter.Id);
        var allMicroPeriods = profile.EnumerateMicroPeriods().ToArray();

        for (int period = 1; period <= instance.PlanningHorizon; period++)
        {
            var expression = new LinearExpressionBuilder();

            foreach (var microPeriod in allMicroPeriods.Where(micro => micro.MacroPeriod == period))
            {
                foreach (ProductionRouting routing in routings)
                {
                    ProductionCharacteristic characteristic =
                        GlspSchedulingData.GetCharacteristic(instance, routing, plantId, workCenter.Id);
                    expression.Add(GetVariable(context,
                        GlspFormulationVariableKeyFactory.CreateMicroProductionKey(
                            plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod)),
                        characteristic.UnitCapacityConsumption![period]);
                }

                int globalIndex = Array.FindIndex(allMicroPeriods,
                    candidate => candidate.RefersToSameMicroPeriod(microPeriod));
                if (globalIndex <= 0) continue;

                foreach (ProductionRouting from in routings)
                {
                    foreach (ProductionRouting to in routings)
                    {
                        if (from.ItemId == to.ItemId) continue;
                        ProductionChangeover? changeover =
                            GlspSchedulingData.FindChangeover(profile, from.ItemId, to.ItemId);
                        double time = changeover?.ChangeoverTime?[period] ?? 0.0;
                        if (time <= 0.0) continue;

                        expression.Add(GetVariable(context,
                            GlspFormulationVariableKeyFactory.CreateChangeoverKey(
                                plantId, workCenter.Id, from.ItemId, to.ItemId, microPeriod)), time);
                    }
                }
            }

            AddConstraint(
                context,
                $"glspMacroCapacity_p{plantId}_w{workCenter.Id}_t{period}",
                expression.Build(),
                MathematicalConstraintSense.LessThanOrEqual,
                workCenter.CapacityConstraint![period],
                description: "GLSP macro-period capacity includes micro production and sequence-dependent changeover time.");
        }

        return ValueTask.CompletedTask;
    }
}
