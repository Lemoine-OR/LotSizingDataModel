using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Runs end-to-end verification campaigns over directories containing
/// serialized <see cref="LotSizingInstance"/> files.
/// </summary>
/// <remarks>
/// File discovery and XML loading are performed deterministically before the
/// Package 10 batch checker is invoked. Candidate verification itself then
/// uses the configured bounded parallelism.
/// </remarks>
public sealed class DirectoryVerificationCampaignService
{
    private readonly LotSizingInstanceXmlFileReader _reader;
    private readonly LotSizingSolutionBatchVerificationService _batchService;
    private readonly DirectoryVerificationCampaignReportWriter _reportWriter;

    /// <summary>
    /// Initializes the campaign service with the standard reader, batch
    /// checker and report writer.
    /// </summary>
    public DirectoryVerificationCampaignService()
        : this(
            new LotSizingInstanceXmlFileReader(),
            new LotSizingSolutionBatchVerificationService(),
            new DirectoryVerificationCampaignReportWriter())
    {
    }

    /// <summary>
    /// Initializes the campaign service with explicit dependencies.
    /// </summary>
    /// <param name="reader">Serialized instance reader.</param>
    /// <param name="batchService">Bounded-parallelism checker service.</param>
    /// <param name="reportWriter">Campaign report writer.</param>
    public DirectoryVerificationCampaignService(
        LotSizingInstanceXmlFileReader reader,
        LotSizingSolutionBatchVerificationService batchService,
        DirectoryVerificationCampaignReportWriter reportWriter)
    {
        _reader =
            reader ??
            throw new ArgumentNullException(nameof(reader));

        _batchService =
            batchService ??
            throw new ArgumentNullException(nameof(batchService));

        _reportWriter =
            reportWriter ??
            throw new ArgumentNullException(nameof(reportWriter));
    }

    /// <summary>
    /// Scans a directory, loads serialized instances, selects their detailed
    /// known results, verifies the candidates and optionally writes reports.
    /// </summary>
    /// <param name="inputDirectory">Directory containing serialized instances.</param>
    /// <param name="outputDirectory">
    /// Optional report directory. A relative path is interpreted below the
    /// input directory. When omitted, <c>_checker-reports</c> is used.
    /// </param>
    /// <param name="options">Optional campaign configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A complete campaign result.</returns>
    public Task<DirectoryVerificationCampaignResult> RunAsync(
        string inputDirectory,
        string? outputDirectory = null,
        DirectoryVerificationCampaignOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return RunAsync(
            inputDirectory,
            outputDirectory,
            options,
            progress: null,
            cancellationToken);
    }

    /// <summary>
    /// Scans a directory, verifies selected detailed solutions, optionally
    /// writes reports and emits count-based campaign progress.
    /// </summary>
    /// <param name="inputDirectory">Directory containing serialized instances.</param>
    /// <param name="outputDirectory">Optional report directory.</param>
    /// <param name="options">Optional campaign configuration.</param>
    /// <param name="progress">Optional progress sink.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A complete campaign result.</returns>
    public async Task<DirectoryVerificationCampaignResult> RunAsync(
        string inputDirectory,
        string? outputDirectory,
        DirectoryVerificationCampaignOptions? options,
        IProgress<DirectoryVerificationCampaignProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputDirectory);

        options ??=
            new DirectoryVerificationCampaignOptions();

        options.EnsureValid();
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedInputDirectory =
            Path.GetFullPath(inputDirectory);

        if (!Directory.Exists(normalizedInputDirectory))
        {
            throw new DirectoryNotFoundException(
                "The campaign input directory does not exist: " +
                normalizedInputDirectory);
        }

        string requestedOutputDirectory =
            string.IsNullOrWhiteSpace(outputDirectory)
                ? Path.Combine(
                    normalizedInputDirectory,
                    "_checker-reports")
                : Path.IsPathRooted(outputDirectory)
                    ? outputDirectory
                    : Path.Combine(
                        normalizedInputDirectory,
                        outputDirectory);

        string normalizedOutputDirectory =
            Path.GetFullPath(requestedOutputDirectory);

