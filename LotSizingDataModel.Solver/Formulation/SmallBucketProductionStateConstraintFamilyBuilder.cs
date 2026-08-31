using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SmallBucketProductionStateConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    private readonly SmallBucketSchedulingFormulationKind _kind;
    public SmallBucketProductionStateConstraintFamilyBuilder(SmallBucketSchedulingFormulationKind kind){_kind=kind;}
    public override string ConstraintFamilyId => _kind switch
    {
        SmallBucketSchedulingFormulationKind.Dlsp=>"dlspProductionState",
        SmallBucketSchedulingFormulationKind.Cslp=>"cslpProductionState",
        SmallBucketSchedulingFormulationKind.Plsp=>"plspProductionState",
        _=>throw new InvalidOperationException("Unknown small-bucket formulation kind.")
    };
    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance,MathematicalModelBuildContext context,StandardLotSizingFormulationOptions options,CancellationToken cancellationToken)
    {
        var wc=instance.SupplyChain.WorkCenters.Single(w=>w.SchedulingProfile is not null);
        var profile=wc.SchedulingProfile!;
        foreach(ProductionRouting routing in instance.SupplyChain.ProductionRoutings)
        {
            var reference=routing.WorkCenters.Single();
            ProductionCharacteristic characteristic=instance.SupplyChain.ProductionCharacteristics.Single(candidate=>candidate.ItemId==routing.ItemId && candidate.WorkCenter.PlantId==reference.PlantId && candidate.WorkCenter.WorkCenterId==reference.WorkCenterId);
            for(int period=1;period<=instance.PlanningHorizon;period++)
            {
                double regular=wc.CapacityConstraint![period];
                double maxAdditional=wc.AdditionalCapacity?[period]??0.0;
                double consumption=characteristic.UnitCapacityConsumption![period];
                MathematicalVariable production=GetVariable(context,ProductionKey(routing.Id,period));
                MathematicalVariable setup=GetVariable(context,SetupKey(routing.Id,period));
                MathematicalVariable active=GetVariable(context,ActivationKey(routing.Id,period));
                if(_kind==SmallBucketSchedulingFormulationKind.Dlsp)
                {
                    AddConstraint(context,$"dlspProductionFullBucket_r{routing.Id}_t{period}",new LinearExpressionBuilder().Add(production,consumption).Subtract(active,regular).Build(),MathematicalConstraintSense.Equal,0.0);
                    AddConstraint(context,$"dlspProductionRequiresSetup_r{routing.Id}_t{period}",new LinearExpressionBuilder().Add(active).Subtract(setup).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);
                    continue;
                }
                AddConstraint(context,$"{Prefix()}ProductionActivation_r{routing.Id}_t{period}",new LinearExpressionBuilder().Add(production,consumption).Subtract(active,regular+maxAdditional).Build(),MathematicalConstraintSense.LessThanOrEqual,0.0);
                var state=new LinearExpressionBuilder().Add(active).Subtract(setup); double rhs=0.0;
                if(_kind==SmallBucketSchedulingFormulationKind.Plsp)
                {
                    if(period>1) state.Subtract(GetVariable(context,SetupKey(routing.Id,period-1)));
                    else if(profile.HasInitialSetupState && profile.InitialSetupItemId==routing.ItemId) rhs=1.0;
                }
                AddConstraint(context,$"{Prefix()}ProductionState_r{routing.Id}_t{period}",state.Build(),MathematicalConstraintSense.LessThanOrEqual,rhs);
            }
        }
        return ValueTask.CompletedTask;
    }
    private string Prefix()=>_kind==SmallBucketSchedulingFormulationKind.Plsp?"plsp":"cslp";
    private static string ProductionKey(int r,int t)=>new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.Production).Add(MathematicalDomainKeySegment.Routing,r).Add(MathematicalDomainKeySegment.Period,t).Build();
    private static string SetupKey(int r,int t)=>new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.Setup).Add(MathematicalDomainKeySegment.Routing,r).Add(MathematicalDomainKeySegment.Period,t).Build();
    private static string ActivationKey(int r,int t)=>new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliarySmallBucketProductionActivation).Add(MathematicalDomainKeySegment.Routing,r).Add(MathematicalDomainKeySegment.Period,t).Build();
}
