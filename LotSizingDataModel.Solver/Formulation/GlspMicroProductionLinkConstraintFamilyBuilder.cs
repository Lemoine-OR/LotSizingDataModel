using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspMicroProductionLinkConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "glspMicroProductionLink";

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

        foreach (var microPeriod in profile.EnumerateMicroPeriods())
        {
            double capacity = workCenter.CapacityConstraint![microPeriod.MacroPeriod];
            foreach (ProductionRouting routing in routings)
            {
                ProductionCharacteristic characteristic =
                    GlspSchedulingData.GetCharacteristic(instance, routing, plantId, workCenter.Id);
                double consumption = characteristic.UnitCapacityConsumption![microPeriod.MacroPeriod];

                AddConstraint(
                    context,
                    $"glspProductionLink_r{routing.Id}_t{microPeriod.MacroPeriod}_s{microPeriod.MicroPeriodIndex}",
                    new LinearExpressionBuilder()
                        .Add(GetVariable(context,
                            GlspFormulationVariableKeyFactory.CreateMicroProductionKey(
                                plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod)), consumption)
                        .Subtract(GetVariable(context,
                            GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(
                                plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod)), capacity)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description: "GLSP production is possible only in the active setup state.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
