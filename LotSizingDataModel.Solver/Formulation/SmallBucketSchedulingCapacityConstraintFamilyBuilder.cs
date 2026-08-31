using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds CSLP/PLSP capacity with additive production, setup-start and
/// start-up time contributions.
/// </summary>
public sealed class SmallBucketSchedulingCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "smallBucketSchedulingCapacity";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var entry =
            instance.SupplyChain.Plants
                .SelectMany(
                    plant =>
                        plant.WorkCenters.Select(
                            workCenter =>
                                (PlantId:plant.Id,WorkCenter:workCenter)))
                .Single(
                    candidate =>
                        candidate.WorkCenter.SchedulingProfile is not null);

        var profile =
            entry.WorkCenter.SchedulingProfile!;

        for (int period=1;
             period<=instance.PlanningHorizon;
             period++)
        {
            var expression =
                new LinearExpressionBuilder();

            foreach (ProductionRouting routing
                     in instance.SupplyChain.ProductionRoutings)
            {
                var reference =
                    routing.WorkCenters.Single();

                ProductionCharacteristic characteristic =
                    instance.SupplyChain.ProductionCharacteristics.Single(
                        candidate =>
                            candidate.ItemId==routing.ItemId &&
                            candidate.WorkCenter.PlantId==reference.PlantId &&
                            candidate.WorkCenter.WorkCenterId==reference.WorkCenterId);

                string productionKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Production)
                        .Add(MathematicalDomainKeySegment.Routing,routing.Id)
                        .Add(MathematicalDomainKeySegment.Period,period)
                        .Build();

                expression.Add(
                    GetVariable(context,productionKey),
                    characteristic.UnitCapacityConsumption![period]);

                double setupTime =
                    characteristic.SetupTime?[period] ?? 0.0;

                if(setupTime>0.0)
                {
                    expression.Add(
                        GetVariable(
                            context,
                            SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(
                                profile,
                                routing,
                                period)),
                        setupTime);
                }

                double startUpTime =
                    characteristic.StartUpTime?[period] ?? 0.0;

                if(startUpTime>0.0)
                {
                    expression.Add(
                        GetVariable(
                            context,
                            ProductionStartUpDomainKeyFactory.CreateSmallBucketKey(
                                profile,
                                routing,
                                period)),
                        startUpTime);
                }
            }

            string additionalKey =
                new MathematicalDomainKeyBuilder(
                    MathematicalDecisionCategory.WorkCenterAdditionalCapacity)
                    .Add(MathematicalDomainKeySegment.Plant,entry.PlantId)
                    .Add(MathematicalDomainKeySegment.WorkCenter,entry.WorkCenter.Id)
                    .Add(MathematicalDomainKeySegment.Period,period)
                    .Build();

            if(context.VariableRegistry.TryGet(
                    additionalKey,
                    out MathematicalVariable? additional) &&
                additional is not null)
            {
                expression.Subtract(additional);
            }

            AddConstraint(
                context,
                $"smallBucketCapacity_p{entry.PlantId}_w{entry.WorkCenter.Id}_t{period}",
                expression.Build(),
                MathematicalConstraintSense.LessThanOrEqual,
                entry.WorkCenter.CapacityConstraint![period]);
        }

        return ValueTask.CompletedTask;
    }
}
