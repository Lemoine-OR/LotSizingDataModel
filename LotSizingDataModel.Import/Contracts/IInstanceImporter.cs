using System;
using System.Collections.Generic;
using System.IO;
using LotSizingDataModel.Import.Common;

namespace LotSizingDataModel.Import.Contracts;

/// <summary>
/// Defines the common contract implemented by all external
/// lot-sizing instance importers.
/// </summary>
/// <remarks>
/// Each importer is responsible for one specific source
/// format or one compatible family of source formats.
///
/// Implementations must:
/// <list type="bullet">
/// <item>
/// <description>
/// identify the format they support;
/// </description>
/// </item>
/// <item>
/// <description>
/// determine whether a source appears compatible with that
/// format;
/// </description>
/// </item>
/// <item>
/// <description>
/// convert the source data into a
/// <see cref="LotSizingDataModel.Instance.LotSizingInstance"/>;
/// </description>
/// </item>
/// <item>
/// <description>
/// return all relevant diagnostics through an
/// <see cref="InstanceImportResult"/>.
/// </description>
/// </item>
/// </list>
///
/// Importers must not close streams supplied by the caller.
/// The caller remains responsible for disposing such streams.
/// </remarks>
public interface IInstanceImporter
{
    /// <summary>
    /// Gets the external instance format supported by this
    /// importer.
    /// </summary>
    InstanceFormat Format { get; }

    /// <summary>
    /// Gets the human-readable importer name.
    /// </summary>
    /// <remarks>
    /// This value is intended for diagnostics, logs and user
    /// interfaces.
    /// </remarks>
    string DisplayName { get; }

    /// <summary>
    /// Gets the importer implementation version.
    /// </summary>
    /// <remarks>
    /// This version identifies the import logic rather than
    /// the version of the source benchmark format.
    /// </remarks>
    string Version { get; }

    /// <summary>
    /// Gets the file extensions commonly associated with the
    /// supported format.
    /// </summary>
    /// <remarks>
    /// Extensions are hints only. They must not be treated as
    /// sufficient proof that a source is compatible.
    ///
    /// Values should include the leading period, for example
    /// <c>.xml</c>, <c>.json</c> or <c>.csv</c>.
    /// </remarks>
    IReadOnlyCollection<string> SupportedFileExtensions
    {
        get;
    }

    /// <summary>
    /// Determines whether the supplied file appears compatible
    /// with this importer.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file to inspect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the file appears compatible
    /// with the supported format; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the specified file does not exist.
    /// </exception>
    bool CanImport(
        string filePath);

    /// <summary>
    /// Determines whether the supplied stream appears
    /// compatible with this importer.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream to inspect.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name used to provide contextual
    /// information, such as a file name or URI.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the stream appears
    /// compatible with the supported format; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// When the stream supports seeking, implementations
    /// should restore its original position before returning.
    ///
    /// Implementations must not dispose the supplied stream.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    bool CanImport(
        Stream stream,
        string? sourceName = null);

    /// <summary>
    /// Imports a lot-sizing instance from a file.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file to import.
    /// </param>
    /// <param name="options">
    /// Optional import settings. When
    /// <see langword="null"/>, the importer must use its
    /// recommended default options.
    /// </param>
    /// <returns>
    /// Complete import result containing the imported instance
    /// and all generated diagnostics.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the specified file does not exist and the
    /// importer is configured to throw on errors.
    /// </exception>
    InstanceImportResult Import(
        string filePath,
        InstanceImportOptions? options = null);

    /// <summary>
    /// Imports a lot-sizing instance from a stream.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name used in diagnostics and import
    /// metadata.
    /// </param>
    /// <param name="options">
    /// Optional import settings. When
    /// <see langword="null"/>, the importer must use its
    /// recommended default options.
    /// </param>
    /// <returns>
    /// Complete import result containing the imported instance
    /// and all generated diagnostics.
    /// </returns>
    /// <remarks>
    /// The importer must not dispose the supplied stream.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    InstanceImportResult Import(
        Stream stream,
        string? sourceName = null,
        InstanceImportOptions? options = null);
}