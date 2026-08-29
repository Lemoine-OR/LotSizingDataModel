namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// First PROB field of Wolsey's 2002 single-item classification.
/// </summary>
public enum WolseyProblemVersion
{
    /// <summary>General lot-sizing problem.</summary>
    LS,

    /// <summary>
    /// Wagner-Whitin restriction: transformed inventory costs satisfy the
    /// nonnegative Wagner-Whitin cost condition.
    /// </summary>
    WW,

    /// <summary>
    /// Discrete lot-sizing with variable initial stock; production in each
    /// period is either zero or at full capacity.
    /// </summary>
    DLSI,

    /// <summary>
    /// Discrete lot-sizing without the variable initial-stock decision.
    /// </summary>
    DLS
}
