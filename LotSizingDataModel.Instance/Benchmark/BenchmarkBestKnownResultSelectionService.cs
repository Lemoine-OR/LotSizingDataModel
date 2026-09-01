using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Instance.Benchmark;

/// <summary>
/// Selects an eligible benchmark reference objective from
/// native KnownResult records.
/// </summary>
public sealed class BenchmarkBestKnownResultSelectionService
{
    private readonly BenchmarkKnownResultAuditService
        _auditService =
            new();

    public BenchmarkBestKnownResultSelection Select(
        IEnumerable<KnownResult> results,
        BenchmarkObjectiveDirection direction,
        bool allowReproduced = false,
        bool requireSourceEvidence = true)
    {
        ArgumentNullException.ThrowIfNull(
            results);

        if (!Enum.IsDefined(
                typeof(BenchmarkObjectiveDirection),
                direction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction));
        }

        KnownResult[] normalized =
            results.ToArray();

        var audits =
            normalized.ToDictionary(
                result =>
                    result.ResultId,
                result =>
                    _auditService.Audit(
                        result,
                        allowReproduced,
                        requireSourceEvidence),
                StringComparer.Ordinal);

        IEnumerable<KnownResult> eligible =
            normalized.Where(
                result =>
                    audits[result.ResultId]
                        .IsReferenceEligible);

        KnownResult? selected =
            direction ==
                BenchmarkObjectiveDirection.Minimize
                ? eligible
                    .OrderBy(
                        result =>
                            result.ReportedObjectiveValue!.Value)
                    .ThenBy(
                        result =>
                            result.ResultId,
                        StringComparer.Ordinal)
                    .FirstOrDefault()
                : eligible
                    .OrderByDescending(
                        result =>
                            result.ReportedObjectiveValue!.Value)
                    .ThenBy(
                        result =>
                            result.ResultId,
                        StringComparer.Ordinal)
                    .FirstOrDefault();

        return new BenchmarkBestKnownResultSelection(
            selected,
            audits.Values
                .OrderBy(
                    audit =>
                        audit.ResultId,
                    StringComparer.Ordinal)
                .ToArray());
    }
}
