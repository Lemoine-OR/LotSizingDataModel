namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Carries explicit derived analyses that cannot be inferred from the coarse
/// typed descriptor alone.
/// </summary>
/// <remarks>
/// Unknown is different from false. This distinction is required by positive
/// specification matching.
/// </remarks>
public sealed class UniversalDerivedSemantics
{
    private readonly IReadOnlyCollection<UniversalTemporalQualifier>
        _temporalQualifiers;

    private readonly IReadOnlyDictionary<
        UniversalSemanticCondition,
        UniversalConditionState> _conditionStates;

    public static UniversalDerivedSemantics Empty { get; } =
        new();

    public UniversalDerivedSemantics(
        IEnumerable<UniversalTemporalQualifier>? temporalQualifiers = null,
        IEnumerable<UniversalSemanticConditionAssessment>?
            conditionAssessments = null)
    {
        // Reuse the canonical beta temporal validation policy.
        var temporalOnlyBeta =
            new UniversalNotationBeta(
                temporalQualifiers: temporalQualifiers);

        _temporalQualifiers =
            temporalOnlyBeta.TemporalQualifiers.ToArray();

        var states =
            new Dictionary<
                UniversalSemanticCondition,
                UniversalConditionState>();

        foreach (
            UniversalSemanticConditionAssessment assessment
            in conditionAssessments ??
               Array.Empty<UniversalSemanticConditionAssessment>())
        {
            if (
                states.TryGetValue(
                    assessment.Condition,
                    out UniversalConditionState existing) &&
                existing != assessment.State)
            {
                throw new ArgumentException(
                    "Conflicting actual states were supplied for semantic " +
                    $"condition '{assessment.Condition}'.",
                    nameof(conditionAssessments));
            }

            states[assessment.Condition] =
                assessment.State;
        }

        _conditionStates = states;
    }

    public IReadOnlyCollection<UniversalTemporalQualifier>
        TemporalQualifiers =>
            _temporalQualifiers;

    public IReadOnlyCollection<UniversalSemanticCondition>
        SatisfiedConditions =>
            _conditionStates
                .Where(
                    pair =>
                        pair.Value ==
                        UniversalConditionState.Satisfied)
                .Select(pair => pair.Key)
                .OrderBy(condition => (int)condition)
                .ToArray();

    public bool TryGetConditionState(
        UniversalSemanticCondition condition,
        out UniversalConditionState state)
    {
        if (_conditionStates.TryGetValue(condition, out state))
        {
            return true;
        }

        state = UniversalConditionState.Unknown;
        return false;
    }
}
