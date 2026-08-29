namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// One explicit actual assessment of a generic semantic condition.
/// </summary>
public sealed record UniversalSemanticConditionAssessment
{
    public UniversalSemanticConditionAssessment(
        UniversalSemanticCondition condition,
        UniversalConditionState state)
    {
        if (!Enum.IsDefined(condition))
        {
            throw new ArgumentOutOfRangeException(
                nameof(condition),
                condition,
                "Unknown universal semantic condition.");
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown universal condition state.");
        }

        Condition = condition;
        State = state;
    }

    public UniversalSemanticCondition Condition { get; }
    public UniversalConditionState State { get; }
}
