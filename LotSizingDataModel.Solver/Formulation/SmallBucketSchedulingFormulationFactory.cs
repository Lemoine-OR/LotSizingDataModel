namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates executable DLSP and CSLP formulations by composing the standard
/// lot-sizing builders with scheduling-specific state/transition families.
/// </summary>
public static class SmallBucketSchedulingFormulationFactory
{
    public static SmallBucketSchedulingFormulation CreateDlsp() =>
        Create(
            SmallBucketSchedulingFormulationKind.Dlsp);

    public static SmallBucketSchedulingFormulation CreateCslp() =>
        Create(
            SmallBucketSchedulingFormulationKind.Cslp);

    public static SmallBucketSchedulingFormulation Create(
        SmallBucketSchedulingFormulationKind kind)
    {
        var options =
            new StandardLotSizingFormulationOptions
            {
                IncludeProductionSetups =
                    false
            };

        var variableFamilies =
            StandardLotSizingFormulationFactory
                .CreateVariableFamilyBuilders()
                .Where(
                    builder =>
                        builder is not SetupVariableFamilyBuilder &&
                        builder is not LotSizeMultipleVariableFamilyBuilder)
                .Concat(
                    kind ==
                        SmallBucketSchedulingFormulationKind.Dlsp
                        ? new IStandardLotSizingVariableFamilyBuilder[]
                        {
                            new SmallBucketSetupStateVariableFamilyBuilder(),
                            new SmallBucketProductionActivationVariableFamilyBuilder(),
                            new SmallBucketSetupStartVariableFamilyBuilder()
                        }
                        : new IStandardLotSizingVariableFamilyBuilder[]
                        {
                            new SmallBucketSetupStateVariableFamilyBuilder(),
                            new SmallBucketSetupStartVariableFamilyBuilder()
                        })
                .ToArray();

        var objectiveTerms =
            StandardLotSizingFormulationFactory
                .CreateObjectiveTermBuilders()
                .Where(
                    builder =>
                        builder is not
                            ProductionSetupCostObjectiveTermBuilder)
                .Append(
                    new SmallBucketSetupStartCostObjectiveTermBuilder())
                .ToArray();

        var constraintFamilies =
            StandardLotSizingFormulationFactory
                .CreateConstraintFamilyBuilders()
                .Where(
                    builder =>
                        builder is not
                            ProductionSetupLinkConstraintFamilyBuilder &&
                        builder is not
                            MinimumLotSizeConstraintFamilyBuilder &&
                        builder is not
                            MaximumLotSizeConstraintFamilyBuilder &&
                        builder is not
                            LotSizeMultipleConstraintFamilyBuilder &&
                        builder is not
                            GroupingConstraintFamilyBuilder)
                .Concat(
                    new IStandardLotSizingConstraintFamilyBuilder[]
                    {
                        new SmallBucketSingleSetupStateConstraintFamilyBuilder(),
                        new SmallBucketSetupStartDefinitionConstraintFamilyBuilder(),
                        new SmallBucketProductionStateConstraintFamilyBuilder(
                            kind)
                    })
                .ToArray();

        return new SmallBucketSchedulingFormulation(
            kind,
            new StandardLotSizingVariableBuilder(
                variableFamilies),
            new StandardLotSizingObjectiveBuilder(
                objectiveTerms),
            new StandardLotSizingConstraintBuilder(
                constraintFamilies),
            options,
            new SmallBucketSchedulingApplicabilityService());
    }
}
