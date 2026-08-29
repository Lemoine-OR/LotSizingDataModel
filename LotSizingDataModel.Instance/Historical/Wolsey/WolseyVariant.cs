namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Third VAR field extensions of Wolsey's 2002 single-item classification.
/// </summary>
/// <remarks>
/// The names deliberately preserve Wolsey's semantics:
/// SC means Start-Up Costs, not setup costs.
/// ST means Start-Up Times, not setup times.
/// SL means Sales, not lost sales.
/// </remarks>
public enum WolseyVariant
{
    /// <summary>Backlogging.</summary>
    B = 0,

    /// <summary>Start-up costs.</summary>
    SC = 1,

    /// <summary>Start-up times.</summary>
    ST = 2,

    /// <summary>Constant start-up times, Wolsey ST(C).</summary>
    STConstant = 3,

    /// <summary>Additional sales.</summary>
    SL = 4,

    /// <summary>Minimum production levels.</summary>
    LB = 5,

    /// <summary>Constant minimum production levels, Wolsey LB(C).</summary>
    LBConstant = 6,

    /// <summary>Safety stocks.</summary>
    SS = 7
}
