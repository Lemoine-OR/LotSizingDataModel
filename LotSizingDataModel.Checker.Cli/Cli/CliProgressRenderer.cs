using LotSizingDataModel.Checker.Campaign;

namespace LotSizingDataModel.Checker.Cli;

internal sealed class CliProgressRenderer : IProgress<DirectoryVerificationCampaignProgress>
{
    private readonly object _sync = new();
    private readonly bool _interactive;
    private DirectoryVerificationCampaignStage? _lastStage;
    private int _lastRenderedWidth;
    private int _lastRedirectedFileCount = -1;
    private int _lastRedirectedCandidateCount = -1;

    public CliProgressRenderer()
    {
        _interactive = !Console.IsErrorRedirected;
    }

    public void Report(DirectoryVerificationCampaignProgress value)
    {
        ArgumentNullException.ThrowIfNull(value);

        lock (_sync)
        {
            if (_interactive)
            {
                RenderInteractive(value);
            }
            else
            {
                RenderRedirected(value);
            }

            _lastStage = value.Stage;
        }
    }

    public void CompleteLine()
    {
        lock (_sync)
        {
            if (_interactive && _lastRenderedWidth > 0)
            {
                Console.Error.WriteLine();
                _lastRenderedWidth = 0;
            }
        }
    }

    private void RenderInteractive(
        DirectoryVerificationCampaignProgress value)
    {
        string text = Format(value);
        int padding = Math.Max(0, _lastRenderedWidth - text.Length);

        Console.Error.Write('\r');
        Console.Error.Write(text);

        if (padding > 0)
        {
            Console.Error.Write(new string(' ', padding));
            Console.Error.Write(new string('\b', padding));
        }

        _lastRenderedWidth = text.Length;

        if (value.Stage == DirectoryVerificationCampaignStage.Completed)
        {
            Console.Error.WriteLine();
            _lastRenderedWidth = 0;
        }
    }

    private void RenderRedirected(
        DirectoryVerificationCampaignProgress value)
    {
        bool stageChanged = _lastStage != value.Stage;

        bool shouldWrite =
            stageChanged ||
            value.Stage == DirectoryVerificationCampaignStage.Completed;

        if (value.Stage == DirectoryVerificationCampaignStage.LoadingFiles &&
            value.ProcessedFileCount > 0 &&
            (value.ProcessedFileCount == value.DiscoveredFileCount ||
             value.ProcessedFileCount - _lastRedirectedFileCount >= 100))
        {
            shouldWrite = true;
            _lastRedirectedFileCount = value.ProcessedFileCount;
        }

        if (value.Stage == DirectoryVerificationCampaignStage.VerifyingCandidates &&
            value.CompletedCandidateCount > 0 &&
            (value.CompletedCandidateCount == value.CandidateCount ||
             value.CompletedCandidateCount - _lastRedirectedCandidateCount >= 25))
        {
            shouldWrite = true;
            _lastRedirectedCandidateCount = value.CompletedCandidateCount;
        }

        if (shouldWrite)
        {
            Console.Error.WriteLine(Format(value));
        }
    }

    private static string Format(
        DirectoryVerificationCampaignProgress value)
    {
        return value.Stage switch
        {
            DirectoryVerificationCampaignStage.DiscoveringFiles =>
                "Discovering XML files...",

            DirectoryVerificationCampaignStage.LoadingFiles =>
                $"Loading XML files: {value.ProcessedFileCount}/{value.DiscoveredFileCount} " +
                $"(instances={value.LoadedInstanceCount})",

            DirectoryVerificationCampaignStage.VerifyingCandidates =>
                $"Checking candidates: {value.CompletedCandidateCount}/{value.CandidateCount}",

            DirectoryVerificationCampaignStage.WritingReports =>
                "Writing campaign reports...",

            DirectoryVerificationCampaignStage.Completed =>
                $"Campaign complete: candidates={value.CandidateCount}",

            _ =>
                value.Stage.ToString()
        };
    }
}
