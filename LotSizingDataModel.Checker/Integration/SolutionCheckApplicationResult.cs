namespace LotSizingDataModel.Checker.Integration;

/// <summary>
/// Describes which domain objects were changed while applying a checker
/// result.
/// </summary>
public sealed class SolutionCheckApplicationResult
{
    /// <summary>
    /// Gets or sets whether LotSizingSolution.Evaluation was updated.
    /// </summary>
    public bool SolutionEvaluationUpdated
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether a KnownResult feasibility status was updated.
    /// </summary>
    public bool KnownResultFeasibilityUpdated
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether a KnownResult was promoted to
    /// AutomaticallyVerified.
    /// </summary>
    public bool KnownResultPromoted
    {
        get;
        set;
    }
}
