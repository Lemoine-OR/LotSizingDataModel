namespace LotSizingDataModel.Solver.Formulation;

public static class GlspSchedulingFormulationFactory
{
    public static GlspSchedulingFormulation CreateDefault()
    {
        var options = new StandardLotSizingFormulationOptions
        {
            IncludeProductionSetups = false
        };

        var variableFamilies =
            StandardLotSizingFormulationFactory.CreateVariableFamilyBuilders()
                .Where(builder =>
                    builder is not SetupVariableFamilyBuilder &&
                    builder is not LotSizeMultipleVariableFamilyBuilder)
                .Concat(new IStandardLotSizingVariableFamilyBuilder[]
                {
                    new GlspMicroProductionVariableFamilyBuilder(),
                    new GlspMicroSetupStateVariableFamilyBuilder(),
                    new GlspChangeoverVariableFamilyBuilder()
                })
                .ToArray();

        var objectiveTerms =
            StandardLotSizingFormulationFactory.CreateObjectiveTermBuilders()
                .Where(builder => builder is not ProductionSetupCostObjectiveTermBuilder)
                .Append(new GlspChangeoverCostObjectiveTermBuilder())
                .ToArray();

        var constraintFamilies =
            StandardLotSizingFormulationFactory.CreateConstraintFamilyBuilders()
                .Where(builder =>
                    builder is not ProductionSetupLinkConstraintFamilyBuilder &&
                    builder is not MinimumLotSizeConstraintFamilyBuilder &&
                    builder is not MaximumLotSizeConstraintFamilyBuilder &&
                    builder is not LotSizeMultipleConstraintFamilyBuilder &&
                    builder is not GroupingConstraintFamilyBuilder &&
                    builder is not WorkCenterCapacityConstraintFamilyBuilder)
                .Concat(new IStandardLotSizingConstraintFamilyBuilder[]
                {
                    new GlspAggregateProductionConstraintFamilyBuilder(),
                    new GlspSingleSetupStateConstraintFamilyBuilder(),
                    new GlspMicroProductionLinkConstraintFamilyBuilder(),
                    new GlspChangeoverDefinitionConstraintFamilyBuilder(),
                    new GlspMacroCapacityConstraintFamilyBuilder()
                })
                .ToArray();

        return new GlspSchedulingFormulation(
            new StandardLotSizingVariableBuilder(variableFamilies),
            new StandardLotSizingObjectiveBuilder(objectiveTerms),
            new StandardLotSizingConstraintBuilder(constraintFamilies),
            options,
            new GlspSchedulingApplicabilityService());
    }
}
