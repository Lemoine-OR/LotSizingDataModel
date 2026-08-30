using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Links production quantities to small-bucket setup states.
/// </summary>
public sealed class SmallBucketProductionStateConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    private readonly SmallBucketSchedulingFormulationKind _kind;

    public SmallBucketProductionStateConstraintFamilyBuilder(
        SmallBucketSchedulingFormulationKind kind)
    {
        _kind = kind;
    }

    public override string ConstraintFamilyId =>
        _kind ==
            SmallBucketSchedulingFormulationKind.Dlsp
            ? "dlspProductionState"
            : "cslpProductionState";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var schedulingWorkCenter =
            instance.SupplyChain.WorkCenters
                .Single(
                    workCenter =>
                        workCenter.SchedulingProfile is not null);

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

                double capacity =
                    schedulingWorkCenter
                        .CapacityConstraint![period];

                double consumption =
                    characteristic
                        .UnitCapacityConsumption![period];

                var production =
                    GetVariable(
                        context,
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.Production)
                            .Add(
                                MathematicalDomainKeySegment.Routing,
                                routing.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build());

                var setup =
                    GetVariable(
                        context,
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.Setup)
                            .Add(
                                MathematicalDomainKeySegment.Routing,
                                routing.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build());

                if (
                    _kind ==
                    SmallBucketSchedulingFormulationKind.Cslp)
                {
                    AddConstraint(
                        context,
                        $"cslpProductionState_r{routing.Id}_t{period}",
                        new LinearExpressionBuilder()
                            .Add(
                                production,
                                consumption)
                            .Subtract(
                                setup,
                                capacity)
                            .Build(),
                        MathematicalConstraintSense.LessThanOrEqual,
                        0.0,
                        description:
                            "CSLP production is continuous but requires the corresponding setup state.");

                    continue;
                }

                var active =
                    GetVariable(
                        context,
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory
                                .AuxiliarySmallBucketProductionActivation)
                            .Add(
                                MathematicalDomainKeySegment.Routing,
                                routing.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build());

                AddConstraint(
                    context,
                    $"dlspProductionFullBucket_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(
                            production,
                            consumption)
                        .Subtract(
                            active,
                            capacity)
                        .Build(),
                    MathematicalConstraintSense.Equal,
                    0.0,
                    description:
                        "DLSP production is either zero or the complete available bucket capacity.");

                AddConstraint(
                    context,
                    $"dlspProductionRequiresSetup_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(active)
                        .Subtract(setup)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        "DLSP full-bucket production requires the corresponding setup state.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
