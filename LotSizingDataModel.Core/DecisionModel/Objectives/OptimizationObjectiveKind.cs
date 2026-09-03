namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Explicit business-level optimization objective family.
/// </summary>
public enum OptimizationObjectiveKind
{
    Unknown = 0,
    Economic = 1,
    Financial = 2,
    Sustainability = 3,
    ServiceLevel = 4
}
