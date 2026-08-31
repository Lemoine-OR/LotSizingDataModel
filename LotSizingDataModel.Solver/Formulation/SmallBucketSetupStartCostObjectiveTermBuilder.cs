using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketSetupStartCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId => "smallBucketSetupStartCost";

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

        foreach (ProductionRouting routing in instance.SupplyChain.ProductionRoutings)
        {
            var reference = routing.WorkCenters.Single();
            ProductionCharacteristic characteristic =
                instance.SupplyChain.ProductionCharacteristics.Single(candidate =>
                    candidate.ItemId == routing.ItemId &&
                    candidate.WorkCenter.PlantId == reference.PlantId &&
                    candidate.WorkCenter.WorkCenterId == reference.WorkCenterId);

            for (int period=1; period<=instance.PlanningHorizon; period++)
            {
                double cost = characteristic.FixedSetupCost?[period] ?? 0.0;
                if (cost == 0.0) continue;
                AddCostTerm(context,expressionBuilder,
                    SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(profile,routing,period),cost);
            }
        }
        return ValueTask.CompletedTask;
    }
}
