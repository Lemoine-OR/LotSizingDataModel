using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.Contracts;
using LotSizingDataModel.Import.DellaertJeunet.XmlModel;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Imports Dellaert–Jeunet XML benchmark files into the
/// LotSizingDataModel instance representation.
/// </summary>
/// <remarks>
/// This importer coordinates:
/// <list type="number">
/// <item>
/// <description>source-format detection;</description>
/// </item>
/// <item>
/// <description>secure XML deserialization;</description>
/// </item>
/// <item>
/// <description>source-data validation;</description>
/// </item>
/// <item>
/// <description>conversion into the domain model;</description>
/// </item>
/// <item>
/// <description>post-import processing;</description>
/// </item>
/// <item>
/// <description>diagnostic aggregation.</description>
/// </item>
/// </list>
///
/// Streams supplied by the caller are never disposed by this
/// importer.
/// </remarks>
public sealed class DellaertJeunetInstanceImporter :
    IInstanceImporter
{
    private static readonly string[]
        SupportedExtensions =
        {
            ".xml"
        };

    private readonly DellaertJeunetFormatDetector
        _formatDetector;

    private readonly DellaertJeunetXmlReader
        _xmlReader;

    private readonly DellaertJeunetSourceValidator
        _sourceValidator;

    private readonly DellaertJeunetInstanceConverter
        _converter;

    /// <summary>
    /// Initializes an importer using the default services.
    /// </summary>
    public DellaertJeunetInstanceImporter()
        : this(
            new DellaertJeunetFormatDetector(),
            new DellaertJeunetXmlReader(),
            new DellaertJeunetSourceValidator(),
            new DellaertJeunetInstanceConverter())
    {
    }

    /// <summary>
    /// Initializes an importer using explicitly supplied
    /// services.
    /// </summary>
    /// <param name="formatDetector">
    /// Source-format detector.
    /// </param>
    /// <param name="xmlReader">
    /// XML source reader.
    /// </param>
    /// <param name="sourceValidator">
    /// Source-data validator.
    /// </param>
    /// <param name="converter">
    /// Domain-model converter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when one of the supplied services is
    /// <see langword="null"/>.
    /// </exception>
    public DellaertJeunetInstanceImporter(
        DellaertJeunetFormatDetector formatDetector,
        DellaertJeunetXmlReader xmlReader,
        DellaertJeunetSourceValidator sourceValidator,
        DellaertJeunetInstanceConverter converter)
    {
        _formatDetector =
            formatDetector ??
            throw new ArgumentNullException(
                nameof(formatDetector));

        _xmlReader =
            xmlReader ??
            throw new ArgumentNullException(
                nameof(xmlReader));

        _sourceValidator =
            sourceValidator ??
            throw new ArgumentNullException(
                nameof(sourceValidator));

        _converter =
            converter ??
            throw new ArgumentNullException(
                nameof(converter));
    }

    /// <inheritdoc/>
    public InstanceFormat Format =>
        InstanceFormat.DellaertJeunetXml;

    /// <inheritdoc/>
    public string DisplayName =>
        "Dellaert–Jeunet XML importer";

    /// <inheritdoc/>
    public string Version =>
        "1.0.0";

    /// <inheritdoc/>
    public IReadOnlyCollection<string>
        SupportedFileExtensions =>
            SupportedExtensions;

    /// <inheritdoc/>
    public bool CanImport(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "A source file path is required.",
                nameof(filePath));
        }

        if (!File.Exists(
                filePath))
        {
            throw new FileNotFoundException(
                "The source file does not exist.",
                filePath);
        }

        return _formatDetector.IsMatch(
            filePath);
    }

    /// <inheritdoc/>
    public bool CanImport(
        Stream stream,
        string? sourceName = null)
    {
        ArgumentNullException.ThrowIfNull(
            stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException(
                "The source stream must be readable.",
                nameof(stream));
        }

        return _formatDetector.IsMatch(
            stream);
    }

    /// <inheritdoc/>
    public InstanceImportResult Import(
        string filePath,
        InstanceImportOptions? options = null)
    {
        DellaertJeunetImportOptions effectiveOptions =
            BuildOptions(
                options);

        var result =
            CreateResult(
                filePath);

        try
        {
            ValidateFilePath(
                filePath);

            if (effectiveOptions.DetectFormatFromContent &&
                !_formatDetector.IsMatch(
                    filePath))
            {
                result.AddDiagnostic(
                    new ImportDiagnostic(
                        ImportSeverity.Fatal,
                        "DJ5001",
                        "The source file does not appear to " +
                        "use the Dellaert–Jeunet XML format.",
                        filePath));

                return CompleteResult(
                    result,
                    effectiveOptions);
            }

            bool readSucceeded =
                _xmlReader.TryRead(
                    filePath,
                    out DellaertJeunetXmlInstance?
                        sourceInstance,
                    out IReadOnlyList<ImportDiagnostic>
                        readDiagnostics);

            AddDiagnostics(
                result,
                readDiagnostics,
                effectiveOptions);

            if (!readSucceeded ||
                sourceInstance is null)
            {
                return CompleteResult(
                    result,
                    effectiveOptions);
            }

            ProcessSourceInstance(
                sourceInstance,
                effectiveOptions,
                result);

            return CompleteResult(
                result,
                effectiveOptions);
        }
        catch (Exception exception)
            when (!effectiveOptions.ThrowOnError)
        {
            AddUnexpectedException(
                result,
                exception,
                filePath,
                effectiveOptions);

            return CompleteResult(
                result,
                effectiveOptions);
        }
    }

    /// <inheritdoc/>
    public InstanceImportResult Import(
        Stream stream,
        string? sourceName = null,
        InstanceImportOptions? options = null)
    {
        DellaertJeunetImportOptions effectiveOptions =
            BuildOptions(
                options);

        var result =
            CreateResult(
                sourceName);

        try
        {
            ValidateStream(
                stream);

            if (effectiveOptions.DetectFormatFromContent &&
                !_formatDetector.IsMatch(
                    stream))
            {
                result.AddDiagnostic(
                    new ImportDiagnostic(
                        ImportSeverity.Fatal,
                        "DJ5001",
                        "The source stream does not appear to " +
                        "use the Dellaert–Jeunet XML format.",
                        sourceName ??
                        string.Empty));

                return CompleteResult(
                    result,
                    effectiveOptions);
            }

            bool readSucceeded =
                _xmlReader.TryRead(
                    stream,
                    sourceName,
                    out DellaertJeunetXmlInstance?
                        sourceInstance,
                    out IReadOnlyList<ImportDiagnostic>
                        readDiagnostics);

            AddDiagnostics(
                result,
                readDiagnostics,
                effectiveOptions);

            if (!readSucceeded ||
                sourceInstance is null)
            {
                return CompleteResult(
                    result,
                    effectiveOptions);
            }

            ProcessSourceInstance(
                sourceInstance,
                effectiveOptions,
                result);

            return CompleteResult(
                result,
                effectiveOptions);
        }
        catch (Exception exception)
            when (!effectiveOptions.ThrowOnError)
        {
            AddUnexpectedException(
                result,
                exception,
                sourceName,
                effectiveOptions);

            return CompleteResult(
                result,
                effectiveOptions);
        }
    }

    private void ProcessSourceInstance(
        DellaertJeunetXmlInstance sourceInstance,
        DellaertJeunetImportOptions options,
        InstanceImportResult result)
    {
        if (options.ValidateSourceData)
        {
            IReadOnlyList<ImportDiagnostic>
                validationDiagnostics =
                    _sourceValidator.Validate(
                        sourceInstance,
                        options);

            AddDiagnostics(
                result,
                validationDiagnostics,
                options);

            if (result.HasBlockingDiagnostics &&
                !options.AllowPartialImport)
            {
                return;
            }

            if (options.StopOnFirstError &&
                result.HasBlockingDiagnostics)
            {
                return;
            }
        }

        bool conversionSucceeded =
            _converter.TryConvert(
                sourceInstance,
                options,
                out var convertedInstance,
                out IReadOnlyList<ImportDiagnostic>
                    conversionDiagnostics);

        AddDiagnostics(
            result,
            conversionDiagnostics,
            options);

        if (!conversionSucceeded ||
            convertedInstance is null)
        {
            return;
        }

        if (result.HasBlockingDiagnostics &&
            !options.AllowPartialImport)
        {
            return;
        }

        result.Instance =
            convertedInstance;

        AddPostImportDiagnostic(
            result,
            options);
    }

    private static DellaertJeunetImportOptions
        BuildOptions(
            InstanceImportOptions? options)
    {
        if (options is null)
        {
            var defaultOptions =
                new DellaertJeunetImportOptions();

            defaultOptions.EnsureValid();

            return defaultOptions;
        }

        if (options is
            DellaertJeunetImportOptions
                specializedOptions)
        {
            DellaertJeunetImportOptions clone =
                (DellaertJeunetImportOptions)
                specializedOptions.Clone();

            clone.EnsureValid();

            return clone;
        }

        var convertedOptions =
            new DellaertJeunetImportOptions
            {
                AnalyzeProductStructure =
                    options.AnalyzeProductStructure,

                ClassifyProblem =
                    options.ClassifyProblem,

                GenerateMethodRecommendations =
                    options.GenerateMethodRecommendations,

                ValidateSourceData =
                    options.ValidateSourceData,

                ValidateImportedInstance =
                    options.ValidateImportedInstance,

                ThrowOnError =
                    options.ThrowOnError,

                PreserveSourceIdentifiers =
                    options.PreserveSourceIdentifiers,

                IncludeInformationDiagnostics =
                    options.IncludeInformationDiagnostics,

                IncludeTechnicalDetails =
                    options.IncludeTechnicalDetails,

                StopOnFirstError =
                    options.StopOnFirstError,

                AllowPartialImport =
                    options.AllowPartialImport,

                NormalizeTextValues =
                    options.NormalizeTextValues,

                TrimIdentifiers =
                    options.TrimIdentifiers,

                RejectDuplicateIdentifiers =
                    options.RejectDuplicateIdentifiers,

                RejectMissingReferences =
                    options.RejectMissingReferences,

                RejectNegativeValues =
                    options.RejectNegativeValues,

                RejectInvalidTimeSeriesLength =
                    options.RejectInvalidTimeSeriesLength,

                DetectFormatFromContent =
                    options.DetectFormatFromContent,

                SourceName =
                    options.SourceName,

                InstanceIdOverride =
                    options.InstanceIdOverride,

                InstanceNameOverride =
                    options.InstanceNameOverride,

                CreatedByOverride =
                    options.CreatedByOverride,

                SourceInformationOverride =
                    options.SourceInformationOverride
            };

        convertedOptions.EnsureValid();

        return convertedOptions;
    }

    private InstanceImportResult CreateResult(
        string? sourcePath)
    {
        return new InstanceImportResult(
            Format,
            sourcePath ??
            string.Empty)
        {
            ImporterName =
                DisplayName,

            ImporterVersion =
                Version
        };
    }

    private static InstanceImportResult CompleteResult(
        InstanceImportResult result,
        DellaertJeunetImportOptions options)
    {
        result.Complete();

        if (options.ThrowOnError &&
            result.HasBlockingDiagnostics)
        {
            ImportDiagnostic?
                blockingDiagnostic =
                    result.GetFirstBlockingDiagnostic();

            throw new InvalidOperationException(
                blockingDiagnostic?.ToString() ??
                "The Dellaert–Jeunet import failed.");
        }

        return result;
    }

    private static void AddDiagnostics(
        InstanceImportResult result,
        IEnumerable<ImportDiagnostic> diagnostics,
        DellaertJeunetImportOptions options)
    {
        foreach (ImportDiagnostic diagnostic
                 in diagnostics)
        {
            if (diagnostic is null)
            {
                continue;
            }

            if (diagnostic.IsInformation &&
                !options.IncludeInformationDiagnostics)
            {
                continue;
            }

            if (!options.IncludeTechnicalDetails)
            {
                diagnostic.TechnicalDetails =
                    string.Empty;
            }

            result.AddDiagnostic(
                diagnostic);

            if (options.StopOnFirstError &&
                diagnostic.IsBlocking)
            {
                break;
            }
        }
    }

    private static void AddPostImportDiagnostic(
        InstanceImportResult result,
        DellaertJeunetImportOptions options)
    {
        if (!options.IncludeInformationDiagnostics)
        {
            return;
        }

        result.AddDiagnostic(
            ImportDiagnostic.Information(
                "DJ0008",
                "The imported lot-sizing instance is " +
                "available in the import result."));
    }

    private static void AddUnexpectedException(
        InstanceImportResult result,
        Exception exception,
        string? sourcePath,
        DellaertJeunetImportOptions options)
    {
        ImportDiagnostic diagnostic =
            ImportDiagnostic.FromException(
                ImportSeverity.Fatal,
                "DJ5002",
                "An unexpected error occurred during the " +
                "Dellaert–Jeunet import.",
                exception);

        diagnostic.SourcePath =
            sourcePath ??
            string.Empty;

        if (!options.IncludeTechnicalDetails)
        {
            diagnostic.TechnicalDetails =
                string.Empty;
        }

        result.AddDiagnostic(
            diagnostic);
    }

    private static void ValidateFilePath(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "A source file path is required.",
                nameof(filePath));
        }

        if (!File.Exists(
                filePath))
        {
            throw new FileNotFoundException(
                "The source file does not exist.",
                filePath);
        }
    }

    private static void ValidateStream(
        Stream stream)
    {
        ArgumentNullException.ThrowIfNull(
            stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException(
                "The source stream must be readable.",
                nameof(stream));
        }
    }
}