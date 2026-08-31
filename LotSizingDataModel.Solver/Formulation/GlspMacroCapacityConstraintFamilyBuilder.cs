using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds GLSP macro-period capacity including production, generic setup-start
/// time, distinct start-up time, and sequence-dependent changeover time.
/// </summary>
public sealed class GlspMacroCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "glspMacroCapacity";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var (plantId,wc,profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);

        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(
                instance,
                plantId,
                wc.Id);

        var ordered =
            profile.EnumerateMicroPeriods().ToArray();

        for(int period=1;
            period<=instance.PlanningHorizon;
            period++)
        {
            var expression =
                new LinearExpressionBuilder();

            for(int index=0;
                index<ordered.Length;
                index++)
            {
                var micro =
                    ordered[index];

                if(micro.MacroPeriod!=period)
                {
                    continue;
                }

                int fixedFrom =
                    GlspSequenceSemantics.GetFixedPredecessorItemId(
                        profile,
                        index);

                bool reset =
                    index>0 &&
                    GlspSequenceSemantics.IsResetBoundary(
                        profile,
                        ordered[index-1],
                        micro);

                foreach(ProductionRouting routing in routings)
                {
                    ProductionCharacteristic characteristic =
                        GlspSchedulingData.GetCharacteristic(
                            instance,
                            routing,
                            plantId,
                            wc.Id);

                    expression.Add(
                        GetVariable(
                            context,
                            GlspFormulationVariableKeyFactory.CreateMicroProductionKey(
                                plantId,
                                wc.Id,
                                routing.Id,
                                routing.ItemId,
                                micro)),
                        characteristic.UnitCapacityConsumption![period]);

                    double setupTime =
                        characteristic.SetupTime?[period] ?? 0.0;

                    if(setupTime>0.0)
                    {
                        expression.Add(
                            GetVariable(
                                context,
                                GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(
                                    plantId,
                                    wc.Id,
                                    routing.Id,
                                    routing.ItemId,
                                    micro,
                                    fixedFrom,
                                    reset)),
                            setupTime);
                    }

                    double startUpTime =
                        characteristic.StartUpTime?[period] ?? 0.0;

                    if(startUpTime>0.0)
                    {
                        expression.Add(
                            GetVariable(
                                context,
                                ProductionStartUpDomainKeyFactory.CreateGlspKey(
                                    plantId,
                                    wc.Id,
                                    routing.Id,
                                    routing.ItemId,
                                    micro,
                                    fixedFrom,
                                    reset)),
                            startUpTime);
                    }
                }

                foreach(ProductionRouting from in routings)
                foreach(ProductionRouting to in routings)
                {
                    if(from.ItemId==to.ItemId)
                    {
                        continue;
                    }

                    string key =
                        GlspFormulationVariableKeyFactory.CreateChangeoverKey(
                            plantId,
                            wc.Id,
                            from.ItemId,
                            to.ItemId,
                            micro);

                    if(!context.VariableRegistry.TryGet(
                            key,
                            out MathematicalVariable? changeover) ||
                        changeover is null)
                    {
                        continue;
                    }

                    double changeoverTime =
                        GlspSchedulingData.FindChangeover(
                            profile,
                            from.ItemId,
                            to.ItemId)?.ChangeoverTime?[period] ?? 0.0;

                    if(changeoverTime>0.0)
                    {
                        expression.Add(
                            changeover,
                            changeoverTime);
                    }
                }
            }

            string additionalKey =
                new MathematicalDomainKeyBuilder(
                    MathematicalDecisionCategory.WorkCenterAdditionalCapacity)
                    .Add(MathematicalDomainKeySegment.Plant,plantId)
                    .Add(MathematicalDomainKeySegment.WorkCenter,wc.Id)
                    .Add(MathematicalDomainKeySegment.Period,period)
                    .Build();

            if(context.VariableRegistry.TryGet(
                    additionalKey,
                    out MathematicalVariable? additional) &&
                additional is not null)
            {
                expression.Subtract(additional);
            }

            AddConstraint(
                context,
                $"glspMacroCapacity_p{plantId}_w{wc.Id}_t{period}",
                expression.Build(),
                MathematicalConstraintSense.LessThanOrEqual,
                wc.CapacityConstraint![period]);
        }

        return ValueTask.CompletedTask;
    }
}
