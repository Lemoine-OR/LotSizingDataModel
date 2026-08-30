using LotSizingDataModel.Core.DecisionModel.Objectives;

namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>
/// Describes factual finance and objective semantics.
/// </summary>
public sealed class ObjectiveFinanceDescriptor
{
    public bool HasFinancialConstraints { get; init; }

    public bool HasMultipleObjectives { get; init; }

    public int ObjectiveCriterionCount { get; init; } = 1;

    public OptimizationObjectiveKind PrimaryObjectiveKind { get; init; } =
        OptimizationObjectiveKind.Economic;

    public ObjectiveAggregationMode AggregationMode { get; init; } =
        ObjectiveAggregationMode.Single;
}
