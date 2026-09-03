namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Policy used to combine several enabled optimization criteria.
/// </summary>
public enum ObjectiveAggregationMode
{
    Single = 0,
    WeightedSum = 1,
    Lexicographic = 2
}
