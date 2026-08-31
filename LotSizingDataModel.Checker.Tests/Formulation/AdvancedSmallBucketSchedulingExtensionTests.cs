using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Checker.Tests.Formulation;
public sealed class AdvancedSmallBucketSchedulingExtensionTests
{
    [Fact]
    public async Task Cslp_AdvancedSetupExtensions_BuildTogether()
    {
        LotSizingInstance instance=CreateCslp(SetupCarryOverPolicy.Allowed);
        MathematicalModel model=await SmallBucketSchedulingFormulationFactory.CreateCslp().BuildAsync(instance);
        Assert.Contains(model.Constraints,c=>c.Name=="smallBucketSetupStartInitialSame_r1");
        Assert.Contains(model.Constraints,c=>c.Name=="smallBucketMaximumSetupCount_t1");
        Assert.Contains(model.Constraints,c=>c.Name.StartsWith("smallBucketGrouping_r1",StringComparison.Ordinal));
        Assert.Contains(model.Constraints,c=>c.Name=="smallBucketCapacity_p1_w1_t1");
        Assert.Contains(model.Variables,v=>v.DomainKey.StartsWith(MathematicalDecisionCategory.WorkCenterAdditionalCapacity+"|",StringComparison.Ordinal));
    }
    [Fact]
    public async Task Cslp_ForbiddenCarryOver_ResetsLaterBucket()
    {
        LotSizingInstance instance=CreateCslp(SetupCarryOverPolicy.Forbidden);
        MathematicalModel model=await SmallBucketSchedulingFormulationFactory.CreateCslp().BuildAsync(instance);
        Assert.Contains(model.Constraints,c=>c.Name=="smallBucketSetupStartReset_r1_t2");
    }
    [Fact]
    public void Dlsp_SetupTimeAndAdditionalCapacity_AreNotSilentlyApproximated()
    {
        LotSizingInstance instance=CreateCslp(SetupCarryOverPolicy.Allowed);
        instance.SupplyChain.WorkCenters.Single().SchedulingProfile!.SmallBucketProductionMode=SmallBucketProductionMode.AllOrNothing;
        Assert.False(SmallBucketSchedulingFormulationFactory.CreateDlsp().CanBuild(instance));
    }
    private static LotSizingInstance CreateCslp(SetupCarryOverPolicy policy)
    {
        const int h=3;var chain=new SupplyChain(h);chain.Items.Add(new Item(1,"I1",0));chain.Items.Add(new Item(2,"I2",0));
        var profile=new ProductionSchedulingProfile{BucketMode=SchedulingBucketMode.SmallBucket,SmallBucketProductionMode=SmallBucketProductionMode.Continuous,SetupCarryOverPolicy=policy,InitialSetupItemId=1,MaximumProducedItemCount=new MaximumProducedItemCount(h,1),MaximumSetupCount=new MaximumSetupCount(h,1)};
        var wc=new WorkCenter(1,"M1"){CapacityConstraint=new CapacityConstraint(h,10.0),AdditionalCapacity=new AdditionalCapacity(h,3.0),AdditionalCapacityCost=new AdditionalCapacityCost(h,2.0),SchedulingProfile=profile};
        var plant=new Plant(1,"P1",new PlantWarehouse("P1-Warehouse"));plant.WorkCenters.Add(wc);chain.Plants.Add(plant);Add(chain,1,1,h,2);Add(chain,2,2,h,1);return new LotSizingInstance(chain,"advanced-cslp");
    }
    private static void Add(SupplyChain chain,int routingId,int itemId,int h,int grouping)
    {
        var r=new ProductionRouting(routingId,itemId,1,0){GroupingConstraint=new GroupingConstraint(h,grouping)};r.AddWorkCenter(1);chain.ProductionRoutings.Add(r);
        chain.ProductionCharacteristics.Add(new ProductionCharacteristic(itemId,1,1){UnitCapacityConsumption=new UnitCapacityConsumption(h,1.0),SetupTime=new SetupTime(h,1.0),FixedSetupCost=new FixedSetupCost(h,4.0)});
    }
}
