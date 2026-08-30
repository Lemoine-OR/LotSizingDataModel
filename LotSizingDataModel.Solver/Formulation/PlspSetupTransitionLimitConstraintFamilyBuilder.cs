using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Enforces period-specific zero-transition PLSP buckets. A limit of one is
/// already intrinsic to the incoming/outgoing state representation.
/// </summary>
public sealed class PlspSetupTransitionLimitConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "plspSetupTransitionLimit";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        ProductionSchedulingProfile profile =
            instance.SupplyChain.WorkCenters
                .Single(
                    workCenter =>
                        workCenter.SchedulingProfile is not null)
                .SchedulingProfile!;

        MaximumSetupCount limit =
            profile.MaximumSetupCount ??
            throw new InvalidOperationException(
                "Executable PLSP requires MaximumSetupCount.");

        for (
            int period = 2;
            period <= instance.PlanningHorizon;
            period++)
        {
            if (limit.GetCount(period) != 0)
            {
                continue;
            }

            foreach (
                var routing
                in instance.SupplyChain.ProductionRoutings)
            {
                AddConstraint(
                    context,
                    $"plspNoSetupTransition_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder()
                        .Add(
                            GetVariable(
                                context,
                                SetupKey(
                                    routing.Id,
                                    period)))
                        .Subtract(
                            GetVariable(
                                context,
                                SetupKey(
                                    routing.Id,
                                    period - 1)))
                        .Build(),
                    MathematicalConstraintSense.Equal,
                    0.0,
                    description:
                        "A zero transition limit preserves the incoming PLSP setup state.");
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
}
