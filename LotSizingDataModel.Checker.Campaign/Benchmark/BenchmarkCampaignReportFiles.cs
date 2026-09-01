namespace LotSizingDataModel.Checker.Campaign.Benchmark;

public sealed record BenchmarkCampaignReportFiles(
    string JsonPath,
    string CsvPath,
    string Sha256Path);
