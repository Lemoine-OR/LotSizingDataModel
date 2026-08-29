namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>
/// Production-capacity regime derived from already represented descriptor
/// semantics. No new serialized source flag is introduced.
/// </summary>
public enum ProductionCapacityRegime
{
    NotApplicable,
    Uncapacitated,
    Constant,
    TimeVarying
}
