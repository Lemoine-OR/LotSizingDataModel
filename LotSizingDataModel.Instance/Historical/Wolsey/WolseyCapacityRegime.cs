namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Second CAP field of Wolsey's 2002 single-item classification.
/// </summary>
public enum WolseyCapacityRegime
{
    /// <summary>Capacities vary over time.</summary>
    C,

    /// <summary>Capacity is constant over time.</summary>
    CC,

    /// <summary>
    /// Uncapacitated case: the production limit does not restrict production
    /// over the remaining demand horizon.
    /// </summary>
    U
}
