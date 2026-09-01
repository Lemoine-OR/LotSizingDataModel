namespace LotSizingDataModel.Checker.Campaign.Benchmark;

public sealed class BenchmarkCampaignReport
{
    public const string SchemaVersion =
        "1.0-alpha.42";

    public string CampaignId
    {
        get;
        init;
    } =
        string.Empty;

    public DateTime GeneratedAtUtc
    {
        get;
        init;
    }

    public IReadOnlyList<BenchmarkCampaignRunRecord>
        Runs
    {
        get;
        init;
    } =
        Array.Empty<BenchmarkCampaignRunRecord>();

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                CampaignId))
        {
            throw new InvalidOperationException(
                "A campaign identifier is required.");
        }

        if (GeneratedAtUtc.Kind !=
            DateTimeKind.Utc)
        {
            throw new InvalidOperationException(
                "Campaign generation timestamp must be UTC.");
        }

        ArgumentNullException.ThrowIfNull(
            Runs);

        foreach (BenchmarkCampaignRunRecord run
                 in Runs)
        {
            ArgumentNullException.ThrowIfNull(
                run);

            run.EnsureValid();
        }
    }
}
