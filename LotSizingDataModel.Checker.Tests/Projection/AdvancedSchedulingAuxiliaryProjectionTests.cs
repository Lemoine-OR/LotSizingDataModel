using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;
namespace LotSizingDataModel.Checker.Tests.Projection;
public sealed class AdvancedSchedulingAuxiliaryProjectionTests
{
    [Fact]
    public void Projector_ReconstructsSmallBucketInitialAndResetSetupStarts()
    {
        var solution=new LotSizingSolution(2);var p=new ProductionDecision(1,2);p.SetSetupActivated(1,true);p.SetSetupActivated(2,true);solution.ProductionDecisions.Add(p);
        var model=new MathematicalModel{Name="advanced-small-projection"};model.Variables.Add(Bin(1,new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliarySchedulingSetupStart).Add(MathematicalDomainKeySegment.Routing,1).Add(MathematicalDomainKeySegment.Item,1).Add(MathematicalDomainKeySegment.Period,1).Add(MathematicalDomainKeySegment.FromItem,1).Build()));model.Variables.Add(Bin(2,new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliarySchedulingSetupStart).Add(MathematicalDomainKeySegment.Routing,1).Add(MathematicalDomainKeySegment.Item,1).Add(MathematicalDomainKeySegment.Period,2).Add(MathematicalDomainKeySegment.SetupReset,1).Build()));
        var result=new MathematicalSolutionValueProjector().Project(model,solution);Assert.Empty(result.Issues);Assert.Equal(0.0,result.Values[1]);Assert.Equal(1.0,result.Values[2]);
    }
    [Fact]
    public void Projector_ReconstructsGlspInitialChangeoverSetupStartAndMacroActivation()
    {
        var solution=new LotSizingSolution(1);var production=new ProductionDecision(1,1);production.SetQuantity(1,3.0);solution.ProductionDecisions.Add(production);var schedule=new WorkCenterSchedulingDecision(new WorkCenterReference(1,1),1);schedule.AddMicroPeriodDecision(new ProductionMicroPeriodDecision(new ProductionMicroPeriodReference(1,1),2,2,2.0));solution.AddWorkCenterSchedulingDecision(schedule);
        var model=new MathematicalModel{Name="advanced-glsp-projection"};
        model.Variables.Add(Bin(1,new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliaryMicroPeriodSetupStart).Add(MathematicalDomainKeySegment.Plant,1).Add(MathematicalDomainKeySegment.WorkCenter,1).Add(MathematicalDomainKeySegment.Period,1).Add(MathematicalDomainKeySegment.MicroPeriod,1).Add(MathematicalDomainKeySegment.Routing,2).Add(MathematicalDomainKeySegment.Item,2).Add(MathematicalDomainKeySegment.FromItem,1).Build()));
        model.Variables.Add(Bin(2,new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover).Add(MathematicalDomainKeySegment.Plant,1).Add(MathematicalDomainKeySegment.WorkCenter,1).Add(MathematicalDomainKeySegment.Period,1).Add(MathematicalDomainKeySegment.MicroPeriod,1).Add(MathematicalDomainKeySegment.FromItem,1).Add(MathematicalDomainKeySegment.ToItem,2).Build()));
        model.Variables.Add(Bin(3,new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.AuxiliaryMacroProductionActivation).Add(MathematicalDomainKeySegment.Routing,1).Add(MathematicalDomainKeySegment.Period,1).Build()));
        var result=new MathematicalSolutionValueProjector().Project(model,solution);Assert.Empty(result.Issues);Assert.Equal(1.0,result.Values[1]);Assert.Equal(1.0,result.Values[2]);Assert.Equal(1.0,result.Values[3]);
    }
    private static MathematicalVariable Bin(int id,string key)=>new(){Id=id,Name=$"v{id}",DomainKey=key,VariableType=MathematicalVariableType.Binary,LowerBound=0.0,UpperBound=1.0};
}
