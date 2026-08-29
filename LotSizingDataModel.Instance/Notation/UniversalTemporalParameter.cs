namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Identifies a time-varying model parameter that may be qualified by a
/// generic temporal pattern in universal notation.
/// </summary>
/// <remarks>
/// Values are semantic parameter families, not historical-classification
/// positions. The catalog can grow independently from Bitran-Yanasse.
/// </remarks>
public enum UniversalTemporalParameter
{
    Demand = 0,
    SetupCost = 10,
    HoldingCost = 11,
    ProductionCost = 12,
    ProductionCapacity = 13
}
