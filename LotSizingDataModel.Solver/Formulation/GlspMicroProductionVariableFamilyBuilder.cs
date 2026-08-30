using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspMicroProductionVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId => "glspMicroProduction";

    protected override ValueTask BuildFamilyAsync(
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
            foreach (ProductionRouting routing in routings)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddNonNegativeContinuousVariable(
                    context,
                    $"glspX_r{routing.Id}_t{microPeriod.MacroPeriod}_s{microPeriod.MicroPeriodIndex}",
                    GlspFormulationVariableKeyFactory.CreateMicroProductionKey(
                        plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod),
                    double.PositiveInfinity,
                    "GLSP production quantity in one micro-period.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
