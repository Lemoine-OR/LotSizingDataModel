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
public sealed class AdvancedGlspSchedulingExtensionTests
{
    [Fact]
    public async Task Glsp_AdvancedExtensions_BuildTogether()
    {
        LotSizingInstance instance=CreateInstance();MathematicalModel model=await GlspSchedulingFormulationFactory.CreateDefault().BuildAsync(instance);
        Assert.Contains(model.Variables,v=>v.DomainKey.StartsWith(MathematicalDecisionCategory.AuxiliaryMicroPeriodSetupStart+"|",StringComparison.Ordinal));
        Assert.Contains(model.Variables,v=>v.DomainKey.StartsWith(MathematicalDecisionCategory.AuxiliaryMacroProductionActivation+"|",StringComparison.Ordinal));
        Assert.Contains(model.Constraints,c=>c.Name=="glspSetupStartInitialSame_r1_t1_s1");
        Assert.Contains(model.Constraints,c=>c.Name.StartsWith("glspSetupStartReset",StringComparison.Ordinal));
        Assert.Contains(model.Constraints,c=>c.Name=="glspMaximumSetupCount_t1");
        Assert.Contains(model.Constraints,c=>c.Name.StartsWith("glspGrouping_r1",StringComparison.Ordinal));
        Assert.Contains(model.Constraints,c=>c.Name=="glspProducedItemCount_t1");
        Assert.Contains(model.Variables,v=>v.DomainKey.StartsWith(MathematicalDecisionCategory.WorkCenterAdditionalCapacity+"|",StringComparison.Ordinal));
    }
    private static LotSizingInstance CreateInstance()
    {
        const int h=2;var chain=new SupplyChain(h);chain.Items.Add(new Item(1,"I1",0));chain.Items.Add(new Item(2,"I2",0));
        var profile=new ProductionSchedulingProfile{BucketMode=SchedulingBucketMode.MacroMicro,MicroPeriodLengthMode=MicroPeriodLengthMode.Variable,MicroPeriodAssignmentMode=MicroPeriodAssignmentMode.SingleItem,SetupCarryOverPolicy=SetupCarryOverPolicy.Forbidden,InitialSetupItemId=1,MicroPeriodCount=new MicroPeriodCount(h,2),MaximumSetupCount=new MaximumSetupCount(h,2),MaximumProducedItemCount=new MaximumProducedItemCount(h,2)};
        profile.Changeovers.Add(new ProductionChangeover{FromItemId=1,ToItemId=2,ChangeoverTime=new SequenceDependentChangeoverTime(h,0.5),ChangeoverCost=new SequenceDependentChangeoverCost(h,3.0)});profile.Changeovers.Add(new ProductionChangeover{FromItemId=2,ToItemId=1,ChangeoverTime=new SequenceDependentChangeoverTime(h,0.5),ChangeoverCost=new SequenceDependentChangeoverCost(h,3.0)});
        var wc=new WorkCenter(1,"M1"){CapacityConstraint=new CapacityConstraint(h,10.0),AdditionalCapacity=new AdditionalCapacity(h,4.0),AdditionalCapacityCost=new AdditionalCapacityCost(h,1.0),SchedulingProfile=profile};var plant=new Plant(1,"P1",new PlantWarehouse("P1-Warehouse"));plant.WorkCenters.Add(wc);chain.Plants.Add(plant);Add(chain,1,1,h,2);Add(chain,2,2,h,1);return new LotSizingInstance(chain,"advanced-glsp");
    }
    private static void Add(SupplyChain chain,int rid,int item,int h,int grouping){var r=new ProductionRouting(rid,item,1,0){GroupingConstraint=new GroupingConstraint(h,grouping)};r.AddWorkCenter(1);chain.ProductionRoutings.Add(r);chain.ProductionCharacteristics.Add(new ProductionCharacteristic(item,1,1){UnitCapacityConsumption=new UnitCapacityConsumption(h,1.0),SetupTime=new SetupTime(h,0.5),FixedSetupCost=new FixedSetupCost(h,2.0)});}
}
