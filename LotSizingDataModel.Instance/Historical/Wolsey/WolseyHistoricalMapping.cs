using LotSizingDataModel.Instance.Historical.BitranYanasse;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Represents a Wolsey historical classification and the portion currently
/// expressible as a universal problem specification.
/// </summary>
public sealed class WolseyHistoricalMapping
{
    internal WolseyHistoricalMapping(
        WolseyExtendedClassification historicalClassification,
        UniversalProblemSpecification universalSpecification,
        HistoricalMappingCoverage coverage,
        IEnumerable<string> unrepresentedHistoricalDimensions)
    {
        HistoricalClassification =
            historicalClassification ??
            throw new ArgumentNullException(
                nameof(historicalClassification));

        UniversalSpecification =
            universalSpecification ??
            throw new ArgumentNullException(
                nameof(universalSpecification));

        Coverage = coverage;

        UnrepresentedHistoricalDimensions =
            (unrepresentedHistoricalDimensions ??
             throw new ArgumentNullException(
                 nameof(unrepresentedHistoricalDimensions)))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
    }

    public WolseyExtendedClassification HistoricalClassification { get; }

    public string HistoricalCode =>
        HistoricalClassification.HistoricalCode;

    public UniversalProblemSpecification UniversalSpecification { get; }

    public HistoricalMappingCoverage Coverage { get; }

    public IReadOnlyList<string>
        UnrepresentedHistoricalDimensions { get; }
}
