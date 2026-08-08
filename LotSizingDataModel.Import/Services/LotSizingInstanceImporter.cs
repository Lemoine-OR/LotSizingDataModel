using System;
using System.IO;
using LotSizingDataModel.Import.Common;

namespace LotSizingDataModel.Import.Services;

/// <summary>
/// Provides simplified static entry points for importing
/// external lot-sizing instances.
/// </summary>
/// <remarks>
/// This facade is intended for common application scenarios
/// where the standard importer registry is sufficient.
///
/// Each operation uses the importers registered by
/// <see cref="InstanceImporterRegistry.CreateDefault"/>.
///
/// Applications requiring custom importers, dependency
/// injection or persistent service configuration should use
/// <see cref="InstanceImportService"/> directly.
/// </remarks>
public static class LotSizingInstanceImporter
{
    private static readonly Lazy<InstanceImportService>
        DefaultService =
            new(
                CreateDefaultService,
                isThreadSafe:
                    true);

    /// <summary>
    /// Imports an instance from a file by automatically
    /// detecting its format.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
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
    public static InstanceImportResult Import(
        string filePath)
    {
        return Service.Import(
            filePath);
    }

    /// <summary>
    /// Imports an instance from a file by automatically
    /// detecting its format.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <param name="options">
    /// Import settings.
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
    public static InstanceImportResult Import(
        string filePath,
        InstanceImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        return Service.Import(
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
    /// <returns>
    /// Complete import result.
    /// </returns>
    public static InstanceImportResult Import(
        string filePath,
        InstanceFormat format)
    {
        return Service.Import(
            filePath,
            format);
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
    /// Import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    public static InstanceImportResult Import(
        string filePath,
        InstanceFormat format,
        InstanceImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        return Service.Import(
            filePath,
            format,
            options);
    }

    /// <summary>
    /// Imports an instance from a stream by automatically
    /// detecting its format.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static InstanceImportResult Import(
        Stream stream)
    {
        return Service.Import(
            stream);
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
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static InstanceImportResult Import(
        Stream stream,
        string? sourceName)
    {
        return Service.Import(
            stream,
            sourceName);
    }

    /// <summary>
    /// Imports an instance from a stream by automatically
    /// detecting its format.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name.
    /// </param>
    /// <param name="options">
    /// Import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static InstanceImportResult Import(
        Stream stream,
        string? sourceName,
        InstanceImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        return Service.Import(
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
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static InstanceImportResult Import(
        Stream stream,
        InstanceFormat format)
    {
        return Service.Import(
            stream,
            format);
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
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static InstanceImportResult Import(
        Stream stream,
        InstanceFormat format,
        string? sourceName)
    {
        return Service.Import(
            stream,
            format,
            sourceName);
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
    /// Import settings.
    /// </param>
    /// <returns>
    /// Complete import result.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static InstanceImportResult Import(
        Stream stream,
        InstanceFormat format,
        string? sourceName,
        InstanceImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        return Service.Import(
            stream,
            format,
            sourceName,
            options);
    }

    /// <summary>
    /// Determines whether the supplied file can be imported by
    /// one of the standard importers.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a compatible importer is
    /// available; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool CanImport(
        string filePath)
    {
        return Service.CanImport(
            filePath);
    }

    /// <summary>
    /// Determines whether the supplied stream can be imported
    /// by one of the standard importers.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a compatible importer is
    /// available; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed by this method.
    /// </remarks>
    public static bool CanImport(
        Stream stream,
        string? sourceName = null)
    {
        return Service.CanImport(
            stream,
            sourceName);
    }

    /// <summary>
    /// Gets the shared standard import service used by the
    /// static facade.
    /// </summary>
    /// <remarks>
    /// Applications should not modify the registry exposed by
    /// this service. Applications requiring a customized
    /// registry should create their own
    /// <see cref="InstanceImportService"/>.
    /// </remarks>
    public static InstanceImportService Service =>
        DefaultService.Value;

    private static InstanceImportService
        CreateDefaultService()
    {
        return new InstanceImportService(
            InstanceImporterRegistry.CreateDefault());
    }
}