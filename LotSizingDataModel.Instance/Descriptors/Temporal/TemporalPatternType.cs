namespace LotSizingDataModel.Instance.Descriptors.Temporal;

/// <summary>
/// Canonical temporal-pattern categories used by historical lot-sizing
/// classifications such as Bitran-Yanasse.
/// </summary>
public enum TemporalPatternType
{
    Zero,
    Constant,
    NonIncreasing,
    NonDecreasing,
    General
}
