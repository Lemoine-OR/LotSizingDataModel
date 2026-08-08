using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds demand-satisfaction constraints for each distribution
/// center, item, and period.
/// </summary>
/// <remarks>
/// The generated relation is:
/// <code>
/// sum(delivery)
/// + sum(current backlog)
/// - sum(previous backlog)
/// + sum(shortage)
/// = demand.
/// </code>
/// Backlog therefore carries unmet demand to the next period,
/// while shortage represents demand abandoned in the current
/// period.
/// </remarks>
public sealed class DemandSatisfactionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "demandSatisfaction";

    /// <summary>
    /// Determines whether demand constraints are required.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.Demands.Count > 0;
    }

    /// <summary>
    /// Builds demand-satisfaction constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (Demand demand in instance.SupplyChain.Demands)
        {
            DistributionCenterSourcing[] sourcings =
                instance.SupplyChain.DistributionCenterSourcings
                    .Where(
                        sourcing =>
                            sourcing.DistributionCenterId ==
                                demand.DistributionCenterId &&
                            sourcing.ItemId == demand.ItemId)
                    .ToArray();

            if (sourcings.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Demand for distribution center " +
                    $"{demand.DistributionCenterId} and item " +
                    $"{demand.ItemId} has no sourcing relation.");
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression =
                    new LinearExpressionBuilder();

                foreach (DistributionCenterSourcing sourcing
                         in sourcings)
                {
                    expression.Add(
                        context.GetVariable(
                            StandardFormulationVariableKeyFactory
                                .CreateDistributionKey(
                                    MathematicalDecisionCategory.Delivery,
                                    demand.DistributionCenterId,
                                    demand.ItemId,
                                    sourcing.Warehouse,
                                    period)));

                    if (options.IncludeBacklog &&
                        sourcing.BacklogConstraint is not null)
                    {
                        string currentBacklogKey =
                            StandardFormulationVariableKeyFactory
                                .CreateDistributionKey(
                                    MathematicalDecisionCategory.Backlog,
                                    demand.DistributionCenterId,
                                    demand.ItemId,
                                    sourcing.Warehouse,
                                    period);

                        if (context.VariableRegistry.TryGet(
                                currentBacklogKey,
                                out MathematicalVariable? currentBacklog) &&
                            currentBacklog is not null)
                        {
                            expression.Add(
                                currentBacklog);
                        }

                        if (period > 1)
                        {
                            string previousBacklogKey =
                                StandardFormulationVariableKeyFactory
                                    .CreateDistributionKey(
                                        MathematicalDecisionCategory.Backlog,
                                        demand.DistributionCenterId,
                                        demand.ItemId,
                                        sourcing.Warehouse,
                                        period - 1);

                            if (context.VariableRegistry.TryGet(
                                    previousBacklogKey,
                                    out MathematicalVariable? previousBacklog) &&
                                previousBacklog is not null)
                            {
                                expression.Subtract(
                                    previousBacklog);
                            }
                        }
                    }

                    if (options.IncludeShortage &&
                        sourcing.ShortageConstraint is not null)
                    {
                        string shortageKey =
                            StandardFormulationVariableKeyFactory
                                .CreateDistributionKey(
                                    MathematicalDecisionCategory.Shortage,
                                    demand.DistributionCenterId,
                                    demand.ItemId,
                                    sourcing.Warehouse,
                                    period);

                        if (context.VariableRegistry.TryGet(
                                shortageKey,
                                out MathematicalVariable? shortage) &&
                            shortage is not null)
                        {
                            expression.Add(
                                shortage);
                        }
                    }
                }

                AddConstraint(
                    context,
                    $"demand_c{demand.DistributionCenterId}" +
                    $"_i{demand.ItemId}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.Equal,
                    demand.GetQuantity(period),
                    description:
                        "Demand satisfaction with optional " +
                        "backlog and shortage.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
