using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global transport-resource capacity constraints.
/// </summary>
public sealed class TransportResourceCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "transportResourceCapacity";

    /// <summary>
    /// Determines whether at least one transport resource is
    /// globally capacitated.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            instance.SupplyChain.TransportResources.Any(
                resource =>
                    resource.CapacityConstraint is not null);
    }

    /// <summary>
    /// Builds global transport-resource capacity constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (TransportResource resource
                 in instance.SupplyChain.TransportResources)
        {
            if (resource.CapacityConstraint is null)
            {
                continue;
            }

            TransportCharacteristic[] characteristics =
                instance.SupplyChain.TransportCharacteristics
                    .Where(
                        characteristic =>
                            characteristic.TransportResourceId ==
                                resource.Id)
                    .ToArray();

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression =
                    new LinearExpressionBuilder();

                foreach (TransportCharacteristic characteristic
                         in characteristics)
                {
                    foreach (TransportLane lane in resource.Lanes)
                    {
                        string transportKey =
                            CreateKey(
                                MathematicalDecisionCategory.Transport,
                                characteristic.ItemId,
                                resource.Id,
                                lane,
                                period);

                        expression.Add(
                            context.GetVariable(transportKey),
                            characteristic.UnitCapacityConsumption?[period] ??
                                1.0);

                        double setupTime =
                            characteristic.SetupTime?[period] ??
                            0.0;

                        if (setupTime > 0.0)
                        {
                            string setupKey =
                                CreateKey(
                                    MathematicalDecisionCategory
                                        .TransportSetup,
                                    characteristic.ItemId,
                                    resource.Id,
                                    lane,
                                    period);

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
                }

                string additionalKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .TransportResourceAdditionalCapacity)
                        .Add(
                            MathematicalDomainKeySegment.TransportResource,
                            resource.Id)
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
                    $"transportResourceCapacity_r{resource.Id}" +
                    $"_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    resource.CapacityConstraint[period],
                    description:
                        "Global transport-resource capacity " +
                        "constraint.");
            }
        }

        return ValueTask.CompletedTask;
    }

    private static string CreateKey(
        string category,
        int itemId,
        int resourceId,
        TransportLane lane,
        int period)
    {
        var keyBuilder =
            new MathematicalDomainKeyBuilder(category)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId)
                .Add(
                    MathematicalDomainKeySegment.TransportResource,
                    resourceId);

        StandardFormulationDomainKeyFactory.AddOriginWarehouse(
            keyBuilder,
            lane.Origin);

        StandardFormulationDomainKeyFactory.AddDestinationWarehouse(
            keyBuilder,
            lane.Destination);

        return keyBuilder
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }
}
