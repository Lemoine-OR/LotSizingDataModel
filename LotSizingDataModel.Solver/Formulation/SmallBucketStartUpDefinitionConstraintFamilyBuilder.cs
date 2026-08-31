using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Synchronizes the distinct start-up event with the exact small-bucket
/// setup-start occurrence.
/// </summary>
public sealed class SmallBucketStartUpDefinitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "smallBucketProductionStartUpDefinition";

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

    protected override ValueTask BuildConstraintsAsync(
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

                MathematicalVariable startUp =
                    GetVariable(
                        context,
                        ProductionStartUpDomainKeyFactory.CreateSmallBucketKey(
                            profile,
                            routing,
                            period));

                MathematicalVariable setupStart =
                    GetVariable(
                        context,
                        SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(
                            profile,
                            routing,
                            period));

                AddConstraint(
                    context,
                    $"smallBucketProductionStartUpDefinition_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(startUp)
                        .Subtract(setupStart)
                        .Build(),
                    MathematicalConstraintSense.Equal,
                    0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
