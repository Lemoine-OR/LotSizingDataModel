namespace LotSizingDataModel.Solver.Formulation;

public static class GlspSchedulingFormulationFactory
{
    public static GlspSchedulingFormulation CreateDefault()
    {
        var options =
            new StandardLotSizingFormulationOptions
            {
                IncludeProductionSetups=false
            };

        var variables =
            StandardLotSizingFormulationFactory.CreateVariableFamilyBuilders()
                .Where(
                    builder =>
                        builder is not SetupVariableFamilyBuilder &&
                        builder is not LotSizeMultipleVariableFamilyBuilder &&
                        builder is not ProductionStartUpVariableFamilyBuilder)
                .Concat(
                    new IStandardLotSizingVariableFamilyBuilder[]
                    {
                        new GlspMicroProductionVariableFamilyBuilder(),
                        new GlspMicroSetupStateVariableFamilyBuilder(),
                        new GlspSetupStartVariableFamilyBuilder(),
                        new GlspStartUpVariableFamilyBuilder(),
                        new GlspChangeoverVariableFamilyBuilder(),
                        new GlspMacroProductionActivationVariableFamilyBuilder()
                    })
                .ToArray();

        var objective =
            StandardLotSizingFormulationFactory.CreateObjectiveTermBuilders()
                .Where(
                    builder =>
                        builder is not ProductionSetupCostObjectiveTermBuilder &&
                        builder is not ProductionStartUpCostObjectiveTermBuilder)
                .Concat(
                    new IStandardLotSizingObjectiveTermBuilder[]
                    {
                        new GlspSetupStartCostObjectiveTermBuilder(),
                        new GlspStartUpCostObjectiveTermBuilder(),
                        new GlspChangeoverCostObjectiveTermBuilder()
                    })
                .ToArray();

        var constraints =
            StandardLotSizingFormulationFactory.CreateConstraintFamilyBuilders()
                .Where(
                    builder =>
                        builder is not ProductionSetupLinkConstraintFamilyBuilder &&
                        builder is not ProductionStartUpDefinitionConstraintFamilyBuilder &&
                        builder is not MinimumLotSizeConstraintFamilyBuilder &&
                        builder is not MaximumLotSizeConstraintFamilyBuilder &&
                        builder is not LotSizeMultipleConstraintFamilyBuilder &&
                        builder is not GroupingConstraintFamilyBuilder &&
                        builder is not WorkCenterCapacityConstraintFamilyBuilder)
                .Concat(
                    new IStandardLotSizingConstraintFamilyBuilder[]
                    {
                        new GlspAggregateProductionConstraintFamilyBuilder(),
                        new GlspSingleSetupStateConstraintFamilyBuilder(),
                        new GlspSetupStartDefinitionConstraintFamilyBuilder(),
                        new GlspStartUpDefinitionConstraintFamilyBuilder(),
                        new GlspMicroProductionLinkConstraintFamilyBuilder(),
                        new GlspChangeoverDefinitionConstraintFamilyBuilder(),
                        new GlspSetupCountConstraintFamilyBuilder(),
                        new GlspGroupingConstraintFamilyBuilder(),
                        new GlspProducedItemCountConstraintFamilyBuilder(),
                        new GlspMacroCapacityConstraintFamilyBuilder()
                    })
                .ToArray();

        return new GlspSchedulingFormulation(
            new StandardLotSizingVariableBuilder(variables),
            new StandardLotSizingObjectiveBuilder(objective),
            new StandardLotSizingConstraintBuilder(constraints),
            options,
            new GlspSchedulingApplicabilityService());
    }
}
