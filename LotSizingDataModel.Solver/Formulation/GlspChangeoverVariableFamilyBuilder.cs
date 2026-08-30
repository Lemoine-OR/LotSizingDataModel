using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspChangeoverVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId => "glspChangeover";

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
        var microPeriods = profile.EnumerateMicroPeriods().ToArray();

        for (int index = 1; index < microPeriods.Length; index++)
        {
            var current = microPeriods[index];
            foreach (ProductionRouting from in routings)
            {
                foreach (ProductionRouting to in routings)
                {
                    if (from.ItemId == to.ItemId) continue;
                    cancellationToken.ThrowIfCancellationRequested();
                    AddBinaryVariable(
                        context,
                        $"glspZ_i{from.ItemId}_j{to.ItemId}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}",
                        GlspFormulationVariableKeyFactory.CreateChangeoverKey(
                            plantId, workCenter.Id, from.ItemId, to.ItemId, current),
                        "GLSP changeover into the current micro-period.");
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
