namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Declares whether a production setup state may persist between periods.
/// </summary>
public enum SetupCarryOverPolicy
{
    Unspecified = 0,
    Forbidden = 1,
    Allowed = 2
}
