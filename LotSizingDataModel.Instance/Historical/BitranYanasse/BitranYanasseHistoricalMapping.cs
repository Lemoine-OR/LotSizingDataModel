using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Historical.BitranYanasse;

/// <summary>
/// Represents a lossless historical Bitran-Yanasse profile together with the
/// lossless projection to universal notation scheme v1.
/// </summary>
public sealed class BitranYanasseHistoricalMapping
{
    internal BitranYanasseHistoricalMapping(
        BitranYanasseTemporalProfile historicalProfile,
        UniversalProblemSpecification universalDomainSpecification,
        HistoricalMappingCoverage coverage,
        IEnumerable<string> unrepresentedHistoricalDimensions,
        BitranYanasseApplicabilityAssessment? applicability)
    {
        HistoricalProfile =
            historicalProfile ??
            throw new ArgumentNullException(
                nameof(historicalProfile));

        UniversalDomainSpecification =
            universalDomainSpecification ??
            throw new ArgumentNullException(
                nameof(universalDomainSpecification));

        Coverage = coverage;

        UnrepresentedHistoricalDimensions =
            (unrepresentedHistoricalDimensions ??
             throw new ArgumentNullException(
                 nameof(unrepresentedHistoricalDimensions)))
                .ToArray();

        Applicability = applicability;
    }

    /// <summary>
    /// Gets the complete historical alpha/beta/gamma/delta temporal profile.
    /// No historical information is discarded.
    /// </summary>
    public BitranYanasseTemporalProfile HistoricalProfile { get; }

    public string HistoricalCode =>
        HistoricalProfile.HistoricalCode;

    /// <summary>
    /// Gets the universal specification containing the historical problem
    /// domain and all four temporal-pattern dimensions.
    /// </summary>
    public UniversalProblemSpecification
        UniversalDomainSpecification { get; }

    public HistoricalMappingCoverage Coverage { get; }

    /// <summary>
    /// Gets historical dimensions not represented by the universal grammar.
    /// For the current Bitran-Yanasse mapping this collection is empty.
    /// </summary>
    public IReadOnlyList<string>
        UnrepresentedHistoricalDimensions { get; }

    public BitranYanasseApplicabilityAssessment?
        Applicability { get; }
}
