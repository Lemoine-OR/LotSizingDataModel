namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Generic derived semantic conditions that may be required by a universal
/// problem specification.
/// </summary>
/// <remarks>
/// These are reusable lot-sizing semantics, never historical-classification
/// tokens.
/// </remarks>
public enum UniversalSemanticCondition
{
    /// <summary>
    /// Adjacent unit production/acquisition and holding costs satisfy the
    /// non-speculative condition.
    /// </summary>
    NonSpeculativeProductionHoldingCosts = 0,

    /// <summary>
    /// Whenever production is positive, the modeled lower bound and capacity
    /// force production exactly to full capacity.
    /// </summary>
    ZeroOrFullCapacityProduction = 1
}