        if (PathsEqual(
                normalizedInputDirectory,
                normalizedOutputDirectory))
        {
            throw new InvalidOperationException(
                "The report output directory cannot be the campaign input " +
                "directory itself.");
        }

        progress?.Report(
            new DirectoryVerificationCampaignProgress
            {
                Stage =
                    DirectoryVerificationCampaignStage.DiscoveringFiles
            });

        string[] files =
            DiscoverFiles(
                normalizedInputDirectory,
                normalizedOutputDirectory,
                options);

        progress?.Report(
            new DirectoryVerificationCampaignProgress
            {
                Stage =
                    DirectoryVerificationCampaignStage.LoadingFiles,
                DiscoveredFileCount =
                    files.Length
            });

        var loadFailures =
            new List<InstanceFileLoadFailure>();

        var candidates =
            new List<SolutionVerificationBatchCandidate>();

        var sources =
            new List<DirectoryVerificationCandidateSource>();

        int ignoredNonInstanceCount =
            0;

        int loadedInstanceCount =
            0;

        int knownResultCount =
            0;

        int predicateFilteredCount =
            0;

        int withoutDetailedSolutionCount =
            0;

        int processedFileCount =
            0;

        foreach (string filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string relativePath =
                NormalizeRelativePath(
                    Path.GetRelativePath(
                        normalizedInputDirectory,
                        filePath));

            LotSizingInstance? instance =
                null;

            try
            {
                bool isInstanceXml =
                    _reader.HasLotSizingInstanceRoot(filePath);

                if (!isInstanceXml)
                {
                    if (options.IgnoreNonLotSizingInstanceXml)
                    {
                        ignoredNonInstanceCount++;
                        processedFileCount++;
                        ReportLoadingProgress(
                            progress,
                            files.Length,
                            processedFileCount,
                            loadedInstanceCount);
                        continue;
                    }

                    throw new InvalidDataException(
                        "The XML root element is not lotSizingInstance.");
                }

                instance =
                    _reader.Read(filePath);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                loadFailures.Add(
                    CreateLoadFailure(
                        filePath,
                        relativePath,
                        exception));

                processedFileCount++;
                ReportLoadingProgress(
                    progress,
                    files.Length,
                    processedFileCount,
                    loadedInstanceCount);
                continue;
            }

            loadedInstanceCount++;

            for (int knownResultIndex = 0;
                 knownResultIndex < instance.KnownResults.Count;
                 knownResultIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                KnownResult knownResult =
                    instance.KnownResults[knownResultIndex];

                knownResultCount++;

                if (options.KnownResultPredicate is not null &&
                    !options.KnownResultPredicate(knownResult))
                {
                    predicateFilteredCount++;
                    continue;
                }

                if (knownResult.DetailedSolution is null)
                {
                    withoutDetailedSolutionCount++;
                    continue;
                }

                string candidateKey =
                    BuildCandidateKey(
                        relativePath,
                        knownResultIndex,
                        knownResult.ResultId);

                string candidateName =
                    BuildCandidateName(
                        instance,
                        knownResult,
                        knownResultIndex);

                candidates.Add(
                    SolutionVerificationBatchCandidate.ForKnownResult(
                        candidateKey,
                        instance,
                        knownResult,
                        candidateName));

                sources.Add(
                    new DirectoryVerificationCandidateSource
                    {
                        CandidateKey =
                            candidateKey,
                        SourceFilePath =
                            filePath,
                        RelativeSourceFilePath =
                            relativePath,
                        InstanceId =
                            instance.InstanceId,
                        InstanceName =
                            instance.Name,
                        KnownResultIndex =
                            knownResultIndex,
                        KnownResultId =
                            knownResult.ResultId,
                        KnownResultName =
                            knownResult.Name
                    });
            }

            processedFileCount++;
            ReportLoadingProgress(
                progress,
                files.Length,
                processedFileCount,
                loadedInstanceCount);
        }

        cancellationToken.ThrowIfCancellationRequested();

        progress?.Report(
            new DirectoryVerificationCampaignProgress
            {
                Stage =
                    DirectoryVerificationCampaignStage.VerifyingCandidates,
                DiscoveredFileCount =
                    files.Length,
                ProcessedFileCount =
                    processedFileCount,
                LoadedInstanceCount =
                    loadedInstanceCount,
                CandidateCount =
                    candidates.Count
            });

