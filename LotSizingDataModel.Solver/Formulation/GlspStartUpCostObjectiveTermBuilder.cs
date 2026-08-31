using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspStartUpCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId =>
        "glspProductionStartUpCost";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.ProductionCharacteristics.Any(
            characteristic =>
                characteristic.StartUpCost is not null);
    }

    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var (plantId, workCenter, profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);

        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(
                instance,
                plantId,
                workCenter.Id);

        var ordered =
            profile.EnumerateMicroPeriods().ToArray();

        for (int index = 0;
             index < ordered.Length;
             index++)
        {
            int fixedFrom =
                GlspSequenceSemantics.GetFixedPredecessorItemId(
                    profile,
                    index);

            bool reset =
                index > 0 &&
                GlspSequenceSemantics.IsResetBoundary(
                    profile,
                    ordered[index - 1],
                    ordered[index]);

            foreach (ProductionRouting routing
                     in routings)
            {
                ProductionCharacteristic characteristic =
                    GlspSchedulingData.GetCharacteristic(
                        instance,
                        routing,
                        plantId,
                        workCenter.Id);

                double cost =
                    characteristic.StartUpCost?[
                        ordered[index].MacroPeriod] ?? 0.0;

                if (cost == 0.0)
                {
                    continue;
                }

                AddCostTerm(
                    context,
                    expressionBuilder,
                    ProductionStartUpDomainKeyFactory.CreateGlspKey(
                        plantId,
                        workCenter.Id,
                        routing.Id,
                        routing.ItemId,
                        ordered[index],
                        fixedFrom,
                        reset),
                    cost);
            }
        }

        return ValueTask.CompletedTask;
    }
}
