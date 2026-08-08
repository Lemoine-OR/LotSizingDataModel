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
/// Links aggregate transport load to binary transport-resource activation.
/// </summary>
public sealed class TransportResourceActivationLinkConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>Gets the family identifier.</summary>
    public override string ConstraintFamilyId =>
        "transportResourceActivationLink";

    /// <summary>Determines whether the family is enabled.</summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeTransport &&
               options.IncludeResourceActivation &&
               instance.SupplyChain.TransportResources.Any(resource =>
                   resource.FixedUsageCost is not null &&
                   resource.CapacityConstraint is not null);
    }

    /// <summary>Builds transport-resource activation links.</summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (TransportResource resource
                 in instance.SupplyChain.TransportResources)
        {
            if (resource.FixedUsageCost is null ||
                resource.CapacityConstraint is null)
            {
                continue;
            }

            TransportCharacteristic[] characteristics =
                instance.SupplyChain.TransportCharacteristics
                    .Where(characteristic =>
                        characteristic.TransportResourceId == resource.Id)
                    .ToArray();

            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression = new LinearExpressionBuilder();

                foreach (TransportCharacteristic characteristic
                         in characteristics)
                {
                    foreach (TransportLane lane in resource.Lanes)
                    {
                        expression.Add(
                            context.GetVariable(
                                StandardFormulationVariableKeyFactory
                                    .CreateTransportKey(
                                        characteristic.ItemId,
                                        resource.Id,
                                        lane.Origin,
                                        lane.Destination,
                                        period)),
                            characteristic.UnitCapacityConsumption?[period] ??
                                1.0);
                    }
                }

                string activationKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .TransportResourceActivation)
                        .Add(
                            MathematicalDomainKeySegment.TransportResource,
                            resource.Id)
                        .Add(MathematicalDomainKeySegment.Period, period)
                        .Build();

                expression.Subtract(
                    context.GetVariable(activationKey),
                    resource.CapacityConstraint[period]);

                AddConstraint(
                    context,
                    $"transportResourceActivationLink_r{resource.Id}" +
                    $"_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        "Transport load requires resource activation.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
