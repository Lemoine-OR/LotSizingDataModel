using LotSizingDataModel.Instance.Benchmark;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Checker.Campaign.Benchmark;

public sealed class BenchmarkCampaignRunRecord
{
    public string InstanceId
    {
        get;
        init;
    } =
        string.Empty;

    public string InstanceFingerprint
    {
        get;
        init;
    } =
        string.Empty;

    public BenchmarkRunProvenance Provenance
    {
        get;
        init;
    } =
        null!;

    public double? ObjectiveValue
    {
        get;
        init;
    }

    public bool HasFeasibleSolution
    {
        get;
        init;
    }

    public bool IsOptimal
    {
        get;
        init;
    }

    public double ElapsedMilliseconds
    {
        get;
        init;
    }

    public string BksResultId
    {
        get;
        init;
    } =
        string.Empty;

    public double? BksObjectiveValue
    {
        get;
        init;
    }

    public KnownResultVerificationStatus?
        BksVerificationStatus
    {
        get;
        init;
    }

    public double? RelativeGapToBks
    {
        get;
        init;
    }

    public BenchmarkHistoricalAuditSnapshot?
        HistoricalAudit
    {
        get;
        init;
    }

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                InstanceId))
        {
            throw new InvalidOperationException(
                "A benchmark campaign record requires an instance identifier.");
        }

        ArgumentNullException.ThrowIfNull(
            Provenance);

        Provenance.EnsureValid();

        if (ObjectiveValue.HasValue &&
            !double.IsFinite(
                ObjectiveValue.Value))
        {
            throw new InvalidOperationException(
                "Benchmark objective value must be finite.");
        }

        if (!double.IsFinite(
                ElapsedMilliseconds) ||
            ElapsedMilliseconds < 0.0)
        {
            throw new InvalidOperationException(
                "Benchmark elapsed time must be finite and non-negative.");
        }

        if (BksObjectiveValue.HasValue &&
            !double.IsFinite(
                BksObjectiveValue.Value))
        {
            throw new InvalidOperationException(
                "BKS objective value must be finite.");
        }

        if (RelativeGapToBks.HasValue &&
            (!double.IsFinite(
                 RelativeGapToBks.Value) ||
             RelativeGapToBks.Value < 0.0))
        {
            throw new InvalidOperationException(
                "Relative BKS gap must be finite and non-negative.");
        }
    }
}
