using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.Contracts;
using LotSizingDataModel.Import.DellaertJeunet;

namespace LotSizingDataModel.Import.Services;

/// <summary>
/// Stores and resolves the instance importers available to an
/// application.
/// </summary>
/// <remarks>
/// The registry associates one importer with one
/// <see cref="InstanceFormat"/>.
///
/// It supports two resolution strategies:
/// <list type="bullet">
/// <item>
/// <description>
/// explicit resolution from a known format;
/// </description>
/// </item>
/// <item>
/// <description>
/// automatic source inspection through
/// <see cref="IInstanceImporter.CanImport(string)"/> or
/// <see cref="IInstanceImporter.CanImport(Stream, string?)"/>.
/// </description>
/// </item>
/// </list>
///
/// The registry does not perform the import itself. Import
/// orchestration is delegated to
/// <see cref="InstanceImportService"/>.
/// </remarks>
public sealed class InstanceImporterRegistry
{
    private readonly Dictionary<
        InstanceFormat,
        IInstanceImporter> _importers =
            new();

    /// <summary>
    /// Initializes an empty importer registry.
    /// </summary>
    public InstanceImporterRegistry()
    {
    }

    /// <summary>
    /// Initializes a registry with the supplied importers.
    /// </summary>
    /// <param name="importers">
    /// Importers to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="importers"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null importer or
    /// several importers for the same format.
    /// </exception>
    public InstanceImporterRegistry(
        IEnumerable<IInstanceImporter> importers)
    {
        ArgumentNullException.ThrowIfNull(
            importers);

        foreach (IInstanceImporter importer
                 in importers)
        {
            Register(
                importer);
        }
    }

    /// <summary>
    /// Gets the number of registered importers.
    /// </summary>
    public int Count =>
        _importers.Count;

    /// <summary>
    /// Gets a value indicating whether at least one importer is
    /// registered.
    /// </summary>
    public bool HasImporters =>
        Count > 0;

    /// <summary>
    /// Gets the registered formats in ascending enumeration
    /// order.
    /// </summary>
    public IReadOnlyList<InstanceFormat>
        RegisteredFormats =>
            _importers.Keys
                .OrderBy(
                    format =>
                        format)
                .ToArray();

    /// <summary>
    /// Gets the registered importers ordered by format.
    /// </summary>
    public IReadOnlyList<IInstanceImporter>
        Importers =>
            _importers
                .OrderBy(
                    pair =>
                        pair.Key)
                .Select(
                    pair =>
                        pair.Value)
                .ToArray();

    /// <summary>
    /// Creates a registry containing the importers supplied by
    /// the LotSizingDataModel import library.
    /// </summary>
    /// <returns>
    /// Registry containing the standard importers.
    /// </returns>
    public static InstanceImporterRegistry CreateDefault()
    {
        return new InstanceImporterRegistry(
            new IInstanceImporter[]
            {
                new DellaertJeunetInstanceImporter()
            });
    }

    /// <summary>
    /// Registers an importer.
    /// </summary>
    /// <param name="importer">
    /// Importer to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="importer"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the importer declares
    /// <see cref="InstanceFormat.Unknown"/> or when another
    /// importer is already registered for the same format.
    /// </exception>
    public void Register(
        IInstanceImporter importer)
    {
        ArgumentNullException.ThrowIfNull(
            importer);

        ValidateImporter(
            importer);

        if (_importers.ContainsKey(
                importer.Format))
        {
            throw new ArgumentException(
                $"An importer is already registered for " +
                $"format '{importer.Format}'.",
                nameof(importer));
        }

        _importers.Add(
            importer.Format,
            importer);
    }

    /// <summary>
    /// Registers an importer or replaces the existing importer
    /// associated with the same format.
    /// </summary>
    /// <param name="importer">
    /// Importer to register.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="importer"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the importer declares
    /// <see cref="InstanceFormat.Unknown"/>.
    /// </exception>
    public void RegisterOrReplace(
        IInstanceImporter importer)
    {
        ArgumentNullException.ThrowIfNull(
            importer);

        ValidateImporter(
            importer);

        _importers[importer.Format] =
            importer;
    }

    /// <summary>
    /// Determines whether an importer is registered for the
    /// supplied format.
    /// </summary>
    /// <param name="format">
    /// Format to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a matching importer is
    /// registered; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        InstanceFormat format)
    {
        return _importers.ContainsKey(
            format);
    }

