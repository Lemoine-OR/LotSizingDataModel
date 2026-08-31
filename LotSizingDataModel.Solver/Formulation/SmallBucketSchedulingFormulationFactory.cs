namespace LotSizingDataModel.Solver.Formulation;

public static class SmallBucketSchedulingFormulationFactory
{
    public static SmallBucketSchedulingFormulation CreateDlsp()=>Create(SmallBucketSchedulingFormulationKind.Dlsp);
    public static SmallBucketSchedulingFormulation CreateCslp()=>Create(SmallBucketSchedulingFormulationKind.Cslp);
    public static SmallBucketSchedulingFormulation CreatePlsp()=>Create(SmallBucketSchedulingFormulationKind.Plsp);
    public static SmallBucketSchedulingFormulation Create(SmallBucketSchedulingFormulationKind kind)
    {
        var options=new StandardLotSizingFormulationOptions{IncludeProductionSetups=false};
        var variables=StandardLotSizingFormulationFactory.CreateVariableFamilyBuilders().Where(b=>b is not SetupVariableFamilyBuilder && b is not LotSizeMultipleVariableFamilyBuilder).Concat(new IStandardLotSizingVariableFamilyBuilder[]{new SmallBucketSetupStateVariableFamilyBuilder(),new SmallBucketProductionActivationVariableFamilyBuilder(),new SmallBucketSetupStartVariableFamilyBuilder()}).ToArray();
        var objective=StandardLotSizingFormulationFactory.CreateObjectiveTermBuilders().Where(b=>b is not ProductionSetupCostObjectiveTermBuilder).Append(new SmallBucketSetupStartCostObjectiveTermBuilder()).ToArray();
        var scheduling=new List<IStandardLotSizingConstraintFamilyBuilder>{kind==SmallBucketSchedulingFormulationKind.Plsp?new PlspSingleSetupStateConstraintFamilyBuilder():new SmallBucketSingleSetupStateConstraintFamilyBuilder(),new SmallBucketSetupStartDefinitionConstraintFamilyBuilder(),new SmallBucketProductionStateConstraintFamilyBuilder(kind),new SmallBucketProducedItemCountConstraintFamilyBuilder(),new SmallBucketSetupCountConstraintFamilyBuilder(),new SmallBucketGroupingConstraintFamilyBuilder()};
        if(kind==SmallBucketSchedulingFormulationKind.Plsp) scheduling.Add(new PlspSetupTransitionLimitConstraintFamilyBuilder());
        if(kind!=SmallBucketSchedulingFormulationKind.Dlsp) scheduling.Add(new SmallBucketSchedulingCapacityConstraintFamilyBuilder());
        var constraints=StandardLotSizingFormulationFactory.CreateConstraintFamilyBuilders().Where(b=>b is not ProductionSetupLinkConstraintFamilyBuilder && b is not MinimumLotSizeConstraintFamilyBuilder && b is not MaximumLotSizeConstraintFamilyBuilder && b is not LotSizeMultipleConstraintFamilyBuilder && b is not GroupingConstraintFamilyBuilder && (kind==SmallBucketSchedulingFormulationKind.Dlsp || b is not WorkCenterCapacityConstraintFamilyBuilder)).Concat(scheduling).ToArray();
        return new SmallBucketSchedulingFormulation(kind,new StandardLotSizingVariableBuilder(variables),new StandardLotSizingObjectiveBuilder(objective),new StandardLotSizingConstraintBuilder(constraints),options,new SmallBucketSchedulingApplicabilityService());
    }
}
