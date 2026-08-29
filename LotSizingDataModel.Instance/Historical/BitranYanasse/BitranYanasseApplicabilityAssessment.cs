namespace LotSizingDataModel.Instance.Historical.BitranYanasse;

/// <summary>
/// Structured assessment of Bitran-Yanasse historical-domain applicability.
/// </summary>
public sealed class BitranYanasseApplicabilityAssessment
{
    internal BitranYanasseApplicabilityAssessment(
        BitranYanasseApplicabilityKind kind,
        IEnumerable<string> failedRequirements,
        IEnumerable<string> extensions)
    {
        Kind = kind;

        FailedRequirements =
            (failedRequirements ??
             throw new ArgumentNullException(
                 nameof(failedRequirements)))
                .ToArray();

        Extensions =
            (extensions ??
             throw new ArgumentNullException(
                 nameof(extensions)))
                .ToArray();
    }

    public BitranYanasseApplicabilityKind Kind { get; }

    /// <summary>
    /// Gets defining historical-domain conditions that are not satisfied.
    /// Empty for applicable/projectable instances.
    /// </summary>
    public IReadOnlyList<string> FailedRequirements { get; }

    /// <summary>
    /// Gets additional modeled characteristics outside the strict historical
    /// domain but not erasing the classical capacitated single-item core.
    /// </summary>
    public IReadOnlyList<string> Extensions { get; }

    public bool IsApplicable =>
        Kind is
            BitranYanasseApplicabilityKind.ExactHistoricalDomain or
            BitranYanasseApplicabilityKind.ExtendedButProjectable;
}
