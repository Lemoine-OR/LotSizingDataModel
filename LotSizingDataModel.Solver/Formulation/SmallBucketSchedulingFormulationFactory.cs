namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates executable DLSP, CSLP and PLSP formulations by composing generic
/// lot-sizing families with scheduling-specific state/transition families.
/// </summary>
public static class SmallBucketSchedulingFormulationFactory
{
    public static SmallBucketSchedulingFormulation CreateDlsp() =>
        Create(
            SmallBucketSchedulingFormulationKind.Dlsp);

    public static SmallBucketSchedulingFormulation CreateCslp() =>
        Create(
            SmallBucketSchedulingFormulationKind.Cslp);

    public static SmallBucketSchedulingFormulation CreatePlsp() =>
        Create(
            SmallBucketSchedulingFormulationKind.Plsp);

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
                    new IStandardLotSizingVariableFamilyBuilder[]
                    {
                        new SmallBucketSetupStateVariableFamilyBuilder(),
                        new SmallBucketProductionActivationVariableFamilyBuilder(),
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

        var schedulingConstraints =
            new List<IStandardLotSizingConstraintFamilyBuilder>();

        if (
            kind ==
            SmallBucketSchedulingFormulationKind.Plsp)
        {
            schedulingConstraints.Add(
                new PlspSingleSetupStateConstraintFamilyBuilder());
        }
        else
        {
            schedulingConstraints.Add(
                new SmallBucketSingleSetupStateConstraintFamilyBuilder());
        }

        schedulingConstraints.Add(
            new SmallBucketSetupStartDefinitionConstraintFamilyBuilder());

        schedulingConstraints.Add(
            new SmallBucketProductionStateConstraintFamilyBuilder(
                kind));

        schedulingConstraints.Add(
            new SmallBucketProducedItemCountConstraintFamilyBuilder());

        if (
            kind ==
            SmallBucketSchedulingFormulationKind.Plsp)
        {
            schedulingConstraints.Add(
                new PlspSetupTransitionLimitConstraintFamilyBuilder());
        }

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
                    schedulingConstraints)
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
