using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Links production quantities and positive-production activation to the
/// admissible setup state(s) of each small-bucket formulation.
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
        _kind switch
        {
            SmallBucketSchedulingFormulationKind.Dlsp =>
                "dlspProductionState",

            SmallBucketSchedulingFormulationKind.Cslp =>
                "cslpProductionState",

            SmallBucketSchedulingFormulationKind.Plsp =>
                "plspProductionState",

            _ =>
                throw new InvalidOperationException(
                    "Unknown small-bucket formulation kind.")
        };

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

                MathematicalVariable production =
                    GetVariable(
                        context,
                        ProductionKey(
                            routing.Id,
                            period));

                MathematicalVariable setup =
                    GetVariable(
                        context,
                        SetupKey(
                            routing.Id,
                            period));

                MathematicalVariable active =
                    GetVariable(
                        context,
                        ProductionActivationKey(
                            routing.Id,
                            period));

                if (
                    _kind ==
                    SmallBucketSchedulingFormulationKind.Dlsp)
                {
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
                        0.0);

                    continue;
                }

                AddConstraint(
                    context,
                    $"{Prefix()}ProductionActivation_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(
                            production,
                            consumption)
                        .Subtract(
                            active,
                            capacity)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        "Positive continuous production requires the mathematical production-activation flag.");

                var stateExpression =
                    new LinearExpressionBuilder()
                        .Add(active)
                        .Subtract(setup);

                if (
                    _kind ==
                        SmallBucketSchedulingFormulationKind.Plsp &&
                    period > 1)
                {
                    stateExpression.Subtract(
                        GetVariable(
                            context,
                            SetupKey(
                                routing.Id,
                                period - 1)));
                }

                AddConstraint(
                    context,
                    $"{Prefix()}ProductionState_r{routing.Id}_t{period}",
                    stateExpression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        _kind ==
                            SmallBucketSchedulingFormulationKind.Plsp
                            ? "PLSP production may use either the incoming or the outgoing setup state."
                            : "CSLP production requires the current setup state.");
            }
        }

        return ValueTask.CompletedTask;
    }

    private string Prefix() =>
        _kind ==
            SmallBucketSchedulingFormulationKind.Plsp
            ? "plsp"
            : "cslp";

    private static string ProductionKey(
        int routingId,
        int period) =>
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Production)
                .Add(
                    MathematicalDomainKeySegment.Routing,
                    routingId)
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();

    private static string SetupKey(
        int routingId,
        int period) =>
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Setup)
                .Add(
                    MathematicalDomainKeySegment.Routing,
                    routingId)
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();

    private static string ProductionActivationKey(
        int routingId,
        int period) =>
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory
                    .AuxiliarySmallBucketProductionActivation)
                .Add(
                    MathematicalDomainKeySegment.Routing,
                    routingId)
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();
}
