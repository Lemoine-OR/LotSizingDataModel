using System.Text;
using LotSizingDataModel.Checker.Campaign;

namespace LotSizingDataModel.Checker.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        CliParseResult parseResult =
            CliOptionsParser.Parse(args);

        if (parseResult.ShowHelp)
        {
            CliHelp.Write();
            return (int)CliExitCode.Success;
        }

        if (parseResult.ShowVersion)
        {
            Console.WriteLine(CliHelp.Version);
            return (int)CliExitCode.Success;
        }

        if (!parseResult.Success || parseResult.Options is null)
        {
            Console.Error.WriteLine(
                "checker: " +
                (parseResult.ErrorMessage ?? "Invalid command line."));
            Console.Error.WriteLine("Use 'checker --help' for usage.");
            return (int)CliExitCode.InvalidArguments;
        }

        CliOptions cliOptions =
            parseResult.Options;

        using var cancellationSource =
            new CancellationTokenSource();

        ConsoleCancelEventHandler cancelHandler =
            (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
            };

        Console.CancelKeyPress += cancelHandler;

        CliProgressRenderer? progressRenderer =
            cliOptions.ShowProgress
                ? new CliProgressRenderer()
                : null;

        try
        {
            DirectoryVerificationCampaignOptions campaignOptions =
                cliOptions.BuildCampaignOptions();

            var service =
                new DirectoryVerificationCampaignService();

            DirectoryVerificationCampaignResult result =
                await service.RunAsync(
                        cliOptions.InputDirectory,
                        cliOptions.OutputDirectory,
                        campaignOptions,
                        progressRenderer,
                        cancellationSource.Token)
                    .ConfigureAwait(false);

            progressRenderer?.CompleteLine();

            WriteSummary(result);

            return (int)CliExitCodePolicy.Determine(result);
        }
        catch (OperationCanceledException)
            when (cancellationSource.IsCancellationRequested)
        {
            progressRenderer?.CompleteLine();
            Console.Error.WriteLine("checker: campaign cancelled.");
            return (int)CliExitCode.Cancelled;
        }
        catch (DirectoryNotFoundException exception)
        {
            progressRenderer?.CompleteLine();
            Console.Error.WriteLine("checker: " + exception.Message);
            return (int)CliExitCode.InvalidArguments;
        }
        catch (ArgumentException exception)
        {
            progressRenderer?.CompleteLine();
            Console.Error.WriteLine("checker: " + exception.Message);
            return (int)CliExitCode.InvalidArguments;
        }
        catch (InvalidOperationException exception)
        {
            progressRenderer?.CompleteLine();
            Console.Error.WriteLine("checker: " + exception.Message);
            return (int)CliExitCode.InvalidArguments;
        }
        catch (Exception exception)
        {
            progressRenderer?.CompleteLine();
            Console.Error.WriteLine(
                "checker: unexpected failure: " +
                exception.GetType().FullName +
                ": " +
                exception.Message);
            return (int)CliExitCode.UnexpectedFailure;
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }

    private static void WriteSummary(
        DirectoryVerificationCampaignResult result)
    {
        Console.WriteLine("Lot-sizing checker campaign");
        Console.WriteLine(new string('=', 48));
        Console.WriteLine($"Input files        : {result.DiscoveredXmlFileCount}");
        Console.WriteLine($"Loaded instances   : {result.LoadedInstanceCount}");
        Console.WriteLine($"Candidates         : {result.CandidateCount}");
        Console.WriteLine($"Valid              : {result.BatchResult.ValidCandidateCount}");
        Console.WriteLine($"Invalid            : {result.BatchResult.InvalidCandidateCount}");
        Console.WriteLine($"Execution failures : {result.BatchResult.ExecutionFailureCount}");
        Console.WriteLine($"File load failures : {result.FileLoadFailureCount}");
        Console.WriteLine($"Overall            : {(result.IsValid ? "VALID" : "INVALID")}");

        if (!string.IsNullOrWhiteSpace(result.ReportFiles.SummaryReportPath))
        {
            Console.WriteLine($"Summary report     : {result.ReportFiles.SummaryReportPath}");
        }

        if (!string.IsNullOrWhiteSpace(result.ReportFiles.ManifestPath))
        {
            Console.WriteLine($"Manifest           : {result.ReportFiles.ManifestPath}");
        }
    }
}
