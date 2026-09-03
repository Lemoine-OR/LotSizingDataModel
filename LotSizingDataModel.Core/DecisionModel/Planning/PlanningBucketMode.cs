namespace LotSizingDataModel.Core.DecisionModel.Planning;

/// <summary>
/// Explicit time-bucket interpretation used by a planning model.
/// </summary>
public enum PlanningBucketMode
{
    Unspecified = 0,
    BigBucket = 1,
    SmallBucket = 2,
    MacroMicro = 3,
    Hybrid = 4
}
