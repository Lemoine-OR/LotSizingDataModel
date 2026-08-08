using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global additional transport-resource capacity
/// variables.
/// </summary>
public sealed class TransportResourceAdditionalCapacityVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the transport-resource additional-capacity family
    /// identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory
            .TransportResourceAdditionalCapacity;

    /// <summary>
    /// Determines whether global additional transport-resource
    /// capacity variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IncludeTransport ||
            !options.IncludeAdditionalCapacity)
        {
            return false;
        }

        foreach (
            TransportResource resource
            in instance.SupplyChain.TransportResources)
        {
            if (resource.AdditionalCapacity is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds global additional transport-resource capacity
    /// variables.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            TransportResource resource
            in instance.SupplyChain.TransportResources)
        {
            if (resource.AdditionalCapacity is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double upperBound =
                    resource.AdditionalCapacity[period];

                if (IsStructurallyZero(
                        upperBound,
                        options))
                {
                    continue;
                }

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .TransportResourceAdditionalCapacity)
                        .Add(
                            MathematicalDomainKeySegment
                                .TransportResource,
                            resource.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"OR_r{resource.Id}_t{period}",
                    domainKey,
                    upperBound,
                    $"Additional capacity of transport resource " +
                    $"{resource.Id} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
