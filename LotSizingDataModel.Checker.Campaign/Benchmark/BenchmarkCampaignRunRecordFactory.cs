using LotSizingDataModel.Instance.Benchmark;
using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Checker.Campaign.Benchmark;

public sealed class BenchmarkCampaignRunRecordFactory
{
    public BenchmarkCampaignRunRecord Create(
        string instanceId,
        string instanceFingerprint,
        BenchmarkRunProvenance provenance,
        double? objectiveValue,
        bool hasFeasibleSolution,
        bool isOptimal,
        TimeSpan elapsed,
        BenchmarkObjectiveDirection direction,
        KnownResult? bks = null,
        BenchmarkHistoricalAuditSnapshot? historicalAudit = null)
    {
        ArgumentNullException.ThrowIfNull(
            provenance);

        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsed));
        }

        double? relativeGap =
            ComputeRelativeGap(
                objectiveValue,
                bks?.ReportedObjectiveValue,
                direction);

        var record =
            new BenchmarkCampaignRunRecord
            {
                InstanceId =
                    instanceId?.Trim() ??
                    string.Empty,

                InstanceFingerprint =
                    instanceFingerprint?.Trim() ??
                    string.Empty,

                Provenance =
                    provenance,

                ObjectiveValue =
                    objectiveValue,

                HasFeasibleSolution =
                    hasFeasibleSolution,

                IsOptimal =
                    isOptimal,

                ElapsedMilliseconds =
                    elapsed.TotalMilliseconds,

                BksResultId =
                    bks?.ResultId ??
                    string.Empty,

                BksObjectiveValue =
                    bks?.ReportedObjectiveValue,

                BksVerificationStatus =
                    bks?.VerificationStatus,

                RelativeGapToBks =
                    relativeGap,

                HistoricalAudit =
                    historicalAudit
            };

        record.EnsureValid();

        return record;
    }

    public static double? ComputeRelativeGap(
        double? candidate,
        double? reference,
        BenchmarkObjectiveDirection direction)
    {
        if (!candidate.HasValue ||
            !reference.HasValue)
        {
            return null;
        }

        if (!double.IsFinite(
                candidate.Value) ||
            !double.IsFinite(
                reference.Value))
        {
            throw new InvalidOperationException(
                "Candidate and BKS objective values must be finite.");
        }

        double deterioration =
            direction ==
                BenchmarkObjectiveDirection.Minimize
                ? candidate.Value -
                  reference.Value
                : reference.Value -
                  candidate.Value;

        return Math.Max(
                   0.0,
                   deterioration) /
               Math.Max(
                   1.0,
                   Math.Abs(
                       reference.Value));
    }
}