        IProgress<SolutionVerificationBatchProgress>? batchProgress =
            progress is null
                ? null
                : new InlineProgress<SolutionVerificationBatchProgress>(
                    batch =>
                        progress.Report(
                            new DirectoryVerificationCampaignProgress
                            {
                                Stage =
                                    DirectoryVerificationCampaignStage.VerifyingCandidates,
                                DiscoveredFileCount =
                                    files.Length,
                                ProcessedFileCount =
                                    processedFileCount,
                                LoadedInstanceCount =
                                    loadedInstanceCount,
                                CandidateCount =
                                    batch.TotalCandidateCount,
                                CompletedCandidateCount =
                                    batch.CompletedCandidateCount,
                                LastCandidateKey =
                                    batch.LastCompletedCandidateKey
                            }));

        SolutionVerificationBatchResult batchResult =
            await _batchService.VerifyAsync(
                    candidates,
                    options.BatchOptions,
                    batchProgress,
                    cancellationToken)
                .ConfigureAwait(false);

        if (batchResult.Items.Count != sources.Count)
        {
            throw new InvalidOperationException(
                "The batch result count does not match the prepared " +
                "campaign candidate count.");
        }

        var items =
            new List<DirectoryVerificationCampaignItem>(sources.Count);

        for (int index = 0;
             index < sources.Count;
             index++)
        {
            items.Add(
                new DirectoryVerificationCampaignItem
                {
                    Source =
                        sources[index],
                    Verification =
                        batchResult.Items[index]
                });
        }

        var result =
            new DirectoryVerificationCampaignResult
            {
                InputDirectory =
                    normalizedInputDirectory,
                OutputDirectory =
                    normalizedOutputDirectory,
                DiscoveredXmlFileCount =
                    files.Length,
                IgnoredNonInstanceXmlFileCount =
                    ignoredNonInstanceCount,
                LoadedInstanceCount =
                    loadedInstanceCount,
                KnownResultCount =
                    knownResultCount,
                PredicateFilteredKnownResultCount =
                    predicateFilteredCount,
                KnownResultWithoutDetailedSolutionCount =
                    withoutDetailedSolutionCount,
                FileLoadFailures =
                    loadFailures.AsReadOnly(),
                Items =
                    items.AsReadOnly(),
                BatchResult =
                    batchResult
            };

        if (options.WriteReports)
        {
            progress?.Report(
                new DirectoryVerificationCampaignProgress
                {
                    Stage =
                        DirectoryVerificationCampaignStage.WritingReports,
                    DiscoveredFileCount =
                        files.Length,
                    ProcessedFileCount =
                        processedFileCount,
                    LoadedInstanceCount =
                        loadedInstanceCount,
                    CandidateCount =
                        candidates.Count,
                    CompletedCandidateCount =
                        batchResult.CompletedCandidateCount
                });

            result.ReportFiles =
                await _reportWriter.WriteAsync(
                        result,
                        options,
                        cancellationToken)
                    .ConfigureAwait(false);
        }

        progress?.Report(
            new DirectoryVerificationCampaignProgress
            {
                Stage =
                    DirectoryVerificationCampaignStage.Completed,
                DiscoveredFileCount =
                    files.Length,
                ProcessedFileCount =
                    processedFileCount,
                LoadedInstanceCount =
                    loadedInstanceCount,
                CandidateCount =
                    candidates.Count,
                CompletedCandidateCount =
                    batchResult.CompletedCandidateCount
            });

