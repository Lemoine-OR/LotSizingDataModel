using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Benchmark;

public sealed class BenchmarkKnownResultAuditRecord
{
    public BenchmarkKnownResultAuditRecord(
        string resultId,
        KnownResultVerificationStatus verificationStatus,
        bool hasObjectiveValue,
        bool hasSourceEvidence,
        bool isReferenceEligible,
        IReadOnlyList<string> diagnostics)
    {
        ResultId =
            resultId?.Trim() ??
            string.Empty;

        VerificationStatus =
            verificationStatus;

        HasObjectiveValue =
            hasObjectiveValue;

        HasSourceEvidence =
            hasSourceEvidence;

        IsReferenceEligible =
            isReferenceEligible;

        Diagnostics =
            diagnostics?.ToArray() ??
            throw new ArgumentNullException(
                nameof(diagnostics));

        if (ResultId.Length == 0)
        {
            throw new InvalidOperationException(
                "A benchmark known-result audit requires a result identifier.");
        }
    }

    public string ResultId
    {
        get;
    }

    public KnownResultVerificationStatus VerificationStatus
    {
        get;
    }

    public bool HasObjectiveValue
    {
        get;
    }

    public bool HasSourceEvidence
    {
        get;
    }

    public bool IsReferenceEligible
    {
        get;
    }

    public IReadOnlyList<string> Diagnostics
    {
        get;
    }
}
