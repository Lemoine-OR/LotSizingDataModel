namespace LotSizingDataModel.Checker.Configuration;

/// <summary>
/// Configures the high-level verification facade and the application of
/// checker results back to domain objects.
/// </summary>
public sealed class SolutionVerificationOptions
{
    /// <summary>
    /// Gets or sets the options used by the underlying solution checker.
    /// </summary>
    public SolutionCheckOptions CheckOptions
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Gets or sets whether the computed checker result is written to
    /// <c>LotSizingSolution.Evaluation</c>.
    /// </summary>
    public bool ApplyToSolutionEvaluation
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether an explicitly supplied known result receives
    /// the independently established feasibility status.
    /// </summary>
    public bool UpdateKnownResultFeasibility
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether a known result is promoted to
    /// AutomaticallyVerified when a complete Full check succeeds.
    /// </summary>
    public bool PromoteFullyVerifiedKnownResult
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets the evaluator name written to the solution evaluation.
    /// </summary>
    public string EvaluatorName
    {
        get;
        set;
    } = "LotSizingSolutionChecker";

    /// <summary>
    /// Gets or sets the evaluator version written to the solution evaluation.
    /// </summary>
    public string EvaluatorVersion
    {
        get;
        set;
    } = "1.0";

    /// <summary>
    /// Validates this configuration.
    /// </summary>
    public void EnsureValid()
    {
        if (CheckOptions is null)
        {
            throw new InvalidOperationException(
                "CheckOptions cannot be null.");
        }

        CheckOptions.EnsureValid();

        if (string.IsNullOrWhiteSpace(EvaluatorName))
        {
            throw new InvalidOperationException(
                "EvaluatorName cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(EvaluatorVersion))
        {
            throw new InvalidOperationException(
                "EvaluatorVersion cannot be empty.");
        }
    }
}
