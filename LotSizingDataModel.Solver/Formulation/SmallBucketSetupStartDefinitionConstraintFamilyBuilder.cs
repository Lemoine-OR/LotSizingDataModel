using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines setup-start variables from successive setup states.
/// </summary>
public sealed class SmallBucketSetupStartDefinitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "smallBucketSetupStartDefinition";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            var routing
            in instance.SupplyChain.ProductionRoutings)
        {
            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var setup =
                    GetVariable(
                        context,
                        SetupKey(
                            routing.Id,
                            period));

                var start =
                    GetVariable(
                        context,
                        StartKey(
                            routing.Id,
                            period));

                if (period == 1)
                {
                    AddConstraint(
                        context,
                        $"smallBucketSetupStartInitial_r{routing.Id}",
                        new LinearExpressionBuilder()
                            .Add(start)
                            .Subtract(setup)
                            .Build(),
                        MathematicalConstraintSense.Equal,
                        0.0,
                        description:
                            "Without an initial setup state, first-period setup state starts immediately.");

                    continue;
                }

                var previousSetup =
                    GetVariable(
                        context,
                        SetupKey(
                            routing.Id,
                            period - 1));

                AddConstraint(
                    context,
                    $"smallBucketSetupStartLower_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(start)
                        .Subtract(setup)
                        .Add(previousSetup)
                        .Build(),
                    MathematicalConstraintSense.GreaterThanOrEqual,
                    0.0);

                AddConstraint(
                    context,
                    $"smallBucketSetupStartUpperState_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(start)
                        .Subtract(setup)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0);

                AddConstraint(
                    context,
                    $"smallBucketSetupStartUpperPrevious_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(start)
                        .Add(previousSetup)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    1.0);
            }
        }

        return ValueTask.CompletedTask;
    }

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

    private static string StartKey(
        int routingId,
        int period) =>
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory
                    .AuxiliarySchedulingSetupStart)
                .Add(
                    MathematicalDomainKeySegment.Routing,
                    routingId)
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();
}
