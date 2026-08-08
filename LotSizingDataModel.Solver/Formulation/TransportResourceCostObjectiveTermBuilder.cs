using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds fixed global transport-resource activation costs.
/// </summary>
public sealed class TransportResourceCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "transportResourceCost";

    /// <summary>
    /// Determines whether these costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            options.IncludeResourceActivation;
    }

    /// <summary>
    /// Builds transport-resource activation cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
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
                            MathematicalDomainKeySegment.TransportResource,
                            resource.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddCostTerm(
                    context,
                    expressionBuilder,
                    domainKey,
                    resource.FixedUsageCost[period]);
            }
        }

        return ValueTask.CompletedTask;
    }
}
