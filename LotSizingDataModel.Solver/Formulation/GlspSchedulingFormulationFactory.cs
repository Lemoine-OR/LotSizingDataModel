namespace LotSizingDataModel.Solver.Formulation;
public static class GlspSchedulingFormulationFactory
{
    public static GlspSchedulingFormulation CreateDefault()
    {
        var options=new StandardLotSizingFormulationOptions{IncludeProductionSetups=false};
        var variables=StandardLotSizingFormulationFactory.CreateVariableFamilyBuilders().Where(b=>b is not SetupVariableFamilyBuilder&&b is not LotSizeMultipleVariableFamilyBuilder).Concat(new IStandardLotSizingVariableFamilyBuilder[]{new GlspMicroProductionVariableFamilyBuilder(),new GlspMicroSetupStateVariableFamilyBuilder(),new GlspSetupStartVariableFamilyBuilder(),new GlspChangeoverVariableFamilyBuilder(),new GlspMacroProductionActivationVariableFamilyBuilder()}).ToArray();
        var objective=StandardLotSizingFormulationFactory.CreateObjectiveTermBuilders().Where(b=>b is not ProductionSetupCostObjectiveTermBuilder).Concat(new IStandardLotSizingObjectiveTermBuilder[]{new GlspSetupStartCostObjectiveTermBuilder(),new GlspChangeoverCostObjectiveTermBuilder()}).ToArray();
        var constraints=StandardLotSizingFormulationFactory.CreateConstraintFamilyBuilders().Where(b=>b is not ProductionSetupLinkConstraintFamilyBuilder&&b is not MinimumLotSizeConstraintFamilyBuilder&&b is not MaximumLotSizeConstraintFamilyBuilder&&b is not LotSizeMultipleConstraintFamilyBuilder&&b is not GroupingConstraintFamilyBuilder&&b is not WorkCenterCapacityConstraintFamilyBuilder).Concat(new IStandardLotSizingConstraintFamilyBuilder[]{new GlspAggregateProductionConstraintFamilyBuilder(),new GlspSingleSetupStateConstraintFamilyBuilder(),new GlspSetupStartDefinitionConstraintFamilyBuilder(),new GlspMicroProductionLinkConstraintFamilyBuilder(),new GlspChangeoverDefinitionConstraintFamilyBuilder(),new GlspSetupCountConstraintFamilyBuilder(),new GlspGroupingConstraintFamilyBuilder(),new GlspProducedItemCountConstraintFamilyBuilder(),new GlspMacroCapacityConstraintFamilyBuilder()}).ToArray();
        return new GlspSchedulingFormulation(new StandardLotSizingVariableBuilder(variables),new StandardLotSizingObjectiveBuilder(objective),new StandardLotSizingConstraintBuilder(constraints),options,new GlspSchedulingApplicabilityService());
    }
}
