namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Production-quantity policy inside one small scheduling bucket.
/// </summary>
public enum SmallBucketProductionMode
{
    /// <summary>
    /// The production-quantity policy is not declared.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Production is either zero or exactly the complete available bucket
    /// capacity for the selected item.
    /// </summary>
    AllOrNothing = 1,

    /// <summary>
    /// Any non-negative production quantity up to available bucket capacity
    /// may be selected.
    /// </summary>
    Continuous = 2
}
