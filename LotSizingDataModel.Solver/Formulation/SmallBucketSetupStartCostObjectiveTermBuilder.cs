using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Charges fixed production setup cost only when a small-bucket setup state
/// starts.
/// </summary>
public sealed class SmallBucketSetupStartCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId =>
        "smallBucketSetupStartCost";

    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            var reference =
                routing.WorkCenters.Single();

            ProductionCharacteristic characteristic =
                instance.SupplyChain.ProductionCharacteristics
                    .Single(
                        candidate =>
                            candidate.ItemId == routing.ItemId &&
                            candidate.WorkCenter.PlantId ==
                                reference.PlantId &&
                            candidate.WorkCenter.WorkCenterId ==
                                reference.WorkCenterId);

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double cost =
                    characteristic.FixedSetupCost?[period] ??
                    0.0;

                if (cost == 0.0)
                {
                    continue;
                }

                string key =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .AuxiliarySchedulingSetupStart)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddCostTerm(
                    context,
                    expressionBuilder,
                    key,
                    cost);
            }
        }

        return ValueTask.CompletedTask;
    }
}
