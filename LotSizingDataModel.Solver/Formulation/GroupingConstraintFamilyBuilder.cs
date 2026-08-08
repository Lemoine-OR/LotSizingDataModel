using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Enforces the minimum temporal spacing between consecutive
/// production setups defined by each routing grouping
/// constraint.
/// </summary>
/// <remarks>
/// <para>
/// The grouping value is interpreted as a minimum setup spacing
/// measured in planning periods.
/// </para>
/// <para>
/// If a routing has <c>GroupingConstraint[t] = g</c> and a setup
/// occurs in period <c>t</c>, no new setup may occur in periods
/// <c>t + 1</c> through <c>t + g - 1</c>. A new setup is allowed
/// again in period <c>t + g</c>.
/// </para>
/// <para>
/// For a time-dependent grouping value, the formulation uses
/// pairwise inequalities:
/// </para>
/// <code>
/// y[r,t] + y[r,k] &lt;= 1
/// for k = t+1, ..., min(T, t+g[t]-1).
/// </code>
/// <para>
/// When the grouping value is constant, this is equivalent to
/// requiring at most one setup in every sliding window of that
/// many consecutive periods.
/// </para>
/// </remarks>
public sealed class GroupingConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "productionSetupGrouping";

    /// <summary>
    /// Determines whether at least one production routing
    /// defines a grouping constraint.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when at least one routing defines
    /// a grouping constraint; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when an argument is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when grouping constraints are present while
    /// production setup variables are disabled.
    /// </exception>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        bool hasGroupingConstraint =
            false;

        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.GroupingConstraint is not null)
            {
                hasGroupingConstraint =
                    true;

                break;
            }
        }

        if (hasGroupingConstraint &&
            !options.IncludeProductionSetups)
        {
            throw new InvalidOperationException(
                "Production grouping constraints require " +
                "production setup variables. Enable " +
                "IncludeProductionSetups before building the " +
                "standard formulation.");
        }

        return hasGroupingConstraint;
    }

    /// <summary>
    /// Builds all setup-spacing constraints.
    /// </summary>
    /// <param name="instance">
    /// Source lot-sizing instance.
    /// </param>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel constraint construction.
    /// </param>
    /// <returns>
    /// Task representing grouping-constraint construction.
    /// </returns>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        int planningHorizon =
            instance.PlanningHorizon;

        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            if (routing.GroupingConstraint is null)
            {
                continue;
            }

            for (
                int setupPeriod = 1;
                setupPeriod <= planningHorizon;
                setupPeriod++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int minimumSpacing =
                    routing.GroupingConstraint[setupPeriod];

                if (minimumSpacing <= 0)
                {
                    throw new InvalidOperationException(
                        $"Grouping constraint for routing " +
                        $"{routing.Id}, period {setupPeriod}, " +
                        "must be strictly positive.");
                }

                if (minimumSpacing == 1)
                {
                    continue;
                }

                MathematicalVariable firstSetup =
                    GetSetupVariable(
                        context,
                        routing.Id,
                        setupPeriod);

                int lastForbiddenPeriod =
                    Math.Min(
                        planningHorizon,
                        setupPeriod +
                        minimumSpacing -
                        1);

                for (
                    int forbiddenPeriod = setupPeriod + 1;
                    forbiddenPeriod <= lastForbiddenPeriod;
                    forbiddenPeriod++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    MathematicalVariable laterSetup =
                        GetSetupVariable(
                            context,
                            routing.Id,
                            forbiddenPeriod);

                    var expression =
                        new LinearExpressionBuilder()
                            .Add(
                                firstSetup)
                            .Add(
                                laterSetup);

                    AddConstraint(
                        context,
                        $"grouping_r{routing.Id}" +
                        $"_t{setupPeriod}" +
                        $"_k{forbiddenPeriod}",
                        expression.Build(),
                        MathematicalConstraintSense.LessThanOrEqual,
                        1.0,
                        description:
                            $"A setup in period {setupPeriod} " +
                            $"for routing {routing.Id} forbids " +
                            $"another setup before period " +
                            $"{setupPeriod + minimumSpacing}.");
                }
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Resolves one production setup variable.
    /// </summary>
    /// <param name="context">
    /// Mathematical-model build context.
    /// </param>
    /// <param name="routingId">
    /// Production routing identifier.
    /// </param>
    /// <param name="period">
    /// One-based planning period.
    /// </param>
    /// <returns>
    /// Registered binary setup variable.
    /// </returns>
    private static MathematicalVariable GetSetupVariable(
        MathematicalModelBuildContext context,
        int routingId,
        int period)
    {
        string domainKey =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Setup)
                .Add(
                    MathematicalDomainKeySegment.Routing,
                    routingId)
                .Add(
                    MathematicalDomainKeySegment.Period,
                    period)
                .Build();

        return context.GetVariable(
            domainKey);
    }
}
