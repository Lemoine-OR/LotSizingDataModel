namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes planning-time dimensions.</summary>
public sealed class TimeDescriptor
{
    public int PlanningHorizon { get; init; }
    public bool IsSinglePeriod => PlanningHorizon == 1;
    public bool IsMultiPeriod => PlanningHorizon > 1;
}
