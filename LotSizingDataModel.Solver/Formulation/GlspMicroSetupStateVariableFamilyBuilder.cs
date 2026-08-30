using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspMicroSetupStateVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId => "glspMicroSetupState";

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
                AddBinaryVariable(
                    context,
                    $"glspY_r{routing.Id}_t{microPeriod.MacroPeriod}_s{microPeriod.MicroPeriodIndex}",
                    GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(
                        plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod),
                    "GLSP setup state active in one micro-period.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
