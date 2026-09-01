using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Instance.Benchmark;

/// <summary>
/// Audits native KnownResult metadata for benchmark/BKS use.
/// No parallel verification-status enum is introduced.
/// </summary>
public sealed class BenchmarkKnownResultAuditService
{
    public BenchmarkKnownResultAuditRecord Audit(
        KnownResult result,
        bool allowReproduced = false,
        bool requireSourceEvidence = true)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        var diagnostics =
            new List<string>();

        bool hasObjective =
            result.ReportedObjectiveValue.HasValue &&
            double.IsFinite(
                result.ReportedObjectiveValue.Value);

        if (!hasObjective)
        {
            diagnostics.Add(
                "BKS-NO-OBJECTIVE");
        }

        bool hasSourceEvidence =
            !string.IsNullOrWhiteSpace(
                result.SourceTitle) ||
            !string.IsNullOrWhiteSpace(
                result.SourceReference) ||
            !string.IsNullOrWhiteSpace(
                result.SourceUri) ||
            !string.IsNullOrWhiteSpace(
                result.DigitalObjectIdentifier);

        if (!hasSourceEvidence)
        {
            diagnostics.Add(
                "BKS-NO-SOURCE");
        }

        bool verificationEligible =
            result.VerificationStatus is
                KnownResultVerificationStatus.AutomaticallyVerified or
                KnownResultVerificationStatus.IndependentlyVerified ||
            (
                allowReproduced &&
                result.VerificationStatus ==
                    KnownResultVerificationStatus.Reproduced
            );

        if (result.VerificationStatus ==
            KnownResultVerificationStatus.Disputed)
        {
            diagnostics.Add(
                "BKS-DISPUTED");
        }

        if (result.VerificationStatus ==
            KnownResultVerificationStatus.Invalidated)
        {
            diagnostics.Add(
                "BKS-INVALIDATED");
        }

        if (!verificationEligible)
        {
            diagnostics.Add(
                "BKS-VERIFICATION-INELIGIBLE");
        }

        bool sourceEligible =
            !requireSourceEvidence ||
            hasSourceEvidence;

        bool eligible =
            hasObjective &&
            verificationEligible &&
            sourceEligible;

        return new BenchmarkKnownResultAuditRecord(
            result.ResultId,
            result.VerificationStatus,
            hasObjective,
            hasSourceEvidence,
            eligible,
            diagnostics);
    }
}
