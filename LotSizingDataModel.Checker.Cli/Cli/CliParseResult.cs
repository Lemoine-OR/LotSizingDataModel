namespace LotSizingDataModel.Checker.Cli;

internal sealed class CliParseResult
{
    public CliOptions? Options { get; init; }
    public string? ErrorMessage { get; init; }
    public bool ShowHelp { get; init; }
    public bool ShowVersion { get; init; }
    public bool Success => ErrorMessage is null;
}