    /// <summary>
    /// Returns the importer registered for the supplied format.
    /// </summary>
    /// <param name="format">
    /// Source instance format.
    /// </param>
    /// <returns>
    /// Matching importer.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="format"/> is
    /// <see cref="InstanceFormat.Unknown"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no importer is registered for the supplied
    /// format.
    /// </exception>
    public IInstanceImporter GetRequiredImporter(
        InstanceFormat format)
    {
        if (format ==
            InstanceFormat.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                "A concrete instance format is required.");
        }

        if (_importers.TryGetValue(
                format,
                out IInstanceImporter? importer))
        {
            return importer;
        }

        throw new KeyNotFoundException(
            $"No importer is registered for format " +
            $"'{format}'.");
    }

    /// <summary>
    /// Attempts to retrieve the importer registered for the
    /// supplied format.
    /// </summary>
    /// <param name="format">
    /// Source instance format.
    /// </param>
    /// <param name="importer">
    /// Matching importer when found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when an importer is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetImporter(
        InstanceFormat format,
        out IInstanceImporter? importer)
    {
        return _importers.TryGetValue(
            format,
            out importer);
    }

    /// <summary>
    /// Removes the importer associated with the supplied
    /// format.
    /// </summary>
    /// <param name="format">
    /// Format whose importer must be removed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when an importer was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(
        InstanceFormat format)
    {
        return _importers.Remove(
            format);
    }

    /// <summary>
    /// Removes every registered importer.
    /// </summary>
    public void Clear()
    {
        _importers.Clear();
    }

    /// <summary>
    /// Detects the importer compatible with the supplied file.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <returns>
    /// First compatible importer, or
    /// <see langword="null"/> when none matches.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the source file does not exist.
    /// </exception>
    public IInstanceImporter? DetectImporter(
        string filePath)
    {
        ValidateFilePath(
            filePath);

        IReadOnlyList<IInstanceImporter> candidates =
            GetCandidatesForFile(
                filePath);

        foreach (IInstanceImporter importer
                 in candidates)
        {
            if (CanImporterReadFile(
                    importer,
                    filePath))
            {
                return importer;
            }
        }

        return null;
    }

    /// <summary>
    /// Detects the importer compatible with the supplied
    /// stream.
    /// </summary>
    /// <param name="stream">
    /// Readable source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name, such as a file name.
    /// </param>
    /// <returns>
    /// First compatible importer, or
    /// <see langword="null"/> when none matches.
    /// </returns>
    /// <remarks>
    /// Automatic detection on a non-seekable stream should be
    /// used with care when several importers are registered.
    ///
    /// Importers are required not to dispose caller-owned
    /// streams.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    public IInstanceImporter? DetectImporter(
        Stream stream,
        string? sourceName = null)
    {
        ValidateStream(
            stream);

        IReadOnlyList<IInstanceImporter> candidates =
            GetCandidatesForSourceName(
                sourceName);

        foreach (IInstanceImporter importer
                 in candidates)
        {
            long? initialPosition =
                GetCurrentPosition(
                    stream);

            try
            {
                if (importer.CanImport(
                        stream,
                        sourceName))
                {
                    return importer;
                }
            }
            catch (
                Exception exception)
                when (
                    exception is
                        InvalidDataException or
                    IOException or
                    UnauthorizedAccessException)
            {
                // A detection failure means that this importer
                // is not selected. Another importer may still
                // recognize the source.
            }
            finally
            {
                RestorePosition(
                    stream,
                    initialPosition);
            }

            if (!stream.CanSeek)
            {
                break;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns all importers that identify the supplied file
    /// as compatible.
    /// </summary>
    /// <param name="filePath">
    /// Path of the source file.
    /// </param>
    /// <returns>
    /// Compatible importers.
    /// </returns>
    public IReadOnlyList<IInstanceImporter>
        DetectAllImporters(
            string filePath)
    {
        ValidateFilePath(
            filePath);

        var matches =
            new List<IInstanceImporter>();

        foreach (IInstanceImporter importer
                 in GetCandidatesForFile(
                     filePath))
        {
            if (CanImporterReadFile(
                    importer,
                    filePath))
            {
                matches.Add(
                    importer);
            }
        }

        return matches;
    }

    /// <summary>
    /// Returns all importers whose declared file extensions
    /// contain the supplied extension.
    /// </summary>
    /// <param name="extension">
    /// File extension, with or without a leading period.
    /// </param>
    /// <returns>
    /// Matching importers.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="extension"/> is empty.
    /// </exception>
    public IReadOnlyList<IInstanceImporter>
        FindByExtension(
            string extension)
    {
        if (string.IsNullOrWhiteSpace(
                extension))
        {
            throw new ArgumentException(
                "A file extension is required.",
                nameof(extension));
        }

        string normalizedExtension =
            NormalizeExtension(
                extension);

        return Importers
            .Where(
                importer =>
                    importer
                        .SupportedFileExtensions
                        .Any(
                            supportedExtension =>
                                string.Equals(
                                    NormalizeExtension(
                                        supportedExtension),
                                    normalizedExtension,
                                    StringComparison
                                        .OrdinalIgnoreCase)))
            .ToArray();
    }

    private IReadOnlyList<IInstanceImporter>
        GetCandidatesForFile(
            string filePath)
    {
        string extension =
            Path.GetExtension(
                filePath);

        IReadOnlyList<IInstanceImporter> extensionMatches =
            string.IsNullOrWhiteSpace(
                extension)
                ? Array.Empty<IInstanceImporter>()
                : FindByExtension(
                    extension);

        if (extensionMatches.Count == 0)
        {
            return Importers;
        }

        return extensionMatches
            .Concat(
                Importers.Where(
                    importer =>
                        !extensionMatches.Contains(
                            importer)))
            .ToArray();
    }

    private IReadOnlyList<IInstanceImporter>
        GetCandidatesForSourceName(
            string? sourceName)
    {
        if (string.IsNullOrWhiteSpace(
                sourceName))
        {
            return Importers;
        }

        string extension =
            Path.GetExtension(
                sourceName);

        if (string.IsNullOrWhiteSpace(
                extension))
        {
            return Importers;
        }

        IReadOnlyList<IInstanceImporter> extensionMatches =
            FindByExtension(
                extension);

        if (extensionMatches.Count == 0)
        {
            return Importers;
        }

        return extensionMatches
            .Concat(
                Importers.Where(
                    importer =>
                        !extensionMatches.Contains(
                            importer)))
            .ToArray();
    }

    private static bool CanImporterReadFile(
        IInstanceImporter importer,
        string filePath)
    {
        try
        {
            return importer.CanImport(
                filePath);
        }
        catch (
            Exception exception)
            when (
                exception is
                    InvalidDataException or
                IOException or
                UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void ValidateImporter(
        IInstanceImporter importer)
    {
        if (importer.Format ==
            InstanceFormat.Unknown)
        {
            throw new ArgumentException(
                "An importer cannot be registered for the " +
                "Unknown format.",
                nameof(importer));
        }

        if (string.IsNullOrWhiteSpace(
                importer.DisplayName))
        {
            throw new ArgumentException(
                "The importer display name is required.",
                nameof(importer));
        }

        if (string.IsNullOrWhiteSpace(
                importer.Version))
        {
            throw new ArgumentException(
                "The importer version is required.",
                nameof(importer));
        }

        if (importer.SupportedFileExtensions is null)
        {
            throw new ArgumentException(
                "The importer file-extension collection " +
                "cannot be null.",
                nameof(importer));
        }

        foreach (string extension
                 in importer.SupportedFileExtensions)
        {
            if (string.IsNullOrWhiteSpace(
                    extension))
            {
                throw new ArgumentException(
                    "The importer file-extension collection " +
                    "cannot contain an empty value.",
                    nameof(importer));
            }
        }
    }

    private static string NormalizeExtension(
        string extension)
    {
        string trimmedExtension =
            extension.Trim();

        return trimmedExtension.StartsWith(
                '.')
            ? trimmedExtension
            : "." + trimmedExtension;
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

    private static long? GetCurrentPosition(
        Stream stream)
    {
        return
            stream.CanSeek
                ? stream.Position
                : null;
    }

    private static void RestorePosition(
        Stream stream,
        long? initialPosition)
    {
        if (!initialPosition.HasValue ||
            !stream.CanSeek)
        {
            return;
        }

        stream.Position =
            initialPosition.Value;
    }
}