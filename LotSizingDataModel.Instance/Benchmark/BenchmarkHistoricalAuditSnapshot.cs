using LotSizingDataModel.Instance.Historical;

namespace LotSizingDataModel.Instance.Benchmark;

public sealed class BenchmarkHistoricalAuditSnapshot
{
    public BenchmarkHistoricalAuditSnapshot(
        HistoricalMappingAuditResult audit)
    {
        ArgumentNullException.ThrowIfNull(
            audit);

        Family =
            audit.Family.ToString();

        IsExactMatch =
            audit.IsExactMatch;

        DeclaredButNotDetected =
            audit.DeclaredButNotDetected.ToArray();

        DetectedButNotDeclared =
            audit.DetectedButNotDeclared.ToArray();
    }

    public string Family
    {
        get;
    }

    public bool IsExactMatch
    {
        get;
    }

    public IReadOnlyList<string> DeclaredButNotDetected
    {
        get;
    }

    public IReadOnlyList<string> DetectedButNotDeclared
    {
        get;
    }
}
