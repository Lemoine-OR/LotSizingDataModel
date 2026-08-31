using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketGroupingConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "smallBucketSetupGrouping";
    public override bool IsEnabled(LotSizingInstance instance,StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);ArgumentNullException.ThrowIfNull(options);
        return instance.SupplyChain.ProductionRoutings.Any(r=>r.GroupingConstraint is not null);
    }
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        ProductionSchedulingProfile profile=instance.SupplyChain.WorkCenters.Single(w=>w.SchedulingProfile is not null).SchedulingProfile!;
        foreach(ProductionRouting routing in instance.SupplyChain.ProductionRoutings)
        {
            if(routing.GroupingConstraint is null) continue;
            for(int setupPeriod=1;setupPeriod<=instance.PlanningHorizon;setupPeriod++)
            {
                int spacing=routing.GroupingConstraint.GetGroupingPeriodCount(setupPeriod);
                int last=Math.Min(instance.PlanningHorizon,setupPeriod+spacing-1);
                for(int forbidden=setupPeriod+1;forbidden<=last;forbidden++)
                {
                    AddConstraint(context,$"smallBucketGrouping_r{routing.Id}_t{setupPeriod}_k{forbidden}",
                        new LinearExpressionBuilder()
                            .Add(GetVariable(context,SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(profile,routing,setupPeriod)))
                            .Add(GetVariable(context,SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(profile,routing,forbidden))).Build(),
                        MathematicalConstraintSense.LessThanOrEqual,1.0);
                }
            }
        }
        return ValueTask.CompletedTask;
    }
}
