namespace LotSizingDataModel.Instance.Historical;

public sealed class HistoricalInverseResolution
{
    public HistoricalInverseResolution(
        HistoricalInverseResolutionStatus status,
        IReadOnlyList<HistoricalMappingRule> candidates)
    {
        Status = status;
        Candidates = candidates?.ToArray() ??
            throw new ArgumentNullException(nameof(candidates));

        if (Status == HistoricalInverseResolutionStatus.Unique &&
            Candidates.Count != 1)
        {
            throw new InvalidOperationException(
                "A unique inverse resolution must contain exactly one candidate.");
        }

        if (Status == HistoricalInverseResolutionStatus.NotFound &&
            Candidates.Count != 0)
        {
            throw new InvalidOperationException(
                "A not-found resolution cannot contain candidates.");
        }

        if (Status == HistoricalInverseResolutionStatus.Ambiguous &&
            Candidates.Count < 2)
        {
            throw new InvalidOperationException(
                "An ambiguous resolution requires at least two candidates.");
        }
    }

    public HistoricalInverseResolutionStatus Status { get; }
    public IReadOnlyList<HistoricalMappingRule> Candidates { get; }

    public HistoricalMappingRule? UniqueRule =>
        Status == HistoricalInverseResolutionStatus.Unique
            ? Candidates[0]
            : null;
}
