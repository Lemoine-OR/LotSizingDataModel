namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Stable canonical LotSizingDataModel identifiers for problem classes.
/// </summary>
public enum CanonicalLotSizingProblemClassId
{
    SingleItemUncapacitatedLotSizing,
    SingleItemCapacitatedLotSizing,
    MultiItemUncapacitatedLotSizing,
    MultiItemCapacitatedLotSizing,
    UncapacitatedMultiLevelLotSizing,
    MultiLevelCapacitatedLotSizing,

    DiscreteLotSizingAndScheduling,
    ContinuousSetupLotSizing,
    ProportionalLotSizingAndScheduling,
    GeneralLotSizingAndScheduling
}
