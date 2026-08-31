using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketStartUpCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId =>
        "smallBucketProductionStartUpCost";

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

            if (characteristic.StartUpCost is null)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                double cost =
                    characteristic.StartUpCost[period];

                if (cost == 0.0)
                {
                    continue;
                }

                AddCostTerm(
                    context,
                    expressionBuilder,
                    ProductionStartUpDomainKeyFactory.CreateSmallBucketKey(
                        profile,
                        routing,
                        period),
                    cost);
            }
        }

        return ValueTask.CompletedTask;
    }
}
