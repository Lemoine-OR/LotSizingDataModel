namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Business-level optimization criterion family.
/// </summary>
public enum OptimizationObjectiveKind
{
    Unknown = 0,
    Economic = 1,
    Financial = 2,
    Sustainability = 3,
    ServiceLevel = 4
}
