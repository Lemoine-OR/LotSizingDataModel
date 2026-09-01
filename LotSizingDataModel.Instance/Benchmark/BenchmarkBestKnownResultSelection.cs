using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Instance.Benchmark;

public sealed class BenchmarkBestKnownResultSelection
{
    public BenchmarkBestKnownResultSelection(
        KnownResult? selectedResult,
        IReadOnlyList<BenchmarkKnownResultAuditRecord> audits)
    {
        SelectedResult =
            selectedResult;

        Audits =
            audits?.ToArray() ??
            throw new ArgumentNullException(
                nameof(audits));
    }

    public KnownResult? SelectedResult
    {
        get;
    }

    public IReadOnlyList<BenchmarkKnownResultAuditRecord> Audits
    {
        get;
    }

    public bool HasSelection =>
        SelectedResult is not null;
}
