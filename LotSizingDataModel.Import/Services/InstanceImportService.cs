using System;
using System.IO;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.Contracts;

namespace LotSizingDataModel.Import.Services;

/// <summary>
/// Provides the high-level entry point for importing external
/// lot-sizing instances.
/// </summary>
/// <remarks>
/// The service relies on an
/// <see cref="InstanceImporterRegistry"/> to resolve the
/// appropriate importer.
///
/// An importer may be selected:
/// <list type="bullet">
/// <item>
/// <description>
/// explicitly, by supplying an
/// <see cref="InstanceFormat"/>;
/// </description>
/// </item>
/// <item>
/// <description>
/// automatically, by inspecting the source content.
/// </description>
/// </item>
/// </list>
///
/// The service does not modify the behavior of the selected
/// importer. All import options are forwarded to it.
/// </remarks>
public sealed class InstanceImportService
{
    private readonly InstanceImporterRegistry _registry;

    /// <summary>
    /// Initializes an import service using the standard
    /// importer registry.
    /// </summary>
    public InstanceImportService()
        : this(
            InstanceImporterRegistry.CreateDefault())
    {
    }

    /// <summary>
    /// Initializes an import service using the supplied
    /// importer registry.
    /// </summary>
    /// <param name="registry">
    /// Registry containing the available importers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registry"/> is
    /// <see langword="null"/>.
    /// </exception>
    public InstanceImportService(
        InstanceImporterRegistry registry)
    {
        _registry =
            registry ??
            throw new ArgumentNullException(
                nameof(registry));
    }

    /// <summary>
    /// Gets the importer registry used by this service.
    /// </summary>
    public InstanceImporterRegistry Registry =>
        _registry;

    /// <summary>
    /// Imports an instance from a file by automatically
    /// detecting its format.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <param name="options">
    /// Optional import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// When no compatible importer is found, the method
    /// returns a failed import result containing a fatal
    /// diagnostic.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the source file does not exist.
    /// </exception>
    public InstanceImportResult Import(
        string filePath,
        InstanceImportOptions? options = null)
    {
        ValidateFilePath(
            filePath);

        IInstanceImporter? importer =
            _registry.DetectImporter(
                filePath);

        if (importer is null)
        {
            return CreateUnsupportedFormatResult(
                filePath,
                options);
        }

        return importer.Import(
            filePath,
            options);
    }

    /// <summary>
    /// Imports an instance from a file using an explicitly
    /// selected source format.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <param name="format">
    /// Source instance format.
    /// </param>
    /// <param name="options">
    /// Optional import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the source file does not exist.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="format"/> is
    /// <see cref="InstanceFormat.Unknown"/>.
    /// </exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// Thrown when no importer is registered for the supplied
    /// format.
    /// </exception>
    public InstanceImportResult Import(
        string filePath,
        InstanceFormat format,
        InstanceImportOptions? options = null)
    {
        ValidateFilePath(
            filePath);

        IInstanceImporter importer =
            _registry.GetRequiredImporter(
                format);

        return importer.Import(
            filePath,
            options);
    }

    /// <summary>
    /// Imports an instance from a stream by automatically
    /// detecting its format.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name, such as a file name.
    /// </param>
    /// <param name="options">
    /// Optional import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// Automatic detection requires a seekable stream when
    /// several importers may inspect the same source.
    ///
    /// The supplied stream is never disposed by this service.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    public InstanceImportResult Import(
        Stream stream,
        string? sourceName = null,
        InstanceImportOptions? options = null)
    {
        ValidateStream(
            stream);

        IInstanceImporter? importer =
            _registry.DetectImporter(
                stream,
                sourceName);

        if (importer is null)
        {
            return CreateUnsupportedFormatResult(
                sourceName,
                options);
        }

        return importer.Import(
            stream,
            sourceName,
            options);
    }

    /// <summary>
    /// Imports an instance from a stream using an explicitly
    /// selected source format.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="format">
    /// Source instance format.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name.
    /// </param>
    /// <param name="options">
    /// Optional import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this service.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="format"/> is
    /// <see cref="InstanceFormat.Unknown"/>.
    /// </exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// Thrown when no importer is registered for the supplied
    /// format.
    /// </exception>
    public InstanceImportResult Import(
        Stream stream,
        InstanceFormat format,
        string? sourceName = null,
        InstanceImportOptions? options = null)
    {
        ValidateStream(
            stream);

        IInstanceImporter importer =
            _registry.GetRequiredImporter(
                format);

        return importer.Import(
            stream,
            sourceName,
            options);
    }

    /// <summary>
    /// Determines whether at least one registered importer can
    /// process the supplied file.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a compatible importer is
    /// found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanImport(
        string filePath)
    {
        ValidateFilePath(
            filePath);

        return
            _registry.DetectImporter(
                filePath) is not null;
    }

    /// <summary>
    /// Determines whether at least one registered importer can
    /// process the supplied stream.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a compatible importer is
    /// found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanImport(
        Stream stream,
        string? sourceName = null)
    {
        ValidateStream(
            stream);

        return
            _registry.DetectImporter(
                stream,
                sourceName) is not null;
    }

    /// <summary>
    /// Returns the importer that would be selected for the
    /// supplied file.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <returns>
    /// Detected importer, or <see langword="null"/> when no
    /// importer recognizes the source.
    /// </returns>
    public IInstanceImporter? DetectImporter(
        string filePath)
    {
        ValidateFilePath(
            filePath);

        return _registry.DetectImporter(
            filePath);
    }

    /// <summary>
    /// Returns the importer that would be selected for the
    /// supplied stream.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name.
    /// </param>
    /// <returns>
    /// Detected importer, or <see langword="null"/> when no
    /// importer recognizes the source.
    /// </returns>
    public IInstanceImporter? DetectImporter(
        Stream stream,
        string? sourceName = null)
    {
        ValidateStream(
            stream);

        return _registry.DetectImporter(
            stream,
            sourceName);
    }

    private static InstanceImportResult
        CreateUnsupportedFormatResult(
            string? sourcePath,
            InstanceImportOptions? options)
    {
        var result =
            new InstanceImportResult(
                InstanceFormat.Unknown,
                sourcePath ??
                string.Empty)
            {
                ImporterName =
                    "Instance import service",

                ImporterVersion =
                    "1.0.0"
            };

        result.AddDiagnostic(
            new ImportDiagnostic(
                ImportSeverity.Fatal,
                "IMP0001",
                "No registered importer recognizes the " +
                "source instance format.",
                sourcePath ??
                string.Empty));

        result.Complete();

        if (options?.ThrowOnError == true)
        {
            throw new InvalidOperationException(
                "No registered importer recognizes the " +
                "source instance format.");
        }

        return result;
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