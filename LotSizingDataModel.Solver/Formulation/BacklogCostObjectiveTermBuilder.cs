using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds backlog costs to the standard objective.
/// </summary>
public sealed class BacklogCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "backlogCost";

    /// <summary>
    /// Determines whether backlog costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeBacklog;
    }

    /// <summary>
    /// Builds backlog cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            DistributionCenterSourcing sourcing
            in instance.SupplyChain.DistributionCenterSourcings)
        {
            if (sourcing.BacklogConstraint is null)
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
                    sourcing.BacklogConstraint[period];

                if (options.RemoveStructurallyZeroVariables &&
                    double.IsFinite(upperBound) &&
                    upperBound <= options.StructuralZeroTolerance)
                {
                    continue;
                }

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Backlog)
                        .Add(
                            MathematicalDomainKeySegment.DistributionCenter,
                            sourcing.DistributionCenterId)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            sourcing.ItemId);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    sourcing.Warehouse);

                AddCostTerm(
                    context,
                    expressionBuilder,
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                    sourcing.BacklogCost?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
