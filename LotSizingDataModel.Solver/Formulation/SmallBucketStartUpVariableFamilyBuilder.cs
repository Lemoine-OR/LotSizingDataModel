using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketStartUpVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        "smallBucketProductionStartUp";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.ProductionCharacteristics.Any(
            characteristic =>
                characteristic.StartUpCost is not null ||
                characteristic.StartUpTime is not null);
    }

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

        foreach (ProductionRouting routing
                 in instance.SupplyChain.ProductionRoutings)
        {
            var reference =
                routing.WorkCenters.Single();

            ProductionCharacteristic characteristic =
                instance.SupplyChain.ProductionCharacteristics.Single(
                    candidate =>
                        candidate.ItemId == routing.ItemId &&
                        candidate.WorkCenter.PlantId == reference.PlantId &&
                        candidate.WorkCenter.WorkCenterId == reference.WorkCenterId);

            if (characteristic.StartUpCost is null &&
                characteristic.StartUpTime is null)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddBinaryVariable(
                    context,
                    $"smallBucketStartUp_r{routing.Id}_t{period}",
                    ProductionStartUpDomainKeyFactory.CreateSmallBucketKey(
                        profile,
                        routing,
                        period),
                    "Small-bucket production start-up occurrence.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
