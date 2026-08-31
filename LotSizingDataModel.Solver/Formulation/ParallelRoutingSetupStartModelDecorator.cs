using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Analysis;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds source-preserving setup-start coordination constraints
/// across concurrent production routings.
/// </summary>
/// <remarks>
/// This decorator limits occurrences of the canonical setup-start
/// decision family. It does not constrain persistent setup state
/// and therefore does not alter GroupingConstraint semantics.
/// </remarks>
public sealed class ParallelRoutingSetupStartModelDecorator
{
    public MathematicalModel Apply(
        MathematicalModel sourceModel,
        IEnumerable<ProductionRouting> routings,
        ParallelRoutingSetupStartLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(
            sourceModel);

        ArgumentNullException.ThrowIfNull(
            routings);

        ArgumentNullException.ThrowIfNull(
            policy);

        sourceModel.EnsureValid();

        policy.EnsureValid();

        ProductionRouting[] routingArray =
            routings.ToArray();

        IReadOnlyList<ParallelRoutingTopologyDescriptor> topology =
            ParallelSchedulingTopologyAnalyzer.Analyze(
                routingArray);

        var routingById =
            routingArray.ToDictionary(
                routing =>
                    routing.Id);

        var setupVariables =
            sourceModel.Variables
                .Select(
                    variable =>
                        TryReadSetupCoordinate(
                            variable))
                .Where(
                    coordinate =>
                        coordinate is not null)
                .Select(
                    coordinate =>
                        coordinate!)
                .ToArray();

        MathematicalModel result =
            sourceModel.Clone();

        int nextConstraintId =
            result.Constraints.Count == 0
                ? 1
                : result.Constraints.Max(
                      constraint =>
                          constraint.Id) + 1;

        var usedNames =
            new HashSet<string>(
                result.Constraints.Select(
                    constraint =>
                        constraint.Name),
                StringComparer.Ordinal);

        foreach (ParallelRoutingTopologyDescriptor itemTopology
                 in topology)
        {
            IEnumerable<IGrouping<CoordinationKey, SetupCoordinate>>
                groups =
                    setupVariables
                        .Where(
                            coordinate =>
                                itemTopology.RoutingIds.Contains(
                                    coordinate.RoutingId))
                        .GroupBy(
                            coordinate =>
                                CreateCoordinationKey(
                                    coordinate,
                                    routingById,
                                    policy.Scope));

            foreach (IGrouping<CoordinationKey, SetupCoordinate> group
                     in groups)
            {
                SetupCoordinate[] coordinates =
                    group
                        .OrderBy(
                            coordinate =>
                                coordinate.RoutingId)
                        .ToArray();

                if (coordinates.Length <=
                    policy.MaximumConcurrentSetupStartsPerItem)
                {
                    continue;
                }

                var expression =
                    new LinearExpression();

                foreach (SetupCoordinate coordinate
                         in coordinates)
                {
                    expression.AddTerm(
                        coordinate.VariableId,
                        1.0);
                }

                string suffix =
                    policy.Scope ==
                    ParallelSchedulingCoordinationScope.WithinEachPlant
                        ? $"_p{group.Key.PlantId}"
                        : string.Empty;

                string name =
                    $"parallelSetupStartLimit_i{itemTopology.ItemId}" +
                    $"_t{group.Key.Period}{suffix}";

                if (!usedNames.Add(
                        name))
                {
                    throw new InvalidOperationException(
                        $"Duplicate parallel setup-start coordination constraint '{name}'.");
                }

                result.AddConstraint(
                    new LinearConstraint(
                        nextConstraintId++,
                        name,
                        expression,
                        MathematicalConstraintSense.LessThanOrEqual,
                        policy.MaximumConcurrentSetupStartsPerItem));
            }
        }

        result.EnsureValid();

        return result;
    }

    private static SetupCoordinate? TryReadSetupCoordinate(
        MathematicalVariable variable)
    {
        if (string.IsNullOrWhiteSpace(
                variable.DomainKey))
        {
            return null;
        }

        if (!MathematicalDomainKey.TryParse(
                variable.DomainKey,
                out MathematicalDomainKey? key) ||
            key is null ||
            !string.Equals(
                key.Category,
                MathematicalDecisionCategory.Setup,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!key.TryGetInt32(
                MathematicalDomainKeySegment.Routing,
                out int routingId) ||
            !key.TryGetInt32(
                MathematicalDomainKeySegment.Period,
                out int period))
        {
            return null;
        }

        if (routingId <= 0 ||
            period <= 0)
        {
            throw new InvalidOperationException(
                $"Setup variable '{variable.Name}' has invalid routing/period coordinates.");
        }

        return new SetupCoordinate(
            variable.Id,
            routingId,
            period);
    }

    private static CoordinationKey CreateCoordinationKey(
        SetupCoordinate coordinate,
        IReadOnlyDictionary<int, ProductionRouting> routingById,
        ParallelSchedulingCoordinationScope scope)
    {
        if (!routingById.TryGetValue(
                coordinate.RoutingId,
                out ProductionRouting? routing))
        {
            throw new InvalidOperationException(
                $"Setup variable references routing '{coordinate.RoutingId}', which is absent from the supplied routing topology.");
        }

        int plantId =
            scope ==
            ParallelSchedulingCoordinationScope.WithinEachPlant
                ? routing.PlantId
                : 0;

        return new CoordinationKey(
            coordinate.Period,
            plantId);
    }

    private sealed record SetupCoordinate(
        int VariableId,
        int RoutingId,
        int Period);

    private readonly record struct CoordinationKey(
        int Period,
        int PlantId);
}
