using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global work-center capacity constraints.
/// </summary>
public sealed class WorkCenterCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "workCenterCapacity";

    /// <summary>
    /// Determines whether at least one work center is
    /// capacitated.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return StandardFormulationResourceEnumerator
            .EnumerateWorkCenters(instance.SupplyChain)
            .Any(
                entry =>
                    entry.WorkCenter.CapacityConstraint is not null);
    }

    /// <summary>
    /// Builds global work-center capacity constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var queries =
            new SupplyChainQueries(
                instance.SupplyChain);

        foreach (var entry in StandardFormulationResourceEnumerator
                     .EnumerateWorkCenters(instance.SupplyChain))
        {
            WorkCenter workCenter =
                entry.WorkCenter;

            if (workCenter.CapacityConstraint is null)
            {
                continue;
            }

            var reference =
                new WorkCenterReference
                {
                    PlantId =
                        entry.PlantId,

                    WorkCenterId =
                        workCenter.Id
                };

            foreach (int period in Enumerable.Range(
                         1,
                         instance.PlanningHorizon))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression =
                    new LinearExpressionBuilder();

                foreach (ProductionRouting routing
                         in instance.SupplyChain.ProductionRoutings)
                {
                    if (!routing.WorkCenters.Any(
                            candidate =>
                                candidate.PlantId ==
                                    reference.PlantId &&
                                candidate.WorkCenterId ==
                                    reference.WorkCenterId))
                    {
                        continue;
                    }

                    ProductionCharacteristic characteristic =
                        queries.GetRequiredProductionCharacteristic(
                            routing.ItemId,
                            reference);

                    string productionKey =
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.Production)
                            .Add(
                                MathematicalDomainKeySegment.Routing,
                                routing.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build();

                    expression.Add(
                        context.GetVariable(productionKey),
                        characteristic.UnitCapacityConsumption?[period] ??
                            1.0);

                    double setupTime =
                        characteristic.SetupTime?[period] ??
                        0.0;

                    if (setupTime > 0.0)
                    {
                        string setupKey =
                            new MathematicalDomainKeyBuilder(
                                MathematicalDecisionCategory.Setup)
                                .Add(
                                    MathematicalDomainKeySegment.Routing,
                                    routing.Id)
                                .Add(
                                    MathematicalDomainKeySegment.Period,
                                    period)
                                .Build();

                        if (context.VariableRegistry.TryGet(
                                setupKey,
                                out MathematicalVariable? setupVariable) &&
                            setupVariable is not null)
                        {
                            expression.Add(
                                setupVariable,
                                setupTime);
                        }
                    }
                }

                foreach (ProductionSetupFamily family
                         in instance.SupplyChain.ProductionSetupFamilies)
                {
                    if (family.WorkCenter.PlantId != entry.PlantId ||
                        family.WorkCenter.WorkCenterId != workCenter.Id)
                    {
                        continue;
                    }

                    double familySetupTime =
                        family.SetupTime?[period] ?? 0.0;

                    if (familySetupTime <= 0.0)
                    {
                        continue;
                    }

                    string familySetupKey =
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory
                                .AuxiliaryProductionFamilySetup)
                            .Add(
                                MathematicalDomainKeySegment.SetupFamily,
                                family.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build();

                    expression.Add(
                        context.GetVariable(familySetupKey),
                        familySetupTime);
                }

                string additionalKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WorkCenterAdditionalCapacity)
                        .Add(
                            MathematicalDomainKeySegment.Plant,
                            entry.PlantId)
                        .Add(
                            MathematicalDomainKeySegment.WorkCenter,
                            workCenter.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                if (context.VariableRegistry.TryGet(
                        additionalKey,
                        out MathematicalVariable? additionalVariable) &&
                    additionalVariable is not null)
                {
                    expression.Subtract(
                        additionalVariable);
                }

                AddConstraint(
                    context,
                    $"workCenterCapacity_p{entry.PlantId}" +
                    $"_w{workCenter.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    workCenter.CapacityConstraint[period],
                    description:
                        "Global work-center capacity constraint.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
