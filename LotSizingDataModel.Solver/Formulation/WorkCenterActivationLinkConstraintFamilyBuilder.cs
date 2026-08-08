using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Links work-center production load to binary work-center activation.
/// </summary>
public sealed class WorkCenterActivationLinkConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>Gets the family identifier.</summary>
    public override string ConstraintFamilyId => "workCenterActivationLink";

    /// <summary>Determines whether the family is enabled.</summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeResourceActivation &&
            StandardFormulationResourceEnumerator
                .EnumerateWorkCenters(instance.SupplyChain)
                .Any(entry =>
                    entry.WorkCenter.FixedUsageCost is not null &&
                    entry.WorkCenter.CapacityConstraint is not null);
    }

    /// <summary>Builds work-center activation-link constraints.</summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var queries = new SupplyChainQueries(instance.SupplyChain);

        foreach (var entry in StandardFormulationResourceEnumerator
                     .EnumerateWorkCenters(instance.SupplyChain))
        {
            if (entry.WorkCenter.FixedUsageCost is null ||
                entry.WorkCenter.CapacityConstraint is null)
            {
                continue;
            }

            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression = new LinearExpressionBuilder();

                foreach (ProductionRouting routing
                         in instance.SupplyChain.ProductionRoutings)
                {
                    var reference =
                        routing.WorkCenters.FirstOrDefault(candidate =>
                            candidate.PlantId == entry.PlantId &&
                            candidate.WorkCenterId == entry.WorkCenter.Id);

                    if (reference is null)
                    {
                        continue;
                    }

                    ProductionCharacteristic characteristic =
                        queries.GetRequiredProductionCharacteristic(
                            routing.ItemId,
                            reference);

                    expression.Add(
                        context.GetVariable(
                            StandardFormulationVariableKeyFactory
                                .CreateProductionKey(routing.Id, period)),
                        characteristic.UnitCapacityConsumption?[period] ?? 1.0);
                }

                string activationKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.WorkCenterActivation)
                        .Add(MathematicalDomainKeySegment.Plant, entry.PlantId)
                        .Add(
                            MathematicalDomainKeySegment.WorkCenter,
                            entry.WorkCenter.Id)
                        .Add(MathematicalDomainKeySegment.Period, period)
                        .Build();

                expression.Subtract(
                    context.GetVariable(activationKey),
                    entry.WorkCenter.CapacityConstraint[period]);

                AddConstraint(
                    context,
                    $"workCenterActivationLink_p{entry.PlantId}" +
                    $"_w{entry.WorkCenter.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        "Work-center production load requires activation.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
