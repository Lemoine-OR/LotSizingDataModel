using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Historical.BitranYanasse;

namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Reports historical mapping/detection capability without inventing a
/// historical code that cannot be derived safely.
/// </summary>
public sealed class HistoricalClassificationCapabilityAnalyzer
{
    private readonly BitranYanasseHistoricalMapper _bitranYanasseMapper =
        new();

    public IReadOnlyList<HistoricalClassificationCapability> Analyze(
        LotSizingProblemDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        BitranYanasseApplicabilityAssessment by =
            _bitranYanasseMapper.AssessApplicability(
                descriptor);

        HistoricalClassificationCapabilityKind byKind =
            by.Kind switch
            {
                BitranYanasseApplicabilityKind.Incomplete =>
                    HistoricalClassificationCapabilityKind.Incomplete,

                BitranYanasseApplicabilityKind.NotApplicable =>
                    HistoricalClassificationCapabilityKind.NotApplicable,

                BitranYanasseApplicabilityKind.ExactHistoricalDomain =>
                    HistoricalClassificationCapabilityKind
                        .ApplicableNeedsExplicitProfile,

                BitranYanasseApplicabilityKind.ExtendedButProjectable =>
                    HistoricalClassificationCapabilityKind
                        .ExtendedProjectableNeedsExplicitProfile,

                _ => throw new ArgumentOutOfRangeException()
            };

        string byNote =
            by.Kind switch
            {
                BitranYanasseApplicabilityKind.Incomplete =>
                    "The descriptor lacks information required to decide " +
                    "membership in the classical Bitran-Yanasse domain.",

                BitranYanasseApplicabilityKind.NotApplicable =>
                    "The descriptor violates classical-domain requirements: " +
                    string.Join(",", by.FailedRequirements),

                BitranYanasseApplicabilityKind.ExactHistoricalDomain =>
                    "The complete historical code requires explicit setup, " +
                    "holding, production-cost and capacity temporal profiles.",

                BitranYanasseApplicabilityKind.ExtendedButProjectable =>
                    "The classical domain is projectable, with extensions: " +
                    string.Join(",", by.Extensions),

                _ => throw new ArgumentOutOfRangeException()
            };

        return new[]
        {
            new HistoricalClassificationCapability(
                code: "BY",
                name: "Bitran-Yanasse temporal classification",
                kind: byKind,
                canMapDeclaredClassification: true,
                canDetectCompleteHistoricalCode: false,
                requiresExplicitParameterProfile: true,
                note: byNote),

            new HistoricalClassificationCapability(
                code: "WOLSEY-2002",
                name: "Wolsey 2002 problem classification",
                kind:
                    HistoricalClassificationCapabilityKind
                        .MappingSupportedOnly,
                canMapDeclaredClassification: true,
                canDetectCompleteHistoricalCode: false,
                requiresExplicitParameterProfile: false,
                note:
                    "Historical-to-universal mapping is supported, but the " +
                    "current engine does not invent a unique Wolsey source " +
                    "code from a descriptor.")
        };
    }
}
