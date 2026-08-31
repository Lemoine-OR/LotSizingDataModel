using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketSetupStartDefinitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "smallBucketSetupStartDefinition";

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

        foreach (var routing in instance.SupplyChain.ProductionRoutings)
        {
            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                MathematicalVariable setup = GetVariable(context, SetupKey(routing.Id, period));
                MathematicalVariable start = GetVariable(
                    context,
                    SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(
                        profile, routing, period));

                if (period == 1)
                {
                    if (profile.HasInitialSetupState &&
                        profile.InitialSetupItemId == routing.ItemId)
                    {
                        AddConstraint(
                            context,
                            $"smallBucketSetupStartInitialSame_r{routing.Id}",
                            new LinearExpressionBuilder().Add(start).Build(),
                            MathematicalConstraintSense.Equal,
                            0.0);
                    }
                    else
                    {
                        AddConstraint(
                            context,
                            $"smallBucketSetupStartInitial_r{routing.Id}",
                            new LinearExpressionBuilder().Add(start).Subtract(setup).Build(),
                            MathematicalConstraintSense.Equal,
                            0.0);
                    }
                    continue;
                }

                if (profile.SetupCarryOverPolicy == SetupCarryOverPolicy.Forbidden)
                {
                    AddConstraint(
                        context,
                        $"smallBucketSetupStartReset_r{routing.Id}_t{period}",
                        new LinearExpressionBuilder().Add(start).Subtract(setup).Build(),
                        MathematicalConstraintSense.Equal,
                        0.0);
                    continue;
                }

                MathematicalVariable previous = GetVariable(context, SetupKey(routing.Id, period - 1));
                AddConstraint(context,$"smallBucketSetupStartLower_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder().Add(start).Subtract(setup).Add(previous).Build(),
                    MathematicalConstraintSense.GreaterThanOrEqual,0.0);
                AddConstraint(context,$"smallBucketSetupStartUpperState_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder().Add(start).Subtract(setup).Build(),
                    MathematicalConstraintSense.LessThanOrEqual,0.0);
                AddConstraint(context,$"smallBucketSetupStartUpperPrevious_r{routing.Id}_t{period}",
                    new LinearExpressionBuilder().Add(start).Add(previous).Build(),
                    MathematicalConstraintSense.LessThanOrEqual,1.0);
            }
        }
        return ValueTask.CompletedTask;
    }

    private static string SetupKey(int routingId,int period) =>
        new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.Setup)
            .Add(MathematicalDomainKeySegment.Routing,routingId)
            .Add(MathematicalDomainKeySegment.Period,period)
            .Build();
}
