namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Time-bucket structure used by an integrated lot-sizing and scheduling
/// description.
/// </summary>
public enum SchedulingBucketMode
{
    Unspecified = 0,

    /// <summary>
    /// A planning period may contain several lots/setups.
    /// </summary>
    BigBucket = 1,

    /// <summary>
    /// The planning horizon is expressed directly with short scheduling
    /// buckets.
    /// </summary>
    SmallBucket = 2,

    /// <summary>
    /// Each planning macro-period is subdivided into explicit micro-periods,
    /// as in GLSP-style time structures.
    /// </summary>
    MacroMicro = 3
}
