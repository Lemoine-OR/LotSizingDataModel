using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketSetupCountConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "smallBucketMaximumSetupCount";
    public override bool IsEnabled(LotSizingInstance instance,StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);ArgumentNullException.ThrowIfNull(options);
        return instance.SupplyChain.WorkCenters.Any(w=>w.SchedulingProfile?.MaximumSetupCount is not null);
    }
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        ProductionSchedulingProfile profile=instance.SupplyChain.WorkCenters.Single(w=>w.SchedulingProfile is not null).SchedulingProfile!;
        MaximumSetupCount limit=profile.MaximumSetupCount ?? throw new InvalidOperationException("MaximumSetupCount is required.");
        for(int period=1;period<=instance.PlanningHorizon;period++)
        {
            var expression=new LinearExpressionBuilder();
            foreach(var routing in instance.SupplyChain.ProductionRoutings)
                expression.Add(GetVariable(context,SmallBucketSchedulingDomainKeyFactory.CreateSetupStartKey(profile,routing,period)));
            AddConstraint(context,$"smallBucketMaximumSetupCount_t{period}",expression.Build(),MathematicalConstraintSense.LessThanOrEqual,limit.GetCount(period));
        }
        return ValueTask.CompletedTask;
    }
}