        return result;
    }

    private static void ReportLoadingProgress(
        IProgress<DirectoryVerificationCampaignProgress>? progress,
        int discoveredFileCount,
        int processedFileCount,
        int loadedInstanceCount)
    {
        progress?.Report(
            new DirectoryVerificationCampaignProgress
            {
                Stage =
                    DirectoryVerificationCampaignStage.LoadingFiles,
                DiscoveredFileCount =
                    discoveredFileCount,
                ProcessedFileCount =
                    processedFileCount,
                LoadedInstanceCount =
                    loadedInstanceCount
            });
    }

    private sealed class InlineProgress<T> : IProgress<T>
    {
        private readonly Action<T> _report;

        public InlineProgress(Action<T> report)
        {
            _report =
                report ??
                throw new ArgumentNullException(nameof(report));
        }

        public void Report(T value)
        {
            _report(value);
        }
    }

    private static InstanceFileLoadFailure CreateLoadFailure(
        string filePath,
        string relativePath,
        Exception exception)
    {
        return new InstanceFileLoadFailure
        {
            FilePath =
                filePath,
            RelativeFilePath =
                relativePath,
            ExceptionType =
                exception.GetType().FullName ??
                exception.GetType().Name,
            Message =
                exception.Message
        };
    }

    private static string[] DiscoverFiles(
        string inputDirectory,
        string outputDirectory,
        DirectoryVerificationCampaignOptions options)
    {
        SearchOption searchOption =
            options.SearchSubdirectories
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

        bool outputIsInsideInput =
            IsDirectoryInsideDirectory(
                outputDirectory,
                inputDirectory);

        return Directory
            .EnumerateFiles(
                inputDirectory,
                options.SearchPattern,
                searchOption)
            .Select(Path.GetFullPath)
            .Where(
                filePath =>
                    !outputIsInsideInput ||
                    !IsPathInsideDirectory(
                        filePath,
                        outputDirectory))
            .OrderBy(
                filePath =>
                    NormalizeRelativePath(
                        Path.GetRelativePath(
                            inputDirectory,
                            filePath)),
                StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsDirectoryInsideDirectory(
        string candidateDirectory,
        string parentDirectory)
    {
        string normalizedCandidate =
            EnsureTrailingDirectorySeparator(
                Path.GetFullPath(candidateDirectory));

        string normalizedParent =
            EnsureTrailingDirectorySeparator(
                Path.GetFullPath(parentDirectory));

        StringComparison comparison =
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        return normalizedCandidate.StartsWith(
            normalizedParent,
            comparison);
    }

    private static bool IsPathInsideDirectory(
        string filePath,
        string directoryPath)
    {
        string normalizedFilePath =
            Path.GetFullPath(filePath);

        string normalizedDirectoryPath =
            EnsureTrailingDirectorySeparator(
                Path.GetFullPath(directoryPath));

        StringComparison comparison =
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        return normalizedFilePath.StartsWith(
            normalizedDirectoryPath,
            comparison);
    }

    private static bool PathsEqual(
        string first,
        string second)
    {
        StringComparison comparison =
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        return string.Equals(
            Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(second)),
            comparison);
    }

    private static string EnsureTrailingDirectorySeparator(
        string directoryPath)
    {
        if (directoryPath.EndsWith(
                Path.DirectorySeparatorChar) ||
            directoryPath.EndsWith(
                Path.AltDirectorySeparatorChar))
        {
            return directoryPath;
        }

        return directoryPath +
            Path.DirectorySeparatorChar;
    }

    private static string NormalizeRelativePath(
        string relativePath)
    {
        return relativePath
            .Replace('\\', '/')
            .Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string BuildCandidateKey(
        string relativePath,
        int knownResultIndex,
        string? resultId)
    {
        string stableResultId =
            string.IsNullOrWhiteSpace(resultId)
                ? "no-result-id"
                : resultId;

        return
            relativePath +
            "::knownResult[" +
            knownResultIndex.ToString("D6") +
            "]::" +
            stableResultId;
    }

    private static string BuildCandidateName(
        LotSizingInstance instance,
        KnownResult knownResult,
        int knownResultIndex)
    {
        string instanceLabel =
            string.IsNullOrWhiteSpace(instance.Name)
                ? instance.InstanceId
                : instance.Name;

        string resultLabel =
            !string.IsNullOrWhiteSpace(knownResult.Name)
                ? knownResult.Name
                : !string.IsNullOrWhiteSpace(knownResult.ResultId)
                    ? knownResult.ResultId
                    : "KnownResult " +
                      knownResultIndex.ToString();

        return
            instanceLabel +
            " / " +
            resultLabel;
    }
}
