namespace LotSizingDataModel.Solver.Formulation;

public static class SmallBucketSchedulingFormulationFactory
{
    public static SmallBucketSchedulingFormulation CreateDlsp()=>Create(SmallBucketSchedulingFormulationKind.Dlsp);
    public static SmallBucketSchedulingFormulation CreateCslp()=>Create(SmallBucketSchedulingFormulationKind.Cslp);
    public static SmallBucketSchedulingFormulation CreatePlsp()=>Create(SmallBucketSchedulingFormulationKind.Plsp);

    public static SmallBucketSchedulingFormulation Create(
        SmallBucketSchedulingFormulationKind kind)
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
                        new SmallBucketSetupStateVariableFamilyBuilder(),
                        new SmallBucketProductionActivationVariableFamilyBuilder(),
                        new SmallBucketSetupStartVariableFamilyBuilder(),
                        new SmallBucketStartUpVariableFamilyBuilder()
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
                        new SmallBucketSetupStartCostObjectiveTermBuilder(),
                        new SmallBucketStartUpCostObjectiveTermBuilder()
                    })
                .ToArray();

        var scheduling =
            new List<IStandardLotSizingConstraintFamilyBuilder>
            {
                kind==SmallBucketSchedulingFormulationKind.Plsp
                    ? new PlspSingleSetupStateConstraintFamilyBuilder()
                    : new SmallBucketSingleSetupStateConstraintFamilyBuilder(),
                new SmallBucketSetupStartDefinitionConstraintFamilyBuilder(),
                new SmallBucketStartUpDefinitionConstraintFamilyBuilder(),
                new SmallBucketProductionStateConstraintFamilyBuilder(kind),
                new SmallBucketProducedItemCountConstraintFamilyBuilder(),
                new SmallBucketSetupCountConstraintFamilyBuilder(),
                new SmallBucketGroupingConstraintFamilyBuilder()
            };

        if(kind==SmallBucketSchedulingFormulationKind.Plsp)
        {
            scheduling.Add(
                new PlspSetupTransitionLimitConstraintFamilyBuilder());
        }

        if(kind!=SmallBucketSchedulingFormulationKind.Dlsp)
        {
            scheduling.Add(
                new SmallBucketSchedulingCapacityConstraintFamilyBuilder());
        }

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
                        (kind==SmallBucketSchedulingFormulationKind.Dlsp ||
                         builder is not WorkCenterCapacityConstraintFamilyBuilder))
                .Concat(scheduling)
                .ToArray();

        return new SmallBucketSchedulingFormulation(
            kind,
            new StandardLotSizingVariableBuilder(variables),
            new StandardLotSizingObjectiveBuilder(objective),
            new StandardLotSizingConstraintBuilder(constraints),
            options,
            new SmallBucketSchedulingApplicabilityService());
    }
}
