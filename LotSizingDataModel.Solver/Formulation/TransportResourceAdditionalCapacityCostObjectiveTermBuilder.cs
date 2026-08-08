using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds global additional transport-resource capacity costs.
/// </summary>
public sealed class TransportResourceAdditionalCapacityCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "transportResourceAdditionalCapacityCost";

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
            options.IncludeAdditionalCapacity;
    }

    /// <summary>
    /// Builds global additional transport-resource capacity cost
    /// terms.
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

                if (options.RemoveStructurallyZeroVariables &&
                    double.IsFinite(upperBound) &&
                    upperBound <= options.StructuralZeroTolerance)
                {
                    continue;
                }

                string domainKey =
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

                AddCostTerm(
                    context,
                    expressionBuilder,
                    domainKey,
                    resource.AdditionalCapacityCost?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
