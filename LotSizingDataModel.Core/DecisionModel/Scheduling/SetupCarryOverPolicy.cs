namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Declares whether a machine/work-center setup state may be carried from one
/// planning bucket to the next.
/// </summary>
public enum SetupCarryOverPolicy
{
    Unspecified = 0,
    Forbidden = 1,
    Allowed = 2
}
