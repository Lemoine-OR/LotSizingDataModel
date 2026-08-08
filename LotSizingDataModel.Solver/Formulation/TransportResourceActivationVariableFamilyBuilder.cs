using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global transport-resource activation variables.
/// </summary>
public sealed class TransportResourceActivationVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the transport-resource activation family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.TransportResourceActivation;

    /// <summary>
    /// Determines whether resource activation variables are
    /// enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IncludeTransport ||
            !options.IncludeResourceActivation)
        {
            return false;
        }

        foreach (
            TransportResource resource
            in instance.SupplyChain.TransportResources)
        {
            if (resource.FixedUsageCost is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds global transport-resource activation variables.
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
            if (resource.FixedUsageCost is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .TransportResourceActivation)
                        .Add(
                            MathematicalDomainKeySegment
                                .TransportResource,
                            resource.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddBinaryVariable(
                    context,
                    $"YR_r{resource.Id}_t{period}",
                    domainKey,
                    $"Activation of transport resource " +
                    $"{resource.Id} in period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
