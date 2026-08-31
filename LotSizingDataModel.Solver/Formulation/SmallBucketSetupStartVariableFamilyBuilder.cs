using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketSetupStartVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId => "smallBucketSetupStart";

    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        ProductionSchedulingProfile profile =
            instance.SupplyChain.WorkCenters
                .Single(workCenter => workCenter.SchedulingProfile is not null)
                .SchedulingProfile!;

        foreach (var routing in instance.SupplyChain.ProductionRoutings)
        {
            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddBinaryVariable(
                    context,
                    $"smallBucketSetupStart_r{routing.Id}_t{period}",
                    SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(
                        profile, routing, period),
                    "Mathematical-only small-bucket setup-start occurrence.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
